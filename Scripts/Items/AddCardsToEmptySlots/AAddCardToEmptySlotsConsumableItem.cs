using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace MoreItemsMod;

public abstract class AAddCardToEmptySlotsConsumableItem : ConsumableItem
{
    protected abstract CardInfo CardInfo { get; }
    
    public override IEnumerator ActivateSequence()
    {
        base.PlayExitAnimation();
        yield return new WaitForSeconds(0.25f);
        Singleton<ViewManager>.Instance.SwitchToView(View.Board, false, false);
        yield return new WaitForSeconds(0.2f);

        List<CardSlot> slots = ValidSlots();
        foreach (CardSlot cardSlot in slots)
        {
            yield return Singleton<BoardManager>.Instance.CreateCardInSlot(CardInfo, cardSlot, 0.25f, true);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private static List<CardSlot> ValidSlots()
    {
        List<CardSlot> slots = new(Singleton<BoardManager>.Instance.AllSlots);
        slots.RemoveAll((a) => a.Card != null);
        return slots;
    }

    public override bool ExtraActivationPrerequisitesMet()
    {
        return ValidSlots().Count > 0;
    }
}