using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName = "New Item";
    public string description = "Item description";
    public Sprite icon;
    public GameObject itemPrefab;

    [Header("Item Properties")]
    public bool isStackable = true;
    public int maxStackSize = 99;
    public bool isEquippable = false;
    public ItemType itemType = ItemType.Misc;

    [Header("Item Stats")]
    public float damage = 0f;
    public float defense = 0f;
    public float durability = 100f;
    public float weight = 1f;

    [Header("Consumable Properties")]
    public bool isConsumable = false;
    public float healthRestore = 0f;
    public float staminaRestore = 0f;
    public float hungerRestore = 0f;
    public float thirstRestore = 0f;

    [Header("Ammunition Properties")]
    public bool isAmmunition = false;
    public AmmoType ammoType = AmmoType.None;
    public int ammoAmount = 1;

    [Header("Crafting Properties")]
    public bool isCraftable = false;
    public CraftingRecipe[] craftingRecipes;
}

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Ammunition,
    Material,
    Tool,
    Key,
    Quest,
    Misc
}

public enum AmmoType
{
    None,
    Pistol,
    Rifle,
    Shotgun,
    Sniper,
    Special
}

[System.Serializable]
public class CraftingRecipe
{
    public ItemData[] requiredItems;
    public int[] requiredAmounts;
    public ItemData craftedItem;
    public int craftedAmount = 1;
    public float craftingTime = 0f;
} 