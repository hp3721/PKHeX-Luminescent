﻿using System.Drawing;
using PKHeX.Core;
using PKHeX.Drawing.PokeSprite.Properties;

namespace PKHeX.Drawing.PokeSprite;

public abstract class SpriteBuilder : ISpriteBuilder<Image>
{
    public static bool ShowEggSpriteAsItem { get; set; } = true;
    public static bool ShowEncounterBall { get; set; } = true;
    public static SpriteBackgroundType ShowEncounterColor { get; set; } = SpriteBackgroundType.FullBackground;
    public static SpriteBackgroundType ShowEncounterColorPKM { get; set; }
    public static bool ShowExperiencePercent { get; set; }

    public static byte ShowEncounterOpacityStripe { get; set; }
    public static byte ShowEncounterOpacityBackground { get; set; }
    public static int ShowEncounterThicknessStripe { get; set; }

    /// <summary> Width of the generated Sprite image. </summary>
    public abstract int Width { get; }
    /// <summary> Height of the generated Sprite image. </summary>
    public abstract int Height { get; }

    /// <summary> Minimum amount of padding on the right side of the image when layering an item sprite. </summary>
    protected abstract int ItemShiftX { get; }
    /// <summary> Minimum amount of padding on the bottom side of the image when layering an item sprite. </summary>
    protected abstract int ItemShiftY { get; }
    /// <summary> Max width / height of an item image. </summary>
    protected abstract int ItemMaxSize { get; }

    protected abstract int EggItemShiftX { get; }
    protected abstract int EggItemShiftY { get; }

    public abstract bool HasFallbackMethod { get; }

    public abstract Bitmap Hover { get; }
    public abstract Bitmap View { get; }
    public abstract Bitmap Set { get; }
    public abstract Bitmap Delete { get; }
    public abstract Bitmap Transparent { get; }
    public abstract Bitmap Drag { get; }
    public abstract Bitmap UnknownItem { get; }
    public abstract Bitmap None { get; }
    public abstract Bitmap ItemTM { get; }
    public abstract Bitmap ItemTR { get; }

    private const double UnknownFormTransparency = 0.5;
    private const double ShinyTransparency = 0.7;
    private const double EggUnderLayerTransparency = 0.33;

    protected abstract string GetSpriteStringSpeciesOnly(int species);

    protected abstract string GetSpriteAll(int species, int form, int gender, uint formarg, bool shiny, int generation);
    protected abstract string GetSpriteAllSecondary(int species, int form, int gender, uint formarg, bool shiny, int generation);
    protected abstract string GetItemResourceName(int item);
    protected abstract Bitmap Unknown { get; }
    protected abstract Bitmap GetEggSprite(int species);
    public abstract Bitmap ShadowLugia { get; }

    public void Initialize(SaveFile sav)
    {
        if (sav.Generation != 3 && sav is not SAV8BSLuminescent)
            return;

        Game = sav.Version;
        if (Game == GameVersion.FRLG)
            Game = sav.Personal == PersonalTable.FR ? GameVersion.FR : GameVersion.LG;
        else if (sav is SAV8BSLuminescent)
            Game = GameVersion.BDSPLUMI;
    }

    private GameVersion Game;

    private static int GetDeoxysForm(GameVersion game) => game switch
    {
        GameVersion.FR => 1, // Attack
        GameVersion.LG => 2, // Defense
        GameVersion.E => 3, // Speed
        _ => 0,
    };

    private static int GetArceusForm4(int form) => form switch
    {
        > 9 => form - 1, // Realign to Gen5+ type indexes
        9 => 999, // Curse, make it show as unrecognized form since we don't have a sprite.
        _ => form,
    };

	private static int GetLumiCustomForm(int species, int form) => species switch
	{
		(int)Species.Eevee or (int)Species.Mewtwo or (int)Species.Venusaur or (int)Species.Blastoise or (int)Species.Charizard or (int)Species.Onix or (int)Species.Gengar when form == 1 => 1000 - form,
		(int)Species.Pikachu when form == 17 => 1000 - form,
		_ => form,
	};

	public Image GetSprite(int species, int form, int gender, uint formarg, int heldItem, bool isEgg, bool isShiny, int generation = -1, bool isBoxBGRed = false, bool isAltShiny = false)
    {
        if (species == 0)
            return None;

        if (generation == 3 && species == (int)Species.Deoxys) // Depends on Gen3 save file version
            form = GetDeoxysForm(Game);
        else if (generation == 4 && species == (int)Species.Arceus) // Curse type's existence in Gen4
            form = GetArceusForm4(form);
        else if (Game == GameVersion.BDSPLUMI)
            form = GetLumiCustomForm(species, form);

        var baseImage = GetBaseImage(species, form, gender, formarg, isShiny, generation);
        return GetSprite(baseImage, species, heldItem, isEgg, isShiny, generation, isBoxBGRed, isAltShiny);
    }

    public Image GetSprite(Image baseSprite, int species, int heldItem, bool isEgg, bool isShiny, int generation = -1, bool isBoxBGRed = false, bool isAltShiny = false)
    {
        if (isEgg)
            baseSprite = LayerOverImageEgg(baseSprite, species, heldItem != 0);
        if (heldItem > 0)
            baseSprite = LayerOverImageItem(baseSprite, heldItem, generation);
        if (isShiny)
            baseSprite = LayerOverImageShiny(baseSprite, isBoxBGRed, generation >= 8 && isAltShiny);
        return baseSprite;
    }

    private Image GetBaseImage(int species, int form, int gender, uint formarg, bool shiny, int generation)
    {
        var img = FormInfo.IsTotemForm(species, form, generation)
            ? GetBaseImageTotem(species, form, gender, formarg, shiny, generation)
            : GetBaseImageDefault(species, form, gender, formarg, shiny, generation);
        return img ?? GetBaseImageFallback(species, form, gender, formarg, shiny, generation);
    }

    private Image? GetBaseImageTotem(int species, int form, int gender, uint formarg, bool shiny, int generation)
    {
        var baseform = FormInfo.GetTotemBaseForm(species, form);
        var baseImage = GetBaseImageDefault(species, baseform, gender, formarg, shiny, generation);
        if (baseImage == null)
            return null;
        return ImageUtil.ToGrayscale(baseImage);
    }

    private Image? GetBaseImageDefault(int species, int form, int gender, uint formarg, bool shiny, int generation)
    {
        var file = GetSpriteAll(species, form, gender, formarg, shiny, generation);
        var resource = (Image?)Resources.ResourceManager.GetObject(file);
        if (resource is null && HasFallbackMethod)
        {
            file = GetSpriteAllSecondary(species, form, gender, formarg, shiny, generation);
            resource = (Image?)Resources.ResourceManager.GetObject(file);
        }
        return resource;
    }

    private Image GetBaseImageFallback(int species, int form, int gender, uint formarg, bool shiny, int generation)
    {
        if (shiny) // try again without shiny
        {
            var img = GetBaseImageDefault(species, form, gender, formarg, false, generation);
            if (img != null)
                return img;
        }

        // try again without form
        var baseImage = (Image?)Resources.ResourceManager.GetObject(GetSpriteStringSpeciesOnly(species));
        if (baseImage == null) // failed again
            return Unknown;
        return ImageUtil.LayerImage(baseImage, Unknown, 0, 0, UnknownFormTransparency);
    }

    private Image LayerOverImageItem(Image baseImage, int item, int generation)
    {
        Image itemimg = generation switch
        {
            <= 4 when item is >=  328 and <=  419 => ItemTM, // gen2/3/4 TM
            8 when item is >=  328 and <=  427 => ItemTM, // BDSP TMs
            >= 8 when item is >= 1130 and <= 1229 => ItemTR, // Gen8 TR
            _ => (Image?)Resources.ResourceManager.GetObject(GetItemResourceName(item)) ?? UnknownItem,
        };

        // Redraw item in bottom right corner; since images are cropped, try to not have them at the edge
        int x = baseImage.Width - itemimg.Width - ((ItemMaxSize - itemimg.Width) / 4) - ItemShiftX;
        int y = baseImage.Height - itemimg.Height - ItemShiftY;
        return ImageUtil.LayerImage(baseImage, itemimg, x, y);
    }

    private static Image LayerOverImageShiny(Image baseImage, bool isBoxBGRed, bool altShiny)
    {
        // Add shiny star to top left of image.
        var rare = isBoxBGRed ? Resources.rare_icon_alt : Resources.rare_icon;
        if (altShiny)
            rare = Resources.rare_icon_2;
        return ImageUtil.LayerImage(baseImage, rare, 0, 0, ShinyTransparency);
    }

    private Image LayerOverImageEgg(Image baseImage, int species, bool hasItem)
    {
        if (ShowEggSpriteAsItem && !hasItem)
            return LayerOverImageEggAsItem(baseImage, species);
        return LayerOverImageEggTransparentSpecies(baseImage, species);
    }

    private Image LayerOverImageEggTransparentSpecies(Image baseImage, int species)
    {
        // Partially transparent species.
        baseImage = ImageUtil.ChangeOpacity(baseImage, EggUnderLayerTransparency);
        // Add the egg layer over-top with full opacity.
        var egg = GetEggSprite(species);
        return ImageUtil.LayerImage(baseImage, egg, 0, 0);
    }

    private Image LayerOverImageEggAsItem(Image baseImage, int species)
    {
        var egg = GetEggSprite(species);
        return ImageUtil.LayerImage(baseImage, egg, EggItemShiftX, EggItemShiftY); // similar to held item, since they can't have any
    }

    public static void LoadSettings(ISpriteSettings sprite)
    {
        ShowEggSpriteAsItem = sprite.ShowEggSpriteAsHeldItem;
        ShowEncounterBall = sprite.ShowEncounterBall;

        ShowEncounterColor = sprite.ShowEncounterColor;
        ShowEncounterColorPKM = sprite.ShowEncounterColorPKM;
        ShowEncounterThicknessStripe = sprite.ShowEncounterThicknessStripe;
        ShowEncounterOpacityBackground = sprite.ShowEncounterOpacityBackground;
        ShowEncounterOpacityStripe = sprite.ShowEncounterOpacityStripe;
        ShowExperiencePercent = sprite.ShowExperiencePercent;
    }
}
