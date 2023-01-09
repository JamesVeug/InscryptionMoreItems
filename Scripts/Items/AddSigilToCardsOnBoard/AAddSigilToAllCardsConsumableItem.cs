using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace MoreItemsMod;

public abstract class AAddSigilToAllCardsConsumableItem : ConsumableItem
{
    protected abstract Ability Ability { get; }
    
    public override IEnumerator ActivateSequence()
    {
        base.PlayExitAnimation();
        yield return new WaitForSeconds(0.25f);
        Singleton<ViewManager>.Instance.SwitchToView(View.Board, false, false);
        yield return new WaitForSeconds(0.2f);

        List<CardSlot> slots = new(Singleton<BoardManager>.Instance.AllSlots);
        slots.RemoveAll((a) => a.Card == null || a.Card.HasTrait(Trait.Uncuttable));
        slots.Sort((a,b)=>a.Index - b.Index);

        // Flip all submerged cards up
        List<CardSlot> submergedCards = slots.FindAll(cardSlot=>cardSlot.Card != null && cardSlot.Card.OpponentCard &&
                                                           cardSlot.Card.HasAbility(Ability.Submerge));
        if (submergedCards.Count > 0)
        {
            foreach (CardSlot cardSlot in submergedCards)
            {
                cardSlot.Card.Anim.SetFaceDown(false);
            }
            yield return new WaitForSeconds(0.2f);
        }

        int slot = 0;
        foreach (CardSlot cardSlot in slots)
        {
            if (cardSlot.Card != null && !cardSlot.Card.Info.abilities.Contains(Ability))
            {
                if (slot != cardSlot.Index)
                {
                    yield return new WaitForSeconds(0.5f);
                    slot = cardSlot.Index;
                }
                
                cardSlot.Card.AddTemporaryMod(new CardModificationInfo(Ability));
            }
        }
        
        // Flip all submerged cards down
        if (submergedCards.Count > 0)
        {
            yield return new WaitForSeconds(0.2f);
            foreach (CardSlot cardSlot in slots)
            {
                if (cardSlot.Card != null && cardSlot.Card.OpponentCard && cardSlot.Card.HasAbility(Ability.Submerge))
                {
                    cardSlot.Card.Anim.SetFaceDown(true);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    public override bool ExtraActivationPrerequisitesMet()
    {
        return Singleton<BoardManager>.Instance.AllSlots.Find((a) => a.Card != null && !a.Card.HasAbility(Ability));
    }
}