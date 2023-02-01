using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace MoreItemsMod;

public class PupilsQuillConsumableItem : ConsumableItem
{
    private class Listener : NonCardTriggerReceiver
    {
        public override bool TriggerBeforeCards => true;

        public CardInfo deadCard = null;

        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            return !card.OpponentCard;
        }

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            deadCard = card.Info;
            yield return base.OnOtherCardDie(card, deathSlot, fromCombat, killer);
        }
    }
    
    private Listener listener = null;

    private void Start()
    {
        listener = gameObject.AddComponent<Listener>();
    }

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

        yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(listener.deadCard, null, 0.25f, null);
        yield return new WaitForSeconds(0.45f);
			
        if (currentView != View.Hand)
        {
            yield return new WaitForSeconds(0.2f);
            Singleton<ViewManager>.Instance.SwitchToView(currentView, false, false);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public override bool ExtraActivationPrerequisitesMet()
    {
        return listener != null && listener.deadCard != null;
    }
}