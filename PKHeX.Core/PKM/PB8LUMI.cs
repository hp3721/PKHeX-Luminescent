namespace PKHeX.Core;

/// <summary> Generation 8 <see cref="PKM"/> format. </summary>
public sealed class PB8LUMI : PB8
{
    public override PersonalInfo PersonalInfo => PersonalTable.BDSPLUMI.GetFormEntry(Species, Form);

    public PB8LUMI()
    {
        Egg_Location = Met_Location = Locations.Default8bNone;
        AffixedRibbon = -1; // 00 would make it show Kalos Champion :)
    }

    public PB8LUMI(PB8 pk) : base((byte[])pk.Data.Clone()) { }
    public PB8LUMI(byte[] data) : base(data) { }
    public override PKM Clone() => new PB8LUMI((byte[])Data.Clone());

    // Maximums
    public override int MaxMoveID => Legal.MaxMoveID_8b;
    public override int MaxSpeciesID => (int)Core.Species.MAX_COUNT - 1;
    public override int MaxAbilityID => Legal.MaxAbilityID_8b;
    public override int MaxItemID => Legal.MaxItemID_8b;
    public override int MaxBallID => Legal.MaxBallID_8b;
    public override int MaxGameID => Legal.MaxGameID_8b;
}
