using DiskCardGame;

namespace MoreItemsMod;

/// <summary>
/// Adds Brittle to all cards on the board
/// </summary>
public class AllBrittleItemBehaviour : AAddSigilToAllCardsConsumableItem
{
    protected override Ability Ability => Ability.Brittle;
}