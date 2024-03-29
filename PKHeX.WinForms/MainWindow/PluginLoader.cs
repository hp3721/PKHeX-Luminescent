﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using static PKHeX.WinForms.PluginLoadSetting;

namespace PKHeX.WinForms;

public static class PluginLoader
{
    public static IEnumerable<T> LoadPlugins<T>(string pluginPath, PluginLoadSetting loadSetting) where T : class
    {
        var dllFileNames = !Directory.Exists(pluginPath)
            ? Array.Empty<string>() // Don't immediately return, as we may be loading plugins merged with this .exe
            : Directory.EnumerateFiles(pluginPath, "*.dll", SearchOption.AllDirectories);
        var assemblies = GetAssemblies(dllFileNames, loadSetting);
        var pluginTypes = GetPluginsOfType<T>(assemblies);
        return LoadPlugins<T>(pluginTypes);
    }

    private static IEnumerable<T> LoadPlugins<T>(IEnumerable<Type> pluginTypes) where T : class
    {
        foreach (var t in pluginTypes)
        {
            T? activate;
            try { activate = (T?)Activator.CreateInstance(t); }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to load plugin [{t.Name}]: {t.FullName}");
                Debug.WriteLine(ex.Message);
                continue;
            }
            if (activate != null)
                yield return activate;
        }
    }

    private static IEnumerable<Assembly> GetAssemblies(IEnumerable<string> dllFileNames, PluginLoadSetting loadSetting)
    {
        var loadMethod = GetPluginLoadMethod(loadSetting);
        var assemblies = dllFileNames.Select(loadMethod);
        if (loadSetting.IsMerged())
            assemblies = assemblies.Concat(new[] { Assembly.GetExecutingAssembly() }); // load merged too
        return assemblies;
    }

    private static Func<string, Assembly> GetPluginLoadMethod(PluginLoadSetting pls) => pls switch
    {
        LoadFrom or LoadFromMerged => Assembly.LoadFrom,
        LoadFile or LoadFileMerged => Assembly.LoadFile,
        UnsafeLoadFrom or UnsafeMerged => Assembly.UnsafeLoadFrom,
        _ => throw new IndexOutOfRangeException($"PluginLoadSetting: {pls} method not defined."),
    };

    public static bool IsMerged(this PluginLoadSetting loadSetting) => loadSetting is LoadFromMerged or LoadFileMerged or UnsafeMerged;

    private static IEnumerable<Type> GetPluginsOfType<T>(IEnumerable<Assembly> assemblies)
    {
        var interfaceTypeName = typeof(T).FullName;
        if (interfaceTypeName is null)
            return Array.Empty<Type>();
        return assemblies.SelectMany(z => GetPluginTypes(z, interfaceTypeName));
    }

    private static IEnumerable<Type> GetPluginTypes(Assembly z, string interfaceTypeName)
    {
        try
        {
            // Handle Costura merged plugin dll's; need to Attach for them to correctly retrieve their dependencies.
            var assemblyLoaderType = z.GetType("Costura.AssemblyLoader", false);
            var attachMethod = assemblyLoaderType?.GetMethod("Attach", BindingFlags.Static | BindingFlags.Public);
            attachMethod?.Invoke(null, Array.Empty<object>());

            var types = z.GetTypes();
            return types.Where(type => IsTypePlugin(type, interfaceTypeName));
        }
        // User plugins can be out of date, with mismatching API surfaces.
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to load plugin [{interfaceTypeName}]: {z.FullName}");
            Debug.WriteLine(ex.Message);
            if (ex is not ReflectionTypeLoadException rtle)
                return Array.Empty<Type>();

            foreach (var le in rtle.LoaderExceptions)
            {
                if (le is not null)
                    Debug.WriteLine(le.Message);
            }
            return Array.Empty<Type>();
        }
    }

    private static bool IsTypePlugin(Type type, string interfaceTypeName)
    {
        if (type.IsInterface || type.IsAbstract)
            return false;
        if (type.GetInterface(interfaceTypeName) == null)
            return false;
        return true;
    }
}
