using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using Pixelplacement;
using UnityEngine;

namespace MoreItemsMod;

/// <summary>
/// Adds brittle to all cards on the board
/// </summary>
public class CameraConsumableItem : TargetSlotItem
{
    public override string FirstPersonPrefabId => "CameraLoadAnimation";
    public override Vector3 FirstPersonItemPos => new Vector3(0f, -100.25f, 4f);
    public override Vector3 FirstPersonItemEulers => new Vector3(0f, 0f, 0f);
    public override View SelectionView => View.OpponentQueue;
    public override CursorType SelectionCursorType => CursorType.Photo;
    
    public override IEnumerator OnValidTargetSelected(CardSlot target, GameObject firstPersonItem)
    {
        AudioController.Instance.PlaySound2D("camera_flash_shorter", MixerGroup.TableObjectsSFX, 0.75f, 0f, null, null, null, null, false);
        Singleton<UIManager>.Instance.Effects.GetEffect<ScreenColorEffect>().SetColor(GameColors.Instance.nearWhite);
        Singleton<UIManager>.Instance.Effects.GetEffect<ScreenColorEffect>().SetIntensity(1f, 100f);
        yield return new WaitForSeconds(0.1f);

        CardInfo clone = (CardInfo)target.Card.Info.Clone();
        List<CardModificationInfo> mods = new List<CardModificationInfo>(target.Card.TemporaryMods);
        yield return target.Card.TakeDamage(target.Card.Health, null);
        Singleton<UIManager>.Instance.Effects.GetEffect<ScreenColorEffect>().SetIntensity(0f, 6f);
        yield return new WaitForSeconds(1f);
        Singleton<ViewManager>.Instance.SwitchToView(View.Default, false, false);
        yield return new WaitForSeconds(0.25f);
        
        yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(clone, mods, 0.25f, null);
        
        yield return new WaitForSeconds(1f);
    }
    
    public override List<CardSlot> GetAllTargets()
    {
        return Singleton<BoardManager>.Instance.AllSlots.FindAll((a)=>a.Card != null);
    }

    
    public override List<CardSlot> GetValidTargets()
    {
        List<CardSlot> opponentSlotsCopy = GetAllTargets();
        opponentSlotsCopy.RemoveAll((CardSlot x) => x.Card.Info.HasTrait(Trait.Uncuttable));
        return opponentSlotsCopy;
    }
}