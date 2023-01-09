using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Saves;
using UnityEngine;

namespace MoreItemsMod;

[HarmonyPatch]
internal class CardStatBoostSequencer_StatBoostSequence_Patch
{
    static Type CardStatBoostSequencer_StatBoostSequence = Type.GetType("DiskCardGame.CardStatBoostSequencer+<StatBoostSequence>d__12, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
    
    static MethodInfo HasItemMethod = AccessTools.Method(typeof(CardStatBoostSequencer_StatBoostSequence_Patch), nameof(HasCardSavingItem), new Type[] { });
    static MethodInfo PostGamestateDoneMethod = AccessTools.Method(typeof(CardStatBoostSequencer_StatBoostSequence_Patch), nameof(PostGameStateDone), new Type[] { });
    static MethodInfo GameFlowManagerTransitionToGameStateMethod = AccessTools.Method(typeof(GameFlowManager), nameof(GameFlowManager.TransitionToGameState), new Type[] { typeof(GameState), typeof(NodeData)});
    static MethodInfo CardGetInfoMethod = AccessTools.PropertyGetter(typeof(Card), nameof(Card.Info));
    
    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(CardStatBoostSequencer_StatBoostSequence, "MoveNext");
    }
    
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // === We want to turn this

        // <destroyedCard>5__2 = cardStatBoostSequencer.selectionSlot.Card.Info;

        // === Into this

        // if (HasCardSavingItem())
        //   Goto ble.un.s IL_08c4
        // <destroyedCard>5__2 = cardStatBoostSequencer.selectionSlot.Card.Info;

        // ===

        FieldInfo destroyedCardField = AccessTools.Field(CardStatBoostSequencer_StatBoostSequence, "<destroyedCard>5__2");
        
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            CodeInstruction codeInstruction = codes[i];
            
            // Look for where we assigned destroyedCard
            if (codeInstruction.opcode == OpCodes.Ldloc_3)
            {
                codes.Insert(++i, new CodeInstruction(OpCodes.Ldc_R4, 0.0f));
                codes.Insert(++i, new CodeInstruction(OpCodes.Mul));
                break;
            }
        }
        
        for (int i = 1; i < codes.Count; i++)
        {
            CodeInstruction codeInstruction = codes[i];
            CodeInstruction previousCodeInstruction = codes[i-1];
            
            // Look for where we assigned destroyedCard
            if (codeInstruction.opcode == OpCodes.Stfld && codeInstruction.operand == destroyedCardField)
            {
                if (previousCodeInstruction.opcode == OpCodes.Callvirt && previousCodeInstruction.operand == CardGetInfoMethod)
                {
                    for (int j = i - 1; j >= 0; --j)
                    {
                        // Look for the last if statement
                        CodeInstruction innerCodeInstruction = codes[j];
                        if (innerCodeInstruction.opcode == OpCodes.Ble_Un)
                        {
                            codes.Insert(++j, new CodeInstruction(OpCodes.Callvirt, HasItemMethod));
                            codes.Insert(++j, new CodeInstruction(OpCodes.Brtrue, innerCodeInstruction.operand));
                            break;
                        }
                    }
                    
                    break;
                }
            }
        }
        
        for (int i = codes.Count-1; i >= 0; --i)
        {
            CodeInstruction codeInstruction = codes[i];
            
            // Look for where we assigned destroyedCard
            if (codeInstruction.opcode == OpCodes.Callvirt && codeInstruction.operand == GameFlowManagerTransitionToGameStateMethod)
            {
                codes.Insert(++i, new CodeInstruction(OpCodes.Callvirt, PostGamestateDoneMethod));
                break;
            }
        }

        return codes;
    }
    
    public static bool HasCardSavingItem()
    {
        bool valueAsBoolean = ModdedSaveManager.SaveData.GetValueAsBoolean(Plugin.PluginDirectory, "HasUsedFlameSavingItem");
        return valueAsBoolean;
    }
    
    public static void PostGameStateDone()
    {
        ModdedSaveManager.SaveData.SetValue(Plugin.PluginDirectory, "HasUsedFlameSavingItem", false);
    }
}

/// <summary>
/// Adds brittle to all cards on the board
/// </summary>
public class SaveFromFlameConsumableItem : ConsumableItem
{
    public override IEnumerator ActivateSequence()
    {
        this.PlayExitAnimation();

        ModdedSaveManager.SaveData.SetValue(Plugin.PluginDirectory, "HasUsedFlameSavingItem", true);
        
        yield return new WaitForSeconds(1);
        
        
        CardStatBoostSequencer sequence = Singleton<SpecialNodeHandler>.Instance.cardStatBoostSequencer;
        foreach (CompositeFigurine figurine in sequence.figurines)
        {
            figurine.gameObject.SetActive(false);
        }
        
        yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("The mystical power of that item scared the survivors away.", 2.5f, 0f, Emotion.Laughter);
    }

    public override bool ExtraActivationPrerequisitesMet()
    {
        GameState gameState = Singleton<GameFlowManager>.Instance.CurrentGameState;
        if (gameState != GameState.SpecialCardSequence)
        {
            return false;
        }
        
        int nodeId = RunState.Run.currentNodeId;
        MapNode mapNode = Singleton<MapNodeManager>.Instance.GetNodeWithId(nodeId);
        if (mapNode.Data is not CardStatBoostNodeData)
        {
            return false;
        }

        return !ModdedSaveManager.SaveData.GetValueAsBoolean(Plugin.PluginDirectory, "HasUsedFlameSavingItem");
    }
}