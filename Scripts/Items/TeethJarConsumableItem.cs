using System.Collections;
using DiskCardGame;
using UnityEngine;

namespace MoreItemsMod;

public class TeethJarConsumableItem : ConsumableItem
{
	public override IEnumerator ActivateSequence()
	{
		base.PlayExitAnimation();
		
		int teeth = 10;
		RunState.Run.currency += teeth;
		yield return Singleton<CurrencyBowl>.Instance.ShowGain(Mathf.Min(20, teeth), true, false);
	}
}