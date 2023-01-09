using System.Collections;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace MoreItemsMod;

/// <summary>
/// Adds brittle to all cards on the board
/// </summary>
public class ShuffleSigilsConsumableItem : ConsumableItem
{
    public override IEnumerator ActivateSequence()
    {
        base.PlayExitAnimation();
        
        // Look at board
        yield return new WaitForSeconds(0.25f);
        Singleton<ViewManager>.Instance.SwitchToView(View.Board, false, false);
        yield return new WaitForSeconds(0.2f);
        
        // Setup
        List<CardSlot> slots = Singleton<BoardManager>.Instance.AllSlots.FindAll((a)=>a.Card != null && !a.Card.HasTrait(Trait.Giant));
        Dictionary<CardSlot, int> slotAbilityCount = new Dictionary<CardSlot, int>();
        slots.Sort(SortSlots);
        List<Ability> abilities = new();

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
        
        // Assign sigil count
        float sigilSpread = abilities.Count % slots.Count;
        foreach (CardSlot slot in slots)
        {
            slotAbilityCount[slot] = (int)sigilSpread;
        }

        if (Math.Abs(sigilSpread % 1) > (Double.Epsilon * 100))
        {
            // Not evenly spread so a random card gets an extra sigil
            int index = UnityEngine.Random.RandomRangeInt(0, slots.Count);
            CardSlot slot = slots[index];
            slotAbilityCount[slot]++;
        }
        
        
        // Remove all abilities
        foreach (CardSlot cardSlot in slots)
        {
            List<Ability> allAbilities = cardSlot.Card.AllAbilities();
            foreach (CardModificationInfo tempMod in cardSlot.Card.temporaryMods.Concat(cardSlot.Card.Info.mods))
            {
                allAbilities.RemoveAll((a)=>tempMod.negateAbilities.Contains(a));
            }
            allAbilities = AbilitiesUtil.RemoveNonDistinctNonStacking(allAbilities);

            abilities.AddRange(allAbilities);
            slotAbilityCount[cardSlot] = allAbilities.Count;
            Plugin.Log.LogInfo($"{cardSlot.Card.Info.displayedName} has {allAbilities.Count} abilities");
            RemoveCardAbilities(cardSlot.Card);
            
            // Show effect
            SpawnSplatter(cardSlot.Card);
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(0.5f);
        
        // Shuffle abilities
        abilities.Sort(Shuffle);

        // Add all abilities to random cards keeping the same amount of sigils they had before
        foreach (KeyValuePair<CardSlot, int> slotToAbilityCount in slotAbilityCount)
        {
            CardModificationInfo modificationInfo = new CardModificationInfo();
            PlayableCard card = slotToAbilityCount.Key.Card;
            
            // Add X unique abilities
            for (int i = abilities.Count - 1; i >= 0; i--)
            {
                Ability ability = abilities[i];
                if (!modificationInfo.abilities.Contains(ability))
                {
                    modificationInfo.abilities.Add(ability);
                    abilities.RemoveAt(i);
                    Plugin.Log.LogInfo($"{card.Info.displayedName} got " + AbilitiesUtil.GetInfo(ability).rulebookName);
                    if (modificationInfo.abilities.Count >= slotToAbilityCount.Value)
                    {
                        break;
                    }
                }
            }
            
            // If we don't have X due to uniqueness then just add any
            while (modificationInfo.abilities.Count < slotToAbilityCount.Value && abilities.Count > 0)
            {
                Ability ability = abilities[abilities.Count - 1];
                modificationInfo.abilities.Add(ability);
                abilities.RemoveAt(abilities.Count - 1);
                Plugin.Log.LogInfo($"{card.Info.displayedName} got2 " + AbilitiesUtil.GetInfo(ability).rulebookName);
                if (modificationInfo.abilities.Count >= slotToAbilityCount.Value)
                {
                    break;
                }
            }
            
            // Remove negates with these abilities
            foreach (Ability newAbility in modificationInfo.abilities)
            {
                foreach (CardModificationInfo temporaryMod in card.temporaryMods)
                {
                    if(temporaryMod.negateAbilities.Contains(newAbility))
                    {
                        temporaryMod.negateAbilities.Remove(newAbility);
                    }
                }
            }

            
            // Add abilities
            card.AddTemporaryMod(modificationInfo);
                
            // Show effect
            SpawnSplatter(card);
            yield return new WaitForSeconds(0.25f);
        }
        
        // Flip all submerged cards up
        submergedCards = slots.FindAll(cardSlot=>cardSlot.Card != null && cardSlot.Card.OpponentCard &&
                                                                cardSlot.Card.HasAbility(Ability.Submerge));
        if (submergedCards.Count > 0)
        {
            foreach (CardSlot cardSlot in submergedCards)
            {
                cardSlot.Card.Anim.SetFaceDown(false);
            }
            yield return new WaitForSeconds(0.2f);
        }
        
        yield return new WaitForSeconds(1.25f);
    }

    public override bool ExtraActivationPrerequisitesMet()
    {
        // At least 1 card on the board without Brittle
        return Singleton<BoardManager>.Instance.AllSlots.Find((a) => a.Card != null && !a.Card.HasAbility(Ability.Brittle));
    }

    private static int Shuffle(Ability a, Ability b)
    {
        return UnityEngine.Random.RandomRangeInt(-1, 1);
    }

    private static int SortSlots(CardSlot a, CardSlot b)
    {
        // Opponent cards first then sort by index
        if (a.Card.OpponentCard != b.Card.OpponentCard)
        {
            if (a.Card.OpponentCard)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        return a.Index - b.Index;
    }

    private void RemoveCardAbilities(PlayableCard card)
    {
        CardModificationInfo cardModificationInfo = new CardModificationInfo();
        cardModificationInfo.negateAbilities = new List<Ability>();
        foreach (CardModificationInfo cardModificationInfo2 in card.TemporaryMods)
        {
            cardModificationInfo.negateAbilities.AddRange(cardModificationInfo2.abilities);
        }
        cardModificationInfo.negateAbilities.AddRange(card.Info.Abilities);
        card.AddTemporaryMod(cardModificationInfo);
    }
    
    private void SpawnSplatter(PlayableCard card)
    {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Items/BleachSplatter"));
        gameObject.transform.position = card.transform.position + new Vector3(0f, 0.1f, -0.25f);
        UnityEngine.Object.Destroy(gameObject, 5f);
    }
}