using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace MoreItemsMod;

/// <summary>
/// Adds brittle to all cards on the board
/// </summary>
public class PickAxeConsumableItem : ConsumableItem
{
    public override IEnumerator ActivateSequence()
    {
        base.PlayExitAnimation();
        
        foreach (CardSlot cardSlot in Singleton<BoardManager>.Instance.opponentSlots)
        {
            if (cardSlot.Card != null && !cardSlot.Card.HasTrait(Trait.Uncuttable))
            {
                yield return StrikeCardSlot(cardSlot);
                Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
            }
        }
        
        // Look at board
        yield return new WaitForSeconds(0.2f);
    }
    
    public IEnumerator StrikeCardSlot(CardSlot slot)
    {
        Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, false);
        yield return new WaitForSeconds(0.1f);
        AudioController.Instance.PlaySound3D("metal_object_hit#1", MixerGroup.TableObjectsSFX, slot.transform.position, 1f, 0f, new AudioParams.Pitch(AudioParams.Pitch.Variation.Medium), null, null, null, false);
        if (slot.Card != null)
        {
            yield return slot.Card.Die(false, null, true);
            if (slot.Card == null)
            {
                yield return new WaitForSeconds(0.25f);
                yield return Singleton<BoardManager>.Instance.CreateCardInSlot(CardLoader.GetCardByName("GoldNugget"), slot, 0.1f, true);
            }
        }
        yield return new WaitForSeconds(0.35f);
        Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, false);
        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
    }
}