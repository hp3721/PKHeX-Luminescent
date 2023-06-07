﻿using System;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Core
{
    /// <summary>
    /// Pokédex structure used for Brilliant Diamond &amp; Shining Pearl.
    /// </summary>
    /// <remarks>size 0x30B8, struct_name ZUKAN_WORK</remarks>
    public class Zukan8b : ZukanBase
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

        private const int COUNT_SPECIES = 493;
        private const int COUNT_UNOWN = 28;
        private const int COUNT_CASTFORM = 4;
        private const int COUNT_DEOXYS = 4;
        private const int COUNT_BURMY = 3;
        private const int COUNT_WORMADAM = 3;
        private const int COUNT_MOTHIM = 3;
        private const int COUNT_CHERRIM = 2;
        private const int COUNT_SHELLOS = 2;
        private const int COUNT_GASTRODON = 2;
        private const int COUNT_ROTOM = 6;
        private const int COUNT_GIRATINA = 2;
        private const int COUNT_SHAYMIN = 2;
        private const int COUNT_ARCEUS = 18;

        private const int SIZE_SPECIES   = sizeof(ZukanState8b) * COUNT_SPECIES;
        private const int ALIGN_BOOLARRAY = 4;
        private const int SIZE_SPECIESBOOL = ALIGN_BOOLARRAY * COUNT_SPECIES;
        private const int SIZE_UNOWN       = ALIGN_BOOLARRAY * COUNT_UNOWN;
        private const int SIZE_CASTFORM    = ALIGN_BOOLARRAY * COUNT_CASTFORM;
        private const int SIZE_DEOXYS      = ALIGN_BOOLARRAY * COUNT_DEOXYS;
        private const int SIZE_BURMY       = ALIGN_BOOLARRAY * COUNT_BURMY;
        private const int SIZE_WORMADAM    = ALIGN_BOOLARRAY * COUNT_WORMADAM;
        private const int SIZE_MOTHIM      = ALIGN_BOOLARRAY * COUNT_MOTHIM;
        private const int SIZE_CHERRIM     = ALIGN_BOOLARRAY * COUNT_CHERRIM;
        private const int SIZE_SHELLOS     = ALIGN_BOOLARRAY * COUNT_SHELLOS;
        private const int SIZE_GASTRODON   = ALIGN_BOOLARRAY * COUNT_GASTRODON;
        private const int SIZE_ROTOM       = ALIGN_BOOLARRAY * COUNT_ROTOM;
        private const int SIZE_GIRATINA    = ALIGN_BOOLARRAY * COUNT_GIRATINA;
        private const int SIZE_SHAYMIN     = ALIGN_BOOLARRAY * COUNT_SHAYMIN;
        private const int SIZE_ARCEUS      = ALIGN_BOOLARRAY * COUNT_ARCEUS;
        private const int SIZE_LANGUAGE    = sizeof(int) * COUNT_SPECIES;

        private const int OFS_STATE = 0;
        private const int OFS_MALESHINY = OFS_STATE + SIZE_SPECIES;
        private const int OFS_FEMALESHINY = OFS_MALESHINY + SIZE_SPECIESBOOL;
        private const int OFS_MALE = OFS_FEMALESHINY + SIZE_SPECIESBOOL;
        private const int OFS_FEMALE = OFS_MALE + SIZE_SPECIESBOOL;

        private const int OFS_UNOWN      = OFS_FEMALE     + SIZE_SPECIESBOOL;
        private const int OFS_SUNOWN     = OFS_UNOWN      + SIZE_UNOWN;
        private const int OFS_CASTFORM   = OFS_SUNOWN     + SIZE_UNOWN;
        private const int OFS_SCASTFORM  = OFS_CASTFORM   + SIZE_CASTFORM;
        private const int OFS_DEOXYS     = OFS_SCASTFORM  + SIZE_CASTFORM;
        private const int OFS_SDEOXYS    = OFS_DEOXYS     + SIZE_DEOXYS;
        private const int OFS_BURMY      = OFS_SDEOXYS    + SIZE_DEOXYS;
        private const int OFS_SBURMY     = OFS_BURMY      + SIZE_BURMY;
        private const int OFS_WORMADAM   = OFS_SBURMY     + SIZE_BURMY;
        private const int OFS_SWORMADAM  = OFS_WORMADAM   + SIZE_WORMADAM;
        private const int OFS_MOTHIM     = OFS_SWORMADAM  + SIZE_WORMADAM;
        private const int OFS_SMOTHIM    = OFS_MOTHIM     + SIZE_MOTHIM;
        private const int OFS_CHERRIM    = OFS_SMOTHIM    + SIZE_MOTHIM;
        private const int OFS_SCHERRIM   = OFS_CHERRIM    + SIZE_CHERRIM;
        private const int OFS_SHELLOS    = OFS_SCHERRIM   + SIZE_CHERRIM;
        private const int OFS_SSHELLOS   = OFS_SHELLOS    + SIZE_SHELLOS;
        private const int OFS_GASTRODON  = OFS_SSHELLOS   + SIZE_SHELLOS;
        private const int OFS_SGASTRODON = OFS_GASTRODON  + SIZE_GASTRODON;
        private const int OFS_ROTOM      = OFS_SGASTRODON + SIZE_GASTRODON;
        private const int OFS_SROTOM     = OFS_ROTOM      + SIZE_ROTOM;
        private const int OFS_GIRATINA   = OFS_SROTOM     + SIZE_ROTOM;
        private const int OFS_SGIRATINA  = OFS_GIRATINA   + SIZE_GIRATINA;
        private const int OFS_SHAYMIN    = OFS_SGIRATINA  + SIZE_GIRATINA;
        private const int OFS_SSHAYMIN   = OFS_SHAYMIN    + SIZE_SHAYMIN;
        private const int OFS_ARCEUS     = OFS_SSHAYMIN   + SIZE_SHAYMIN;
        private const int OFS_SARCEUS    = OFS_ARCEUS     + SIZE_ARCEUS;

        protected const int OFS_LANGUAGE = OFS_SARCEUS + SIZE_ARCEUS;
        private const int OFS_FLAG_REGIONAL = OFS_LANGUAGE + SIZE_LANGUAGE; // 0x30B0
        private const int OFS_FLAG_NATIONAL = OFS_FLAG_REGIONAL + 4;        // 0x30B4
        // sizeof(this) = 0x30B8

        protected const int LANGUAGE_NONE = 0;
        protected const int LANGUAGE_ALL = // 0x1FF
            1 << (int)LanguageID.Japanese - 1 |
            1 << (int)LanguageID.English  - 1 |
            1 << (int)LanguageID.French   - 1 |
            1 << (int)LanguageID.Italian  - 1 |
            1 << (int)LanguageID.German   - 1 |
            1 << (int)LanguageID.Spanish  - 2 | // Skip over Language 6 (unused)
            1 << (int)LanguageID.Korean   - 2 |
            1 << (int)LanguageID.ChineseS - 2 |
            1 << (int)LanguageID.ChineseT - 2;

        private static PersonalTable Personal => PersonalTable.BDSP;

        public Zukan8b(SAV8BS sav, int dex) : base(sav, dex) { }

        public virtual ZukanState8b GetState(int species)
        {
            if ((uint)species > Legal.MaxSpeciesID_4)
                throw new ArgumentOutOfRangeException(nameof(species));

            var index = species - 1;
            var offset = OFS_STATE + (sizeof(int) * index);
            return (ZukanState8b)ReadInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset));
        }

        public virtual void SetState(int species, ZukanState8b state)
        {
            if ((uint)species > Legal.MaxSpeciesID_4)
                throw new ArgumentOutOfRangeException(nameof(species));

            var index = species - 1;
            var offset = OFS_STATE + (sizeof(int) * index);
            WriteInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset), (int)state);
        }

        public virtual bool GetBoolean(int index, int baseOffset, int max)
        {
            if ((uint)index > (uint)max)
                throw new ArgumentOutOfRangeException(nameof(index));

            var offset = baseOffset + (ALIGN_BOOLARRAY * index);
            return ReadUInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset)) == 1;
        }

        public virtual void SetBoolean(int index, int baseOffset, int max, bool value)
        {
            if ((uint)index > (uint)max)
                throw new ArgumentOutOfRangeException(nameof(index));

            var offset = baseOffset + (ALIGN_BOOLARRAY * index);
            WriteUInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset), value ? 1u : 0u);
        }

        public void GetGenderFlags(int species, out bool m, out bool f, out bool ms, out bool fs)
        {
            m  = GetBoolean(species - 1, OFS_MALE, COUNT_SPECIES - 1);
            f  = GetBoolean(species - 1, OFS_FEMALE, COUNT_SPECIES - 1);
            ms = GetBoolean(species - 1, OFS_MALESHINY, COUNT_SPECIES - 1);
            fs = GetBoolean(species - 1, OFS_FEMALESHINY, COUNT_SPECIES - 1);
        }

        public void SetGenderFlags(int species, bool m, bool f, bool ms, bool fs)
        {
            SetBoolean(species - 1, OFS_MALE, COUNT_SPECIES - 1, m);
            SetBoolean(species - 1, OFS_FEMALE, COUNT_SPECIES - 1, f);
            SetBoolean(species - 1, OFS_MALESHINY, COUNT_SPECIES - 1, ms);
            SetBoolean(species - 1, OFS_FEMALESHINY, COUNT_SPECIES - 1, fs);
        }

        public virtual bool GetLanguageFlag(int species, int language)
        {
            if ((uint)species > Legal.MaxSpeciesID_4)
                throw new ArgumentOutOfRangeException(nameof(species));
            var languageBit = GetLanguageBit(language);
            if (languageBit == -1)
                return false;

            var index = species - 1;
            var offset = OFS_LANGUAGE + (sizeof(int) * index);
            var current = ReadInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset));
            return (current & (1 << languageBit)) != 0;
        }

        public virtual void SetLanguageFlag(int species, int language, bool value)
        {
            if ((uint)species > Legal.MaxSpeciesID_4)
                throw new ArgumentOutOfRangeException(nameof(species));
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

        public virtual void SetLanguageFlags(int species, int value)
        {
            if ((uint)species > Legal.MaxSpeciesID_4)
                throw new ArgumentOutOfRangeException(nameof(species));

            var index = species - 1;
            var offset = OFS_LANGUAGE + (sizeof(int) * index);
            WriteInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset), value);
        }

        protected static int GetLanguageBit(int language)
        {
            if (language is 0 or (int)LanguageID.UNUSED_6 or > (int)LanguageID.ChineseT)
                return -1;
            if (language >= (int)LanguageID.Spanish)
                return language - 2;
            return language - 1;
        }

        public bool HasRegionalDex
        {
            get => ReadUInt32LittleEndian(SAV.Data.AsSpan(PokeDex + OFS_FLAG_REGIONAL)) == 1;
            set => WriteUInt32LittleEndian(SAV.Data.AsSpan(PokeDex + OFS_FLAG_REGIONAL), value ? 1u : 0u);
        }

        public bool HasNationalDex
        {
            get => ReadUInt32LittleEndian(SAV.Data.AsSpan(PokeDex + OFS_FLAG_NATIONAL)) == 1;
            set => WriteUInt32LittleEndian(SAV.Data.AsSpan(PokeDex + OFS_FLAG_NATIONAL), value ? 1u : 0u);
        }

        public bool GetHasFormFlag(int species, int form, bool shiny)
        {
            var ct = GetFormCount(species);
            if (ct == 0)
                return false;

            var baseOffset = GetFormOffset(species);
            var sizeShift = shiny ? GetFormSize(species) : 0;
            var offset = baseOffset + sizeShift + (ALIGN_BOOLARRAY * form);
            return ReadUInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset)) == 1;
        }

        public void SetHasFormFlag(int species, int form, bool shiny, bool value)
        {
            var formCount = GetFormCount(species);
            if (formCount is 0 || (uint)form >= formCount)
                return;

            var baseOffset = GetFormOffset(species);
            var sizeShift = shiny ? GetFormSize(species) : 0;
            var offset = baseOffset + sizeShift + (ALIGN_BOOLARRAY * form);
            WriteUInt32LittleEndian(SAV.Data.AsSpan(PokeDex + offset), value ? 1u : 0u);
        }

        public static int GetFormCount(int species) => species switch
        {
            (int)Species.Unown     => COUNT_UNOWN,
            (int)Species.Castform  => COUNT_CASTFORM,
            (int)Species.Deoxys    => COUNT_DEOXYS,
            (int)Species.Burmy     => COUNT_BURMY,
            (int)Species.Wormadam  => COUNT_WORMADAM,
            (int)Species.Mothim    => COUNT_MOTHIM,
            (int)Species.Cherrim   => COUNT_CHERRIM,
            (int)Species.Shellos   => COUNT_SHELLOS,
            (int)Species.Gastrodon => COUNT_GASTRODON,
            (int)Species.Rotom     => COUNT_ROTOM,
            (int)Species.Giratina  => COUNT_GIRATINA,
            (int)Species.Shaymin   => COUNT_SHAYMIN,
            (int)Species.Arceus    => COUNT_ARCEUS,
            _ => 0,
        };

        private static int GetFormSize(int species) => species switch
        {
            (int)Species.Unown     => SIZE_UNOWN,
            (int)Species.Castform  => SIZE_CASTFORM,
            (int)Species.Deoxys    => SIZE_DEOXYS,
            (int)Species.Burmy     => SIZE_BURMY,
            (int)Species.Wormadam  => SIZE_WORMADAM,
            (int)Species.Mothim    => SIZE_MOTHIM,
            (int)Species.Cherrim   => SIZE_CHERRIM,
            (int)Species.Shellos   => SIZE_SHELLOS,
            (int)Species.Gastrodon => SIZE_GASTRODON,
            (int)Species.Rotom     => SIZE_ROTOM,
            (int)Species.Giratina  => SIZE_GIRATINA,
            (int)Species.Shaymin   => SIZE_SHAYMIN,
            (int)Species.Arceus    => SIZE_ARCEUS,
            _ => 0,
        };

        private static int GetFormOffset(int species) => species switch
        {
            (int)Species.Unown     => OFS_UNOWN,
            (int)Species.Castform  => OFS_CASTFORM,
            (int)Species.Deoxys    => OFS_DEOXYS,
            (int)Species.Burmy     => OFS_BURMY,
            (int)Species.Wormadam  => OFS_WORMADAM,
            (int)Species.Mothim    => OFS_MOTHIM,
            (int)Species.Cherrim   => OFS_CHERRIM,
            (int)Species.Shellos   => OFS_SHELLOS,
            (int)Species.Gastrodon => OFS_GASTRODON,
            (int)Species.Rotom     => OFS_ROTOM,
            (int)Species.Giratina  => OFS_GIRATINA,
            (int)Species.Shaymin   => OFS_SHAYMIN,
            (int)Species.Arceus    => OFS_ARCEUS,
            _ => 0,
        };

        public bool GetHeard(int species) => GetState(species) >= ZukanState8b.HeardOf;
        public override bool GetSeen(int species) => GetState(species) >= ZukanState8b.Seen;
        public override bool GetCaught(int species) => GetState(species) >= ZukanState8b.Caught;

        public override void SetDex(PKM pkm)
        {
            int species = pkm.Species;
            if (species is 0 or > Legal.MaxSpeciesID_4)
                return;
            if (pkm.IsEgg) // do not add
                return;

            var originalState = GetState(species);
            bool shiny = pkm.IsShiny;
            SetState(species, ZukanState8b.Caught);
            SetGenderFlag(species, pkm.Gender, shiny);
            SetLanguageFlag(species, pkm.Language, true);
            SetHasFormFlag(species, pkm.Form, shiny, true);
            if (species is (int)Species.Spinda)
                ((SAV8BS)SAV).ZukanExtra.SetDex(originalState, pkm.EncryptionConstant, pkm.Gender, shiny);
        }

        private void SetGenderFlag(int species, int gender, bool shiny)
        {
            switch (gender)
            {
                case 0: SetGenderFlagMale(species, shiny); break;
                case 1: SetGenderFlagFemale(species, shiny); break;
                case 2: // Yep, sets both gender flags.
                    SetGenderFlagMale(species, shiny);
                    SetGenderFlagFemale(species, shiny);
                    break;
            }
        }

        private void SetGenderFlagMale(int species, bool shiny) => SetBoolean(species - 1, shiny ? OFS_MALESHINY : OFS_MALE, COUNT_SPECIES - 1, true);
        private void SetGenderFlagFemale(int species, bool shiny) => SetBoolean(species - 1, shiny ? OFS_FEMALESHINY : OFS_FEMALE, COUNT_SPECIES - 1, true);

        public override void SeenNone()
        {
            for (int species = 1; species <= Legal.MaxSpeciesID_4; species++)
                ClearDexEntryAll(species);
        }

        public override void CaughtNone()
        {
            for (int species = 1; species <= Legal.MaxSpeciesID_4; species++)
            {
                if (GetCaught(species))
                    SetState(species, ZukanState8b.Seen);
                SetLanguageFlags(species, 0);
            }
        }

        public override void SeenAll(bool shinyToo = false)
        {
            var pt = Personal;
            for (int i = 1; i <= Legal.MaxSpeciesID_4; i++)
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
            for (int species = 1; species <= Legal.MaxSpeciesID_4; species++)
                SetDexEntryAll(species, shinyToo);
        }

        public override void CaughtAll(bool shinyToo = false)
        {
            var pt = Personal;
            for (int species = 1; species <= Legal.MaxSpeciesID_4; species++)
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
            for (int species = 1; species <= Legal.MaxSpeciesID_4; species++)
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

        public override void SetDexEntryAll(int species, bool shinyToo = false)
        {
            SetState(species, ZukanState8b.Caught);

            var pt = Personal;
            var pi = pt[species];
            var m = !pi.OnlyFemale;
            var f = !pi.OnlyMale;
            SetGenderFlags(species, m, f, m && shinyToo, f && shinyToo);

            var formCount = GetFormCount(species);
            if (formCount is not 0)
            {
                for (int form = 0; form < formCount; form++)
                {
                    SetHasFormFlag(species, form, false, true);
                    if (shinyToo)
                        SetHasFormFlag(species, form, true, true);
                }
            }
            SetLanguageFlags(species, LANGUAGE_ALL);
        }

        public override void ClearDexEntryAll(int species)
        {
            SetState(species, ZukanState8b.None);
            SetGenderFlags(species, false, false, false, false);

            var formCount = GetFormCount(species);
            if (formCount is not 0)
            {
                for (int form = 0; form < formCount; form++)
                {
                    SetHasFormFlag(species, form, false, false);
                    SetHasFormFlag(species, form, true, false);
                }
            }
            SetLanguageFlags(species, LANGUAGE_NONE);
        }
    }

    public enum ZukanState8b
    {
        None,
        HeardOf,
        Seen,
        Caught,
    }
}
