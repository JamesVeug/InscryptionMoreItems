using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using Pixelplacement;
using UnityEngine;

namespace MoreItemsMod;

public class MycoSawConsumableItem : ConsumableItem
{
    private List<CardSlot> selectedSlots = new List<CardSlot>();
    
    public override IEnumerator ActivateSequence()
    {
        selectedSlots.Clear();
        
        base.PlayExitAnimation();
        yield return new WaitForSeconds(0.25f);

        // Choose slots
        BoardManager boardManager = Singleton<BoardManager>.Instance;
        Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(boardManager.ChoosingSlotViewMode, false);
        
        yield return ChooseCard();
        yield return ChooseCard();
        
        // Move cards up from the table
        foreach (CardSlot slot in selectedSlots)
        {
            Tween.LocalPosition(slot.Card.transform, slot.Card.transform.localPosition + Vector3.up * 20, 0.1f, 0.05f,
                Tween.EaseOut, Tween.LoopType.None, null);
        }
        
        CardInfo mergeCards = MergeCards();
        
        // Saw blade effect
        Singleton<ViewManager>.Instance.SwitchToView(View.TableStraightDown, false, false);
        AudioController.Instance.PlaySound3D("mycologist_carnage", MixerGroup.TableObjectsSFX, LeshyAnimationController.Instance.head.transform.position, 1f, 0f, null, null, null, null, false);
        yield return new WaitForSeconds(1f);
       // Singleton<CameraEffects>.Instance.Shake(0.1f, 0.25f);
        //this.bloodParticles1.gameObject.SetActive(true);
        KillCardInSlot(selectedSlots[0]);
        yield return new WaitForSeconds(0.5f);
        //Singleton<CameraEffects>.Instance.Shake(0.05f, 0.4f);
        //this.paperParticles.gameObject.SetActive(true);
        KillCardInSlot(selectedSlots[1]);
        yield return new WaitForSeconds(1f);
        //this.bloodParticles2.gameObject.SetActive(true);
        //Singleton<CameraEffects>.Instance.Shake(0.1f, 0.4f);
        yield return new WaitForSeconds(0.5f);


        yield return new WaitForSeconds(0.25f);
        
        Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(boardManager.DefaultViewMode, false);
        Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, false);
        
        // Create card
        yield return base.StartCoroutine(Singleton<CardSpawner>.Instance.SpawnCardToHand(mergeCards, 0.5f));

        yield return new WaitForSeconds(0.5f);
    }

    private void KillCardInSlot(CardSlot slot)
    {
        PlayableCard card = slot.Card;
        if (card == null)
        {
            return;
        }
        card.Dead = true;
        card.UnassignFromSlot();
        Destroy(card.gameObject);
    }
    
    private CardInfo MergeCards()
    {
        CardInfo mergedCard = (CardInfo)selectedSlots[0].Card.Info.Clone();
        CardInfo card2 = selectedSlots[1].Card.Info;
        CardModificationInfo duplicateMod = DuplicateMergeSequencer.GetDuplicateMod(card2.Attack, card2.Health);
        int num = 0;
        foreach (CardModificationInfo cardModificationInfo in mergedCard.Mods)
        {
            if (cardModificationInfo != null && cardModificationInfo.fromCardMerge)
            {
                num += cardModificationInfo.abilities.Count;
            }
        }
        foreach (CardModificationInfo cardModificationInfo2 in card2.Mods)
        {
            if (cardModificationInfo2.fromCardMerge)
            {
                duplicateMod.fromCardMerge = true;
            }
            foreach (Ability ability in cardModificationInfo2.abilities)
            {
                if (!mergedCard.HasAbility(ability) && duplicateMod.abilities.Count + num < 4)
                {
                    duplicateMod.abilities.Add(ability);
                }
            }
        }
        
        // Extra!
        foreach (var ability in card2.abilities)
        {
            if (!mergedCard.HasAbility(ability) && !duplicateMod.abilities.Contains(ability) && duplicateMod.abilities.Count + num < 4)
            {
                duplicateMod.abilities.Add(ability);
            }
        }
        mergedCard.Mods.Add(duplicateMod);
        return mergedCard;
    }

    private static Sprite CombineTextures(Sprite sprite1, Sprite sprite2)
    {
        Texture2D card1Texture = sprite1.texture;
        Texture2D card2Texture = sprite2.texture;
        
        Texture2D combinedTexture = new Texture2D(card1Texture.width, card2Texture.height);
        Color[] pixels1 = card1Texture.GetPixels(0, 0, card1Texture.width / 2, card1Texture.height);
        Color[] pixels2 = card1Texture.GetPixels(card2Texture.width / 2, 0, card2Texture.width / 2, card2Texture.height);
        combinedTexture.SetPixels(0, 0, card2Texture.width / 2, card2Texture.height, pixels1);
        combinedTexture.SetPixels(card2Texture.width / 2, 0, card2Texture.width / 2, card2Texture.height, pixels2);
        return combinedTexture.ConvertTexture(TextureHelper.SpriteType.CardPortrait, FilterMode.Point);
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
        validTargetSlots.RemoveAll((a)=>selectedSlots.Contains(a));

        Action<CardSlot> targetSelectedCallback;
        if ((targetSelectedCallback = callback1) == null)
        {
            targetSelectedCallback = (callback1 = delegate(CardSlot s)
            {
                selectedSlots.Add(s);
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
        return ValidTargets().Count > 1;
    }

    private static List<CardSlot> ValidTargets()
    {
        return Singleton<BoardManager>.Instance.playerSlots.FindAll((a) => a.Card != null && !a.Card.HasTrait(Trait.Uncuttable));
    }
}