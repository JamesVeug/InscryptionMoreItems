using System.Collections;
using DiskCardGame;
using UnityEngine;

namespace MoreItemsMod;

public class MushroomConsumableItem : ConsumableItem
{
	public List<string> cards = new List<string>()
	{
		"MoreItems_JSON_LargeShroom",
		"MoreItems_JSON_MealShroom",
		"MoreItems_JSON_PoisonShroom",
		"MoreItems_JSON_SmallShroom",
		"MoreItems_JSON_SpinyShroom",
	};
	
	public override IEnumerator ActivateSequence()
	{
		base.PlayExitAnimation();
		CardInfo cardInfo = GetRandomCard();
		
		Singleton<ViewManager>.Instance.SwitchToView(View.Board, false, false);
		yield return new WaitForSeconds(0.1f);

		CardSlot cardSlot = Singleton<BoardManager>.Instance.PlayerSlotsCopy.FindAll((a) => a.Card == null).Randomize().First();
		yield return Singleton<BoardManager>.Instance.CreateCardInSlot(cardInfo, cardSlot, 0.15f, true);
		
		yield return new WaitForSeconds(0.5f);
	}

	private CardInfo GetRandomCard()
	{
		int currentRandomSeed = SaveManager.SaveFile.GetCurrentRandomSeed();
		int index = SeededRandom.Range(0, cards.Count, currentRandomSeed++);
		string cardID = cards[index];
		return CardLoader.GetCardByName(cardID);
	}

	public override bool ExtraActivationPrerequisitesMet()
	{
		return Singleton<BoardManager>.Instance.PlayerSlotsCopy.Find((a) => a.Card == null) != null;
	}
}