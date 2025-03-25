using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float healthRegenRate = 0f;
    [SerializeField] private float healthRegenDelay = 5f;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float staminaRegenDelay = 1f;

    [Header("Hunger Settings")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hungerDecayRate = 1f;
    [SerializeField] private float hungerDamageRate = 1f;

    [Header("Thirst Settings")]
    [SerializeField] private float maxThirst = 100f;
    [SerializeField] private float thirstDecayRate = 1.5f;
    [SerializeField] private float thirstDamageRate = 1.5f;

    [Header("Infection Settings")]
    [SerializeField] private float infectionRate = 0.1f;
    [SerializeField] private float maxInfection = 100f;
    [SerializeField] private float infectionDamageRate = 2f;

    // Current values
    private float currentHealth;
    private float currentStamina;
    private float currentHunger;
    private float currentThirst;
    private float currentInfection;

    // Timers
    private float lastDamageTime;
    private float lastStaminaUseTime;

    // Events
    public UnityEvent<float> onHealthChanged = new UnityEvent<float>();
    public UnityEvent<float> onStaminaChanged = new UnityEvent<float>();
    public UnityEvent<float> onHungerChanged = new UnityEvent<float>();
    public UnityEvent<float> onThirstChanged = new UnityEvent<float>();
    public UnityEvent<float> onInfectionChanged = new UnityEvent<float>();
    public UnityEvent onPlayerDeath = new UnityEvent();

    private void Start()
    {
        // Initialize values
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentInfection = 0f;

        // Trigger initial events
        onHealthChanged.Invoke(currentHealth);
        onStaminaChanged.Invoke(currentStamina);
        onHungerChanged.Invoke(currentHunger);
        onThirstChanged.Invoke(currentThirst);
        onInfectionChanged.Invoke(currentInfection);
    }

    private void Update()
    {
        HandleRegeneration();
        HandleHungerAndThirst();
        HandleInfection();
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        lastDamageTime = Time.time;
        onHealthChanged.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (currentHealth >= maxHealth) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHealthChanged.Invoke(currentHealth);
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina < amount) return false;

        currentStamina -= amount;
        lastStaminaUseTime = Time.time;
        onStaminaChanged.Invoke(currentStamina);
        return true;
    }

    public void RestoreStamina(float amount)
    {
        if (currentStamina >= maxStamina) return;

        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        onStaminaChanged.Invoke(currentStamina);
    }

    public void ConsumeFood(float amount)
    {
        if (currentHunger >= maxHunger) return;

        currentHunger = Mathf.Min(maxHunger, currentHunger + amount);
        onHungerChanged.Invoke(currentHunger);
    }

    public void ConsumeWater(float amount)
    {
        if (currentThirst >= maxThirst) return;

        currentThirst = Mathf.Min(maxThirst, currentThirst + amount);
        onThirstChanged.Invoke(currentThirst);
    }

    public void AddInfection(float amount)
    {
        if (currentInfection >= maxInfection) return;

        currentInfection = Mathf.Min(maxInfection, currentInfection + amount);
        onInfectionChanged.Invoke(currentInfection);
    }

    private void HandleRegeneration()
    {
        // Health regeneration
        if (healthRegenRate > 0 && Time.time - lastDamageTime >= healthRegenDelay)
        {
            Heal(healthRegenRate * Time.deltaTime);
        }

        // Stamina regeneration
        if (Time.time - lastStaminaUseTime >= staminaRegenDelay)
        {
            RestoreStamina(staminaRegenRate * Time.deltaTime);
        }
    }

    private void HandleHungerAndThirst()
    {
        // Hunger decay
        if (currentHunger > 0)
        {
            currentHunger = Mathf.Max(0, currentHunger - hungerDecayRate * Time.deltaTime);
            onHungerChanged.Invoke(currentHunger);

            if (currentHunger <= 0)
            {
                TakeDamage(hungerDamageRate * Time.deltaTime);
            }
        }

        // Thirst decay
        if (currentThirst > 0)
        {
            currentThirst = Mathf.Max(0, currentThirst - thirstDecayRate * Time.deltaTime);
            onThirstChanged.Invoke(currentThirst);

            if (currentThirst <= 0)
            {
                TakeDamage(thirstDamageRate * Time.deltaTime);
            }
        }
    }

    private void HandleInfection()
    {
        if (currentInfection > 0)
        {
            TakeDamage(infectionDamageRate * Time.deltaTime);
        }
    }

    private void Die()
    {
        onPlayerDeath.Invoke();
        // Handle death (e.g., show death screen, respawn, etc.)
        Debug.Log("Player died!");
    }

    // Getters for UI
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public float GetStaminaPercentage() => currentStamina / maxStamina;
    public float GetHungerPercentage() => currentHunger / maxHunger;
    public float GetThirstPercentage() => currentThirst / maxThirst;
    public float GetInfectionPercentage() => currentInfection / maxInfection;
} 