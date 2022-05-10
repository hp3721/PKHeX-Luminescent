using System;
using static PKHeX.Core.Gem8LumiVersion;

namespace PKHeX.Core
{
    /// <summary>
    /// Indicates various <see cref="SAV8BS"/> revision values which change on each major patch update, updating the structure of the stored player save data.
    /// </summary>
    public enum Gem8LumiVersion
    {
        /// <summary>
        /// Initial cartridge version shipped (No Luminescent release).
        /// </summary>
        /// <remarks><see cref="SaveUtil.SIZE_G8BDSP"/></remarks>
        V1_0 = 0x25, // 37

        /// <summary>
        /// Pre-release patch.
        /// </summary>
        /// <remarks><see cref="SaveUtil.SIZE_G8BDSPLUMI_1"/></remarks>
        V1_1 = 0x2C, // 44

        /// <summary>
        /// February patch (No Luminescent release).
        /// </summary>
        /// <remarks><see cref="SaveUtil.SIZE_G8BDSP_2"/></remarks>
        V1_2 = 0x32, // 50

        /// <summary>
        /// March patch.
        /// </summary>
        /// <remarks><see cref="SaveUtil.SIZE_G8BDSPLUMI_3"/></remarks>
        V1_3 = 0x34, // 52
    }

    public static class Gem8LumiVersionExtensions
    {
        /// <summary>
        /// Returns a string to append to the savedata type info, indicating the revision of the player save data.
        /// </summary>
        /// <param name="version">Stored version value in the save data.</param>
        public static string GetSuffixString(this Gem8LumiVersion version) => version switch
        {
            V1_1 => "-1.1.0", // 1.1.0-Luminescent
            V1_3 => "-1.3.0", // 1.3.0-Luminescent
            _ => throw new ArgumentOutOfRangeException(nameof(version)),
        };
    }
}
