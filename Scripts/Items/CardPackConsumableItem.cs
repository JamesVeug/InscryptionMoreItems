using System.Collections;
using DiskCardGame;
using UnityEngine;

namespace MoreItemsMod;

public class CardPackConsumableItem : ConsumableItem
{
	private static int CardPacksOpened = 0; 
	public override IEnumerator ActivateSequence()
	{
		base.PlayExitAnimation();
		
		Singleton<ViewManager>.Instance.SwitchToView(View.Hand, false, true);
		yield return new WaitForSeconds(0.2f);
		foreach (CardInfo info in GenerateCardPack())
		{
			Plugin.Log.LogInfo("Card " + info.name);
			yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(info, 0.25f);
			Singleton<ViewManager>.Instance.SwitchToView(View.Hand, false, true);
		}
		yield return new WaitForSeconds(0.25f);
	}
	
	private List<CardInfo> GenerateCardPack()
	{
		List<CardInfo> list = new List<CardInfo>();
		int currentRandomSeed = SaveManager.SaveFile.GetCurrentRandomSeed() + CardPacksOpened++;
		list.Add(CardInfoAmalgamIfNull(CardLoader.GetCardByName("Squirrel")));
		list.Add(CardInfoAmalgamIfNull(CardLoader.GetRandomChoosableCardWithCost(currentRandomSeed++, 1)));
		list.Add(CardInfoAmalgamIfNull(CardLoader.GetRandomChoosableCardWithCost(currentRandomSeed++, 2)));
		if (ProgressionData.LearnedMechanic(MechanicsConcept.Bones))
		{
			list.Add(CardInfoAmalgamIfNull(CardLoader.GetRandomChoosableBonesCard(currentRandomSeed++)));
		}
		else
		{
			list.Add(CardInfoAmalgamIfNull(CardLoader.GetRandomChoosableCardWithCost(currentRandomSeed++, 1)));
		}
		return list;
	}

	private CardInfo CardInfoAmalgamIfNull(CardInfo info)
	{
		if (info == null)
		{
			return CardLoader.GetCardByName("Amalgam");
		}

		return info;
	}
}