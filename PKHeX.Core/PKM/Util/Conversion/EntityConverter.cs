﻿using System;
using System.Diagnostics;
using static PKHeX.Core.EntityConverterResult;

namespace PKHeX.Core;

/// <summary>
/// Logic for converting a <see cref="PKM"/> from one generation specific format to another.
/// </summary>
public static class EntityConverter
{
    /// <summary>
    /// If a conversion method does not officially (legally) exist, then the program can try to convert via other means (illegal).
    /// </summary>
    public static bool AllowIncompatibleConversion { get; set; }

    /// <summary>
    /// Checks if the input <see cref="PKM"/> file is capable of being converted to the desired format.
    /// </summary>
    /// <param name="pk"></param>
    /// <param name="format"></param>
    /// <returns>True if can be converted to the requested format value.</returns>
    public static bool IsConvertibleToFormat(PKM pk, int format)
    {
        if (pk.Format >= 3 && pk.Format > format)
            return false; // pk3->upward can't go backwards
        if (pk.Format <= 2 && format is > 2 and < 7)
            return false; // pk1/2->upward has to be 7 or greater
        return true;
    }

    /// <summary>
    /// Converts a PKM from one Generation format to another. If it matches the destination format, the conversion will automatically return.
    /// </summary>
    /// <param name="pk">PKM to convert</param>
    /// <param name="destType">Format/Type to convert to</param>
    /// <param name="result">Comments regarding the transfer's success/failure</param>
    /// <returns>Converted PKM</returns>
    public static PKM? ConvertToType(PKM pk, Type destType, out EntityConverterResult result)
    {
        Type fromType = pk.GetType();
        if (fromType == destType)
        {
            result = None;
            return pk;
        }

        var pkm = ConvertPKM(pk, destType, fromType, out result);
        if (!AllowIncompatibleConversion || pkm != null)
            return pkm;

        if (pk is PK8 && destType == typeof(PB8))
        {
            result = SuccessIncompatibleManual;
            return new PB8((byte[])pk.Data.Clone());
        }

        if (pk is PB8 && destType == typeof(PK8))
        {
            result = SuccessIncompatibleManual;
            return new PK8((byte[])pk.Data.Clone());
        }

        // Try Incompatible Conversion
        pkm = EntityBlank.GetBlank(destType);
        pk.TransferPropertiesWithReflection(pkm);
        if (!IsCompatibleWithModifications(pkm))
            return null; // NoTransferRoute
        result = SuccessIncompatibleReflection;
        return pkm;
    }

    private static PKM? ConvertPKM(PKM pk, Type destType, Type srcType, out EntityConverterResult result)
    {
        result = CheckTransfer(pk);
        if (result != Success)
            return null;

        Debug.WriteLine($"Trying to convert {srcType.Name} to {destType.Name}.");

        // All types that inherit PKM have the generation specifier as the last char in their class name.
        return ConvertPKM(pk, destType, ref result);
    }

    private static PKM? ConvertPKM(PKM pk, Type destType, ref EntityConverterResult result)
    {
        PKM? pkm = pk.Clone();
        if (pkm.IsEgg)
            pkm.ForceHatchPKM();
        while (true)
        {
            pkm = IntermediaryConvert(pkm, destType, ref result);
            if (pkm == null) // fail convert
                return null;
            if (pkm.GetType() == destType) // finish convert
                return pkm;
        }
    }

    private static PKM? IntermediaryConvert(PKM pk, Type destType, ref EntityConverterResult result) => pk switch
    {
        // Non-sequential
        PK1 pk1 when destType.Name[^1] - '0' > 2 => pk1.ConvertToPK7(),
        PK2 pk2 when destType.Name[^1] - '0' > 2 => pk2.ConvertToPK7(),
        PK2 pk2 when destType == typeof(SK2) => pk2.ConvertToSK2(),
        PK3 pk3 when destType == typeof(CK3) => pk3.ConvertToCK3(),
        PK3 pk3 when destType == typeof(XK3) => pk3.ConvertToXK3(),
        PK4 pk4 when destType == typeof(BK4) => pk4.ConvertToBK4(),
        PB8 pb8 when destType == typeof(PB8LUMI) => pb8.ConvertToPB8LUMI(),

        // Invalid
        PK2 { Species: > Legal.MaxSpeciesID_1 } => InvalidTransfer(out result, IncompatibleSpecies),

        // Sequential
        PK1 pk1 => pk1.ConvertToPK2(),
        PK2 pk2 => pk2.ConvertToPK1(),
        PK3 pk3 => pk3.ConvertToPK4(),
        PK4 pk4 => pk4.ConvertToPK5(),
        PK5 pk5 => pk5.ConvertToPK6(),
        PK6 pk6 => pk6.ConvertToPK7(),
        PK7 pk7 => pk7.ConvertToPK8(),
        PB7 pb7 => pb7.ConvertToPK8(),

        // Side-Formats back to Mainline
        SK2 sk2 => sk2.ConvertToPK2(),
        CK3 ck3 => ck3.ConvertToPK3(),
        XK3 xk3 => xk3.ConvertToPK3(),
        BK4 bk4 => bk4.ConvertToPK4(),

        _ => InvalidTransfer(out result, NoTransferRoute),
    };

    private static PKM? InvalidTransfer(out EntityConverterResult result, EntityConverterResult value)
    {
        result = value;
        return null;
    }

    /// <summary>
    /// Checks to see if a PKM is transferable relative to in-game restrictions and <see cref="PKM.Form"/>.
    /// </summary>
    /// <param name="pk">PKM to convert</param>
    /// <returns>Indication if Not Transferable</returns>
    private static EntityConverterResult CheckTransfer(PKM pk) => pk switch
    {
        PK4 { Species: (int)Species.Pichu, Form: not 0 } => IncompatibleForm,
        PK6 { Species: (int)Species.Pikachu, Form: not 0 } => IncompatibleForm,
        PB7 { Species: (int)Species.Pikachu, Form: not 0 } => IncompatibleForm,
        PB7 { Species: (int)Species.Eevee, Form: not 0 } => IncompatibleForm,
        _ => Success,
    };

    /// <summary>
    /// Checks if the <see cref="PKM"/> is compatible with the input <see cref="PKM"/>, and makes any necessary modifications to force compatibility.
    /// </summary>
    /// <remarks>Should only be used when forcing a backwards conversion to sanitize the PKM fields to the target format.
    /// If the PKM is compatible, some properties may be forced to sanitized values.</remarks>
    /// <param name="pk">PKM input that is to be sanity checked.</param>
    /// <param name="limit">Value clamps for the destination format</param>
    /// <returns>Indication whether or not the PKM is compatible.</returns>
    public static bool IsCompatibleWithModifications(PKM pk, IGameValueLimit limit)
    {
        if (pk.Species > limit.MaxSpeciesID)
            return false;

        MakeCompatible(pk, limit);
        return true;
    }

    private static void MakeCompatible(PKM pk, IGameValueLimit limit)
    {
        if (pk.HeldItem > limit.MaxItemID)
            pk.HeldItem = 0;

        if (pk.Nickname.Length > limit.NickLength)
            pk.Nickname = pk.Nickname[..pk.NickLength];

        if (pk.OT_Name.Length > limit.OTLength)
            pk.OT_Name = pk.OT_Name[..pk.OTLength];

        if (pk.Move1 > limit.MaxMoveID || pk.Move2 > limit.MaxMoveID || pk.Move3 > limit.MaxMoveID || pk.Move4 > limit.MaxMoveID)
            pk.ClearInvalidMoves();

        int maxEV = pk.MaxEV;
        for (int i = 0; i < 6; i++)
        {
            if (pk.GetEV(i) > maxEV)
                pk.SetEV(i, maxEV);
        }

        int maxIV = pk.MaxIV;
        for (int i = 0; i < 6; i++)
        {
            if (pk.GetIV(i) > maxIV)
                pk.SetIV(i, maxIV);
        }
    }

    /// <inheritdoc cref="IsCompatibleWithModifications(PKM, IGameValueLimit)"/>
    public static bool IsCompatibleWithModifications(PKM pk) => IsCompatibleWithModifications(pk, pk);

    /// <summary>
    /// Checks if the input <see cref="PKM"/> is compatible with the target <see cref="PKM"/>.
    /// </summary>
    /// <param name="pk">Input to check -> update/sanitize</param>
    /// <param name="target">Target type PKM with misc properties accessible for checking.</param>
    /// <param name="result">Comment output</param>
    /// <param name="converted">Output compatible PKM</param>
    /// <returns>Indication if the input is (now) compatible with the target.</returns>
    public static bool TryMakePKMCompatible(PKM pk, PKM target, out EntityConverterResult result, out PKM converted)
    {
        if (!IsConvertibleToFormat(pk, target.Format))
        {
            converted = target;
            if (!AllowIncompatibleConversion)
            {
                result = NoTransferRoute;
                return false;
            }
        }
        if (IsIncompatibleGB(target, target.Japanese, pk.Japanese))
        {
            converted = target;
            result = IncompatibleLanguageGB;
            return false;
        }
        var convert = ConvertToType(pk, target.GetType(), out result);
        if (convert == null)
        {
            converted = target;
            return false;
        }

        converted = convert;
        return true;
    }

    /// <summary>
    /// Checks if a <see cref="GBPKM"/> is incompatible with the Generation 1/2 destination environment.
    /// </summary>
    public static bool IsIncompatibleGB(PKM pk, bool destJapanese, bool srcJapanese)
    {
        if (pk.Format > 2)
            return false;
        if (destJapanese == srcJapanese)
            return false;
        if (pk is SK2 sk2 && sk2.IsPossible(srcJapanese))
            return false;
        return true;
    }
}
