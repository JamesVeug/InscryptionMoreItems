using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Items;
using InscryptionAPI.Items.Extensions;
using InscryptionAPI.Pelts;
using InscryptionAPI.Sound;
using InscryptionAPI.Totems;
using MoreItemsMod.Scripts.SpecialAbilities;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Diagnostics;
using Random = UnityEngine.Random;

namespace MoreItemsMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
	    public const string PluginGuid = "jamesgames.inscryption.moreitems";
	    public const string PluginName = "More Items";
	    public const string PluginVersion = "0.2.0.0";

	    public static string PluginDirectory;
	    public static ManualLogSource Log;

        private void Awake()
        {
	        Log = Logger;
	        Log.LogInfo($"Loading {PluginName}...");
	        PluginDirectory = this.Info.Location.Replace("MoreItems.dll", "");
	        new Harmony(PluginGuid).PatchAll();
	            
	        CustomItems();
	        
	        BaitBucket.Initialize();
            
            Logger.LogInfo($"Loaded {PluginName}!");	        
        }

	    private void CustomItems()
        {
	        MakeItem("All Brittle", 
		        "All cards on the board gain Brittle", 
		        "Art/rulebookitemicon_allbrittle.png", 
		        typeof(AllBrittleItemBehaviour), 
		        ConsumableItemManager.ModelType.BasicRune);
	        
	        MakeItem("All Sharp", 
		        "All cards on the board gain Sharp", 
		        "Art/rulebookitemicon_allsharp.png", 
		        typeof(AllSharpItemBehaviour), 
		        ConsumableItemManager.ModelType.BasicRuneWithVeins);
	        
	        MakeItem("All Split Strike", 
		        "All cards on the board gain Split Strike", 
		        "Art/rulebookitemicon_allsplitstrike.png", 
		        typeof(AllSplitStrikeItemBehaviour), 
		        ConsumableItemManager.ModelType.HoveringRune);
	        
	        MakeItem("Shuffle Sigils", 
		        "All sigils on the board are removed and added to a random card", 
		        "Art/rulebookitemicon_sigilshuffle.png", 
		        typeof(ShuffleSigilsConsumableItem), 
		        ConsumableItemManager.ModelType.BasicRune);
	        
	        MakeItem("No Waterbourne", 
		        "All my cards on the board with Submerge are revealed and the sigil removed", 
		        "Art/rulebookitemicon_nowaterbourne.png", 
		        typeof(NoWaterbourneConsumableItem), 
		        ConsumableItemManager.ModelType.BasicRune);
	        
	        MakeItem("Hot Meat", 
		        "Using this item at the campfire will save your card from being destroyed for a single turn.", 
		        "Art/rulebookitem_hotmeat.png", 
		        typeof(SaveFromFlameConsumableItem), 
		        ConsumableItemManager.ModelType.BasicRuneWithVeins)
		        .SetCanActivateOutsideBattles(true);

	        MakeItem("Bait Bucket",
		        "Places Bait Buckets on all empty spaces. The smell is overwhelming.",
		        "Art/rulebookitemicon_baitbucket.png",
		        typeof(BaitBucketConsumableitem),
		        ConsumableItemManager.ModelType.BasicRuneWithVeins);
	        
	        CreateMeatPileItem();
	        CreateCameraItem();
	        CreatePickaxeItem();
	        CreateMycoSawItem();
	        CreateCardPackItem();
	        CreateMrsBombItem();
        }

	    private void CreateMrsBombItem()
	    {
		    // component uses: BombRemoteItem
		    GameObject prefab = Resources.Load<GameObject>("prefabs/items/BombRemoteItem");
		    MakeItem("Mrs. Bomb's Remote", "Places Explode Bots on all empty spaces. Pretty annoying honestly.", "Art/rulebookitemicon_remotebomb.png", typeof(BombRemoteItem), prefab, "Mrs. Bomb's Remote. Click that thing and Explode Bots will drop in every open space.");
	    }

	    private void CreateMycoSawItem()
	    {
		    AssetBundle loadFromFile = AssetBundle.LoadFromFile(Path.Combine(PluginDirectory, "AssetBundles/mycosawitem"));
		    GameObject prefab = loadFromFile.LoadAsset<GameObject>("MycoSawItem");
		    ConsumableItemResource resource = new ConsumableItemResource();
		    resource.FromPrefab(prefab);
		    
		    ConsumableItemManager.ModelType modelType = ConsumableItemManager.RegisterPrefab(PluginGuid, "MycoSaw", resource);

		    MakeItem("Mycologist Saw", "Combine 2 cards on your side of the board to become one.", 
			    "Art/rulebooksaw.png",
			    typeof(MycoSawConsumableItem), 
			    modelType, "A temporary abomination if I ever 'saw' one.");
	    }

	    private void CreateCardPackItem()
	    {
		    
		    AssetBundle loadFromFile = AssetBundle.LoadFromFile(Path.Combine(PluginDirectory, "AssetBundles/cardpackitem"));
		    GameObject prefab = loadFromFile.LoadAsset<GameObject>("CardPackItem");
		    if (prefab == null)
		    {
			    Logger.LogError("CardPackItem wasn't loaded from cardpackitem bundle!");
		    }
		    
		    ConsumableItemResource resource = new ConsumableItemResource();
		    resource.FromPrefab(prefab);
		    
		    ConsumableItemManager.ModelType modelType = ConsumableItemManager.RegisterPrefab(PluginGuid, "Card Pack", resource);

		    MakeItem("Card Pack", "Adds 5 random cards to your hand.", 
			    "Art/rulebookitemicon_cardpack.png",
			    typeof(CardPackConsumableItem), 
			    modelType, "So that's why my pack mules are coming back short.");
	    }

        private void CreateMeatPileItem()
        {
	        GameObject meatPile = new GameObject("MeatPile");
	        GameObject animation = new GameObject("Anim");
	        animation.AddComponent<Animator>();
	        animation.transform.SetParent(meatPile.transform);
	        GameObject model = GameObject.Instantiate(Resources.Load<GameObject>("art/assets3d/cabin/meat/meatplate-Fin"));
	        model.transform.localScale = Vector3.one * 0.50f;
	        model.transform.localPosition = new Vector3(0, 0.23f, 0);
	        model.transform.SetParent(animation.transform);

	        model.FindChild("Meat").GetComponent<MeshRenderer>().materials = new Material[]
	        {
		        Resources.Load<Material>("art/assets3d/cabin/meat/Meat_Meat")
	        };
	        model.FindChild("CandleWax").GetComponent<MeshRenderer>().materials = new Material[]
	        {
		        Resources.Load<Material>("art/assets3d/cabin/meat/Meat_PlateAndCandle")
	        };
	        
	        MakeItem("Meat Cake", "Still fresh", "Art/rulebookitemicon_meatpile.png", typeof(MeatCakeConsumableItem), meatPile, "In case you change your mind");
        }

        private void CreatePickaxeItem()
        {
	        GameObject pickaxe = new GameObject("Pickaxe");
	        GameObject animation = new GameObject("Anim");
	        animation.AddComponent<Animator>();
	        animation.transform.SetParent(pickaxe.transform);

	        GameObject model = new GameObject("Model");
	        model.transform.SetParent(animation.transform);
	        model.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
	        model.transform.localRotation = Quaternion.Euler(0, 90, 0);
	        model.AddComponent<MeshFilter>().mesh = Resources.Load<Mesh>("art/assets3d/nodesequences/pickaxe/pickaxe");
	        model.AddComponent<MeshRenderer>().materials = new Material[]
	        {
		        Resources.Load<Material>("art/assets3d/nodesequences/pickaxe/pickaxe")
	        };
	        
	        MakeItem("Pick Axe", "All my cards on the board are replaced with a gold nugget",
		        "Art/rulebookitemicon_pickaxe.png", typeof(PickAxeConsumableItem), pickaxe, "Still sharp since the day i crafted it");
        }

        private void CreateCameraItem()
        {
	        GameObject camera = new GameObject("Camera");
	        GameObject animation = new GameObject("Anim");
	        animation.AddComponent<Animator>();
	        animation.transform.SetParent(camera.transform);

	        GameObject model = new GameObject("Model");
	        model.transform.SetParent(animation.transform);
	        model.transform.localPosition = new Vector3(0, 0.85f, 0);
	        model.transform.localRotation = Quaternion.Euler(0, 180, 0);
	        model.AddComponent<MeshFilter>().mesh = Resources.Load<Mesh>("art/assets3d/cabin/camera/leshy_camera");
	        model.AddComponent<MeshRenderer>().materials = new Material[]
	        {
		        Resources.Load<Material>("art/assets3d/cabin/camera/Leshy_Camera_2"),
		        Resources.Load<Material>("art/assets3d/cabin/camera/Leshy_Camera_1")
	        };

	        MakeItem("Camera", "Take a picture of a card on the board and add it to your deck",
		        "Art/rulebookitemicon_camera.png", typeof(CameraConsumableItem), camera, "My spare camera, use it wisely.");
        }

        private void MakeItem(string rulebookName, string rulebookDescription, string texturePath, Type type, GameObject prefab, string learnText=null)
        {
	        string path = Path.Combine(PluginDirectory, texturePath);
	        Texture2D texture = TextureHelper.GetImageAsTexture(path);
	        ConsumableItemManager.New(PluginGuid, rulebookName, rulebookDescription, texture, type, prefab)
		        .SetLearnItemDescription(learnText)
		        .SetAct1();
	        Logger.LogInfo("[Red Totem Mod] MakeItem " + rulebookName);
        }

        private ConsumableItemData MakeItem(string rulebookName, string rulebookDescription, string texturePath, Type type, ConsumableItemManager.ModelType modelType, string learnText=null)
        {
	        Logger.LogInfo("[Red Totem Mod] Making Item " + rulebookName);
	        string path = Path.Combine(PluginDirectory, texturePath);
	        Texture2D texture = TextureHelper.GetImageAsTexture(path);
	        ConsumableItemData data = ConsumableItemManager.New(PluginGuid, rulebookName, rulebookDescription, texture, type, modelType)
		        .SetLearnItemDescription(learnText)
		        .SetAct1();
	        Logger.LogInfo("[Red Totem Mod] Made Item " + data.rulebookName);
	        return data;
        }

    }
}
