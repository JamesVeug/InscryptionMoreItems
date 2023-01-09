using DiskCardGame;

namespace MoreItemsMod;

/// <summary>
/// Adds Sharp to all cards on the board
/// </summary>
public class AllSharpItemBehaviour : AAddSigilToAllCardsConsumableItem
{
    protected override Ability Ability => Ability.Sharp;
}