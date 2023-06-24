using System;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Core
{
    /// <summary>
    /// Pokédex structure used for Brilliant Diamond &amp; Shining Pearl.
    /// </summary>
    /// <remarks>size 0x30B8, struct_name ZUKAN_WORK</remarks>
    public sealed class Zukan8bLumi : Zukan8b
    {
        /* Structure Notes:
            u32 [493] state: None/HeardOf/Seen/Captured
            bool[493] maleShiny
            bool[493] femaleShiny
            bool[493] male
            bool[493] female
            bool[28] Unown Form
            bool[28] Unown FormShiny
            bool[4] Castform
            bool[4] Castform
            bool[4] Deoxys
            bool[4] Deoxys
            bool[3] Burmy
            bool[3] Burmy
            bool[3] Wormadam
            bool[3] Wormadam
            bool[3] Wormadam
            bool[3] Wormadam
            bool[3] Mothim
            bool[3] Mothim
            bool[2] Cherrim
            bool[2] Cherrim
            bool[2] Shellos
            bool[2] Shellos
            bool[2] Gastrodon
            bool[2] Gastrodon
            bool[6] Rotom
            bool[6] Rotom
            bool[2] Giratina
            bool[2] Giratina
            bool[2] Shaymin
            bool[2] Shaymin
            bool[18] Arceus
            bool[18] Arceus
            u32 [493] language flags
            bool regional dex obtained
            bool national dex obtained
         */

        private const int OFS_STATE = 0;

        private static PersonalTable Personal => PersonalTable.BDSPLUMI;

        public Zukan8bLumi(SAV8BSLuminescent sav, int dex) : base(sav, dex) { }

        private int GetStateStructOffset(int species)
        {
            if ((uint)species > (uint)Species.MAX_COUNT - 1)
                throw new ArgumentOutOfRangeException(nameof(species));
            return OFS_STATE + (species / 2);
        }

        private int GetBooleanStructOffset(int index, int baseOffset, int max)
        {
            if ((uint)index > (uint)Species.MAX_COUNT - 1)
                throw new ArgumentOutOfRangeException(nameof(index));
            return baseOffset + (index / 8);
        }

        private void SetNibble(ref byte bitFlag, byte bitIndex, byte nibbleValue)
        {
            bitFlag = (byte)(bitFlag & ~(0xF << bitIndex) | (nibbleValue << bitIndex));
        }

        private void SetBit(ref byte bitFlag, byte bitIndex, bool bitValue)
        {
            bitFlag = (byte)(bitFlag & ~(0xF << bitIndex) | ((bitValue ? 1 : 0) << bitIndex));
        }

        public override ZukanState8b GetState(int species) => (ZukanState8b)(SAV.Data[PokeDex + GetStateStructOffset(species)] >> ((species & 1) * 4) & 0xF);

        public override void SetState(int species, ZukanState8b state) => SetNibble(ref SAV.Data[PokeDex + GetStateStructOffset(species)], (byte)((species & 1) * 4), (byte)state);

        public override bool GetBoolean(int index, int baseOffset, int max) => (SAV.Data[PokeDex + GetBooleanStructOffset(index, baseOffset, max)] >> (index % 8) & 1) == 1;

        public override void SetBoolean(int index, int baseOffset, int max, bool value) => SetBit(ref SAV.Data[PokeDex + GetBooleanStructOffset(index, baseOffset, max)], (byte)(index % 8), value);

        public override bool GetLanguageFlag(int species, int language)
        {
            if ((uint)species > (uint)Species.MAX_COUNT - 1)
                throw new ArgumentOutOfRangeException(nameof(species));
            // Lang flags in 1.3.0 Lumi Revision 1 Save hasn't been changed to bitfields
            else if ((uint)species > Legal.MaxSpeciesID_4)
                return false;

            var languageBit = GetLanguageBit(language);
            if (languageBit == -1)
                return false;

            var index = species - 1;
            var offset = OFS_LANGUAGE + (sizeof(int) * index);
            var current = ReadInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset));
            return (current & (1 << languageBit)) != 0;
        }

        public override void SetLanguageFlag(int species, int language, bool value)
        {
            if ((uint)species > (uint)Species.MAX_COUNT - 1)
                throw new ArgumentOutOfRangeException(nameof(species));
            // Lang flags in 1.3.0 Lumi Revision 1 Save hasn't been changed to bitfields
            else if ((uint)species > Legal.MaxSpeciesID_4)
                return;

            var languageBit = GetLanguageBit(language);
            if (languageBit == -1)
                return;

            var index = species - 1;
            var offset = OFS_LANGUAGE + (sizeof(int) * index);
            var current = ReadInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset));
            var mask = (1 << languageBit);
            var update = value ? current | mask : current & ~(mask);
            WriteInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset), update);
        }

        public override void SetLanguageFlags(int species, int value)
        {
            if ((uint)species > (uint)Species.MAX_COUNT - 1)
                throw new ArgumentOutOfRangeException(nameof(species));
            // Lang flags in 1.3.0 Lumi Revision 1 Save hasn't been changed to bitfields
            else if ((uint)species > Legal.MaxSpeciesID_4)
                return;

            var index = species - 1;
            var offset = OFS_LANGUAGE + (sizeof(int) * index);
            WriteInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset), value);
        }

        public override void SeenNone()
        {
            for (int species = 1; species <= (uint)Species.MAX_COUNT - 1; species++)
                ClearDexEntryAll(species);
        }

        public override void CaughtNone()
        {
            for (int species = 1; species <= (uint)Species.MAX_COUNT - 1; species++)
            {
                if (GetCaught(species))
                    SetState(species, ZukanState8b.Seen);
                SetLanguageFlags(species, 0);
            }
        }

        public override void SeenAll(bool shinyToo = false)
        {
            var pt = Personal;
            for (int i = 1; i <= (uint)Species.MAX_COUNT - 1; i++)
            {
                if (!GetSeen(i))
                    SetState(i, ZukanState8b.Seen);
                var pi = pt[i];
                var m = !pi.OnlyFemale;
                var f = !pi.OnlyMale;
                SetGenderFlags(i, m, f, m && shinyToo, f && shinyToo);
            }
        }

        public override void CompleteDex(bool shinyToo = false)
        {
            for (int species = 1; species <= (uint)Species.MAX_COUNT - 1; species++)
                SetDexEntryAll(species, shinyToo);
        }

        public override void CaughtAll(bool shinyToo = false)
        {
            var pt = Personal;
            for (int species = 1; species <= (uint)Species.MAX_COUNT - 1; species++)
            {
                SetState(species, ZukanState8b.Caught);
                var pi = pt[species];
                var m = !pi.OnlyFemale;
                var f = !pi.OnlyMale;
                SetGenderFlags(species, m, f, m && shinyToo, f && shinyToo);
                SetLanguageFlag(species, SAV.Language, true);
            }
        }

        public override void SetAllSeen(bool value = true, bool shinyToo = false)
        {
            var pt = Personal;
            for (int species = 1; species <= (uint)Species.MAX_COUNT - 1; species++)
            {
                if (value)
                {
                    if (!GetSeen(species))
                        SetState(species, ZukanState8b.Seen);
                    var pi = pt[species];
                    var m = !pi.OnlyFemale;
                    var f = !pi.OnlyMale;
                    SetGenderFlags(species, m, f, m && shinyToo, f && shinyToo);
                }
                else
                {
                    ClearDexEntryAll(species);
                }
            }
        }
    }
}
