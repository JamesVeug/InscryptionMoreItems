using DiskCardGame;

namespace MoreItemsMod;

/// <summary>
/// Adds SplitStrike to all cards on the board
/// </summary>
public class AllSplitStrikeItemBehaviour : AAddSigilToAllCardsConsumableItem
{
    protected override Ability Ability => Ability.SplitStrike;
}