using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;

namespace MoreItemsMod.Scripts.SpecialAbilities;

public class BaitBucket : SpecialCardBehaviour
{
	public static SpecialTriggeredAbility ID;

	public static void Initialize()
	{
		ID =  SpecialTriggeredAbilityManager.Add(Plugin.PluginGuid, "Bait Bucket", typeof(BaitBucket)).Id;
	}

	public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
	{
		return true;
	}

	public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
	{
		yield return base.OnDie(wasSacrifice, killer);
		
		CardInfo cardByName = CardLoader.GetCardByName("Shark");
		yield return Singleton<BoardManager>.Instance.CreateCardInSlot(cardByName, this.PlayableCard.slot, 0.25f, true);
	}
}