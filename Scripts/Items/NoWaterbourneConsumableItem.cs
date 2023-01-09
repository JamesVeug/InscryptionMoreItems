using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace MoreItemsMod;

/// <summary>
/// Adds brittle to all cards on the board
/// </summary>
public class NoWaterbourneConsumableItem : ConsumableItem
{
    public override IEnumerator ActivateSequence()
    {
        base.PlayExitAnimation();
        yield return new WaitForSeconds(0.25f);
        Singleton<ViewManager>.Instance.SwitchToView(View.Board, false, false);
        yield return new WaitForSeconds(0.2f);

        List<CardSlot> slots = new(Singleton<BoardManager>.Instance.AllSlots.FindAll((a)=>a.Card != null && a.Card.OpponentCard && a.Card.HasAbility(Ability.Submerge)));
        if (slots.Count > 0)
        {
            foreach (CardSlot cardSlot in slots)
            {
                cardSlot.Card.Anim.SetFaceDown(false);
            }
            yield return new WaitForSeconds(0.1f);
        }
        
        if (slots.Count > 0)
        {
            foreach (CardSlot cardSlot in slots)
            {
                CardModificationInfo mod = new CardModificationInfo()
                {
                    negateAbilities = new List<Ability>()
                    {
                        Ability.Submerge
                    }
                };
                cardSlot.Card.AddTemporaryMod(mod);
                cardSlot.Card.Anim.LightNegationEffect();
                yield return new WaitForSeconds(0.1f);
            }
        }


        yield return new WaitForSeconds(0.5f);
    }
}