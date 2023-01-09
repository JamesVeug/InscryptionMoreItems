using System.Collections;
using DiskCardGame;
using UnityEngine;

namespace MoreItemsMod;

public class MeatCakeConsumableItem : ConsumableItem
{
    public override IEnumerator ActivateSequence()
    {
        base.PlayExitAnimation();
        
        Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, true);
        yield return new WaitForSeconds(0.2f);
        
        yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("May you continue their legacy");
        
        Singleton<ViewManager>.Instance.SwitchToView(View.Candles, false, true);
        yield return new WaitForSeconds(0.8f);
            
        if (Singleton<CandleHolder>.Instance != null)
        {
            yield return Singleton<CandleHolder>.Instance.ReplenishFlamesSequence();
        }
        RunState.Run.playerLives++;
        
        // Look at board
        yield return new WaitForSeconds(0.2f);
    }

    public override bool ExtraActivationPrerequisitesMet()
    {
        if (!base.ExtraActivationPrerequisitesMet())
        {
            return false;
        }

        if (Singleton<TurnManager>.Instance.Opponent is Part1BossOpponent)
        {
            return false;
        }

        return RunState.Run.playerLives < RunState.Run.maxPlayerLives;
    }
}