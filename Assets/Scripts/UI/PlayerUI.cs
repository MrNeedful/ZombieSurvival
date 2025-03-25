using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerUI : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Stamina UI")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TextMeshProUGUI staminaText;

    [Header("Hunger UI")]
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private TextMeshProUGUI hungerText;

    [Header("Thirst UI")]
    [SerializeField] private Slider thirstSlider;
    [SerializeField] private TextMeshProUGUI thirstText;

    [Header("Infection UI")]
    [SerializeField] private Slider infectionSlider;
    [SerializeField] private TextMeshProUGUI infectionText;

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private Transform inventoryContent;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;

    [Header("Crosshair")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private float crosshairSpread = 5f;
    [SerializeField] private float crosshairRecoverySpeed = 10f;

    // Components
    private PlayerHealth playerHealth;
    private InventorySystem inventorySystem;
    private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        inventorySystem = GetComponent<InventorySystem>();

        // Subscribe to events
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(UpdateHealthUI);
            playerHealth.onStaminaChanged.AddListener(UpdateStaminaUI);
            playerHealth.onHungerChanged.AddListener(UpdateHungerUI);
            playerHealth.onThirstChanged.AddListener(UpdateThirstUI);
            playerHealth.onInfectionChanged.AddListener(UpdateInfectionUI);
        }

        // Initialize inventory UI
        if (inventorySystem != null)
        {
            InitializeInventoryUI();
        }

        // Hide inventory at start
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void Update()
    {
        HandleInventoryToggle();
        UpdateCrosshair();
    }

    private void HandleInventoryToggle()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            Cursor.lockState = inventoryPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = inventoryPanel.activeSelf;
        }
    }

    private void InitializeInventoryUI()
    {
        // Clear existing slots
        foreach (var slot in inventorySlots)
        {
            Destroy(slot.gameObject);
        }
        inventorySlots.Clear();

        // Create slots based on inventory size
        for (int i = 0; i < inventorySystem.GetMaxSlots(); i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryContent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            slotUI.Initialize(i, OnSlotSelected);
            inventorySlots.Add(slotUI);
        }
    }

    private void UpdateInventoryUI()
    {
        var inventory = inventorySystem.GetInventory();
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < inventory.Count)
            {
                inventorySlots[i].UpdateSlot(inventory[i]);
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }
    }

    private void OnSlotSelected(int slotIndex)
    {
        var inventory = inventorySystem.GetInventory();
        if (slotIndex < inventory.Count)
        {
            var item = inventory[slotIndex].item;
            itemNameText.text = item.itemName;
            itemDescriptionText.text = item.description;
        }
        else
        {
            itemNameText.text = "";
            itemDescriptionText.text = "";
        }
    }

    private void UpdateHealthUI(float health)
    {
        if (healthSlider != null)
        {
            healthSlider.value = playerHealth.GetHealthPercentage();
        }
        if (healthText != null)
        {
            healthText.text = $"Health: {Mathf.Round(health)}";
        }
    }

    private void UpdateStaminaUI(float stamina)
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = playerHealth.GetStaminaPercentage();
        }
        if (staminaText != null)
        {
            staminaText.text = $"Stamina: {Mathf.Round(stamina)}";
        }
    }

    private void UpdateHungerUI(float hunger)
    {
        if (hungerSlider != null)
        {
            hungerSlider.value = playerHealth.GetHungerPercentage();
        }
        if (hungerText != null)
        {
            hungerText.text = $"Hunger: {Mathf.Round(hunger)}";
        }
    }

    private void UpdateThirstUI(float thirst)
    {
        if (thirstSlider != null)
        {
            thirstSlider.value = playerHealth.GetThirstPercentage();
        }
        if (thirstText != null)
        {
            thirstText.text = $"Thirst: {Mathf.Round(thirst)}";
        }
    }

    private void UpdateInfectionUI(float infection)
    {
        if (infectionSlider != null)
        {
            infectionSlider.value = playerHealth.GetInfectionPercentage();
        }
        if (infectionText != null)
        {
            infectionText.text = $"Infection: {Mathf.Round(infection)}%";
        }
    }

    private void UpdateCrosshair()
    {
        if (crosshairImage == null) return;

        // Update crosshair spread based on movement and actions
        float targetSpread = crosshairSpread;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            targetSpread *= 1.5f;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            targetSpread *= 2f;
        }

        // Smoothly update crosshair spread
        Vector2 currentSize = crosshairImage.rectTransform.sizeDelta;
        Vector2 targetSize = new Vector2(targetSpread, targetSpread);
        crosshairImage.rectTransform.sizeDelta = Vector2.Lerp(currentSize, targetSize, Time.deltaTime * crosshairRecoverySpeed);
    }
} 