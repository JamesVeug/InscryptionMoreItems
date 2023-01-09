using DiskCardGame;
using InscryptionAPI.Card;
using MoreItemsMod.Scripts.SpecialAbilities;

namespace MoreItemsMod;

public class BaitBucketConsumableitem : AAddCardToEmptySlotsConsumableItem
{
	protected override CardInfo CardInfo
	{
		get
		{
			CardInfo cardInfo = CardLoader.GetCardByName("BaitBucket");
			cardInfo.AddSpecialAbilities(BaitBucket.ID);
			return cardInfo;
		}
	}

	public override bool ExtraActivationPrerequisitesMet()
	{
		if (!base.ExtraActivationPrerequisitesMet())
		{
			return false;
		}
		
		// At least 1 card on the field that can kill a bucket
		CardSlot attackingCard = Singleton<BoardManager>.Instance.AllSlots.Find((a)=>a.Card != null && !a.Card.Dead && a.Card.Attack > 0);
		return attackingCard != null;
	}
}