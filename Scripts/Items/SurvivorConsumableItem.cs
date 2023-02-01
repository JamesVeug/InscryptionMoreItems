using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace MoreItemsMod;

public class SurvivorConsumableItem : ConsumableItem
{
    private CardSlot selectedSlot = null;
    private static int totalUsedItems = 0;
    
    public override IEnumerator ActivateSequence()
    {
        selectedSlot = null;
        
        base.PlayExitAnimation();
        yield return new WaitForSeconds(0.25f);

        // Choose slots
        BoardManager boardManager = Singleton<BoardManager>.Instance;
        Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(boardManager.ChoosingSlotViewMode, false);
        
        yield return ChooseCard();
        
        int currentRandomSeed = SaveManager.SaveFile.GetCurrentRandomSeed() + totalUsedItems++;
        
        bool destroy = SeededRandom.Range(0, 100, currentRandomSeed++) < 25f;
        if (destroy)
        {
            yield return selectedSlot.Card.Die(false, null, true);
        }
        else
        {
            CardModificationInfo mod = new CardModificationInfo();
            
            bool attackBuff = SeededRandom.Range(0, 100, currentRandomSeed++) < 50f;
            if (attackBuff)
            {
                mod.attackAdjustment = 1;
            }
            else
            {
                mod.healthAdjustment = 2;
            }
            
            selectedSlot.Card.Anim.PlayTransformAnimation();
            yield return new WaitForSeconds(0.15f);
            selectedSlot.Card.AddTemporaryMod(mod);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ChooseCard()
    {
        CombatPhaseManager combatPhaseManager = Singleton<CombatPhaseManager>.Instance;
        BoardManager boardManager = Singleton<BoardManager>.Instance;
        List<CardSlot> allSlots = new List<CardSlot>(boardManager.playerSlots);

        Action<CardSlot> callback1 = null;
        Action<CardSlot> callback2 = null;
	        
        //combatPhaseManager.VisualizeStartSniperAbility(Card.slot);
	        
        CardSlot cardSlot = Singleton<InteractionCursor>.Instance.CurrentInteractable as CardSlot;
        if (cardSlot != null && allSlots.Contains(cardSlot))
        {
            //combatPhaseManager.VisualizeAimSniperAbility(Card.slot, cardSlot);
        }

        List<CardSlot> allTargetSlots = allSlots;
        List<CardSlot> validTargetSlots = ValidTargets();

        Action<CardSlot> targetSelectedCallback;
        if ((targetSelectedCallback = callback1) == null)
        {
            targetSelectedCallback = (callback1 = delegate(CardSlot s)
            {
                selectedSlot = s;
                //combatPhaseManager.VisualizeConfirmSniperAbility(s);
            });
        }
	        
        Action<CardSlot> invalidTargetCallback = null;
        Action<CardSlot> slotCursorEnterCallback;
        if ((slotCursorEnterCallback = callback2) == null)
        {
            slotCursorEnterCallback = (callback2 = delegate(CardSlot s)
            {
                //combatPhaseManager.VisualizeAimSniperAbility(Card.slot, s);
            });
        }

        yield return boardManager.ChooseTarget(allTargetSlots, validTargetSlots, targetSelectedCallback, invalidTargetCallback, slotCursorEnterCallback, () => false, CursorType.Target);
    }

    public override bool ExtraActivationPrerequisitesMet()
    {
        return ValidTargets().Count > 0;
    }

    private static List<CardSlot> ValidTargets()
    {
        return Singleton<BoardManager>.Instance.playerSlots.FindAll((a) => a.Card != null);
    }
}