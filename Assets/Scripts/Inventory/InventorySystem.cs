using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InventorySystem : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int quantity;
        public bool isEquipped;
    }

    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = 20;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask itemLayer;

    private List<InventorySlot> inventory = new List<InventorySlot>();
    private Camera playerCamera;

    private void Awake()
    {
        playerCamera = Camera.main;
    }

    private void Update()
    {
        // Check for item pickup input
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupItem();
        }
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        // Check if item is stackable and already exists in inventory
        if (item.isStackable)
        {
            var existingSlot = inventory.FirstOrDefault(slot => slot.item == item && slot.quantity < item.maxStackSize);
            if (existingSlot != null)
            {
                int spaceInStack = item.maxStackSize - existingSlot.quantity;
                int amountToAdd = Mathf.Min(quantity, spaceInStack);
                existingSlot.quantity += amountToAdd;
                quantity -= amountToAdd;
            }
        }

        // If we still have items to add and space in inventory
        while (quantity > 0 && inventory.Count < maxSlots)
        {
            int amountToAdd = item.isStackable ? Mathf.Min(quantity, item.maxStackSize) : 1;
            inventory.Add(new InventorySlot
            {
                item = item,
                quantity = amountToAdd,
                isEquipped = false
            });
            quantity -= amountToAdd;
        }

        return quantity <= 0;
    }

    public bool RemoveItem(ItemData item, int quantity = 1)
    {
        int remainingToRemove = quantity;
        var slotsToRemove = new List<InventorySlot>();

        foreach (var slot in inventory.Where(s => s.item == item))
        {
            if (remainingToRemove <= 0) break;

            if (slot.quantity <= remainingToRemove)
            {
                remainingToRemove -= slot.quantity;
                slotsToRemove.Add(slot);
            }
            else
            {
                slot.quantity -= remainingToRemove;
                remainingToRemove = 0;
            }
        }

        foreach (var slot in slotsToRemove)
        {
            inventory.Remove(slot);
        }

        return remainingToRemove <= 0;
    }

    public bool HasItem(ItemData item, int quantity = 1)
    {
        int count = inventory.Where(s => s.item == item).Sum(s => s.quantity);
        return count >= quantity;
    }

    public void EquipItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Count) return;

        var slot = inventory[slotIndex];
        if (slot.item.isEquippable)
        {
            // Unequip any currently equipped item of the same type
            var equippedSlot = inventory.FirstOrDefault(s => s.isEquipped && s.item.itemType == slot.item.itemType);
            if (equippedSlot != null)
            {
                equippedSlot.isEquipped = false;
            }

            slot.isEquipped = true;
            // Here you would typically instantiate the item model or apply its effects
        }
    }

    private void TryPickupItem()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, itemLayer))
        {
            var itemPickup = hit.collider.GetComponent<ItemPickup>();
            if (itemPickup != null)
            {
                if (AddItem(itemPickup.itemData, itemPickup.quantity))
                {
                    Destroy(itemPickup.gameObject);
                }
            }
        }
    }

    // Getters for UI
    public List<InventorySlot> GetInventory() => inventory;
    public int GetMaxSlots() => maxSlots;
    public int GetCurrentSlots() => inventory.Count;
} 