using System.Collections;
using DiskCardGame;
using InscryptionAPI.Pelts;
using UnityEngine;

namespace MoreItemsMod;

public class TrapConsumableItem : ConsumableItem
{
	private const int cards = 4;
	
	public override IEnumerator ActivateSequence()
	{
		base.PlayExitAnimation();
		
		View currentView = Singleton<ViewManager>.Instance.CurrentView;
		if (currentView != View.Hand)
		{
			yield return new WaitForSeconds(0.2f);
			Singleton<ViewManager>.Instance.SwitchToView(View.Hand, false, false);
			yield return new WaitForSeconds(0.2f);
		}

		List<string> list = new();
		for (int i = 0; i < cards; i++)
		{
			if (i >= list.Count)
			{
				list.AddRange(CardLoader.PeltNames);
				list.Randomize();
			}
			string card = list[0];
			list.RemoveAt(0);
			
			CardInfo cardInfo = CardLoader.GetCardByName(card);
			yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(cardInfo, null, 0.25f, null);
		}

		if (currentView != View.Hand)
		{
			yield return new WaitForSeconds(0.2f);
			Singleton<ViewManager>.Instance.SwitchToView(currentView, false, false);
			yield return new WaitForSeconds(0.2f);
		}
	}
}