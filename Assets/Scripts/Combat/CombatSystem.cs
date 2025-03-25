using UnityEngine;
using System.Collections;

public class CombatSystem : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Melee Settings")]
    [SerializeField] private float meleeAttackSpeed = 1f;
    [SerializeField] private float meleeDamage = 20f;
    [SerializeField] private float meleeStaminaCost = 10f;

    [Header("Ranged Settings")]
    [SerializeField] private float rangedAttackSpeed = 0.5f;
    [SerializeField] private float rangedDamage = 30f;
    [SerializeField] private float rangedStaminaCost = 15f;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float projectileLifetime = 3f;

    // Components
    private PlayerHealth playerHealth;
    private Animator animator;
    private Camera playerCamera;

    // Combat state
    private bool isAttacking;
    private float nextAttackTime;
    private WeaponType currentWeaponType = WeaponType.Melee;
    private GameObject currentWeapon;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();
        playerCamera = Camera.main;
    }

    private void Update()
    {
        HandleAttackInput();
    }

    private void HandleAttackInput()
    {
        if (isAttacking || Time.time < nextAttackTime) return;

        // Left click for primary attack
        if (Input.GetMouseButtonDown(0))
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        if (currentWeapon == null) return;

        switch (currentWeaponType)
        {
            case WeaponType.Melee:
                StartMeleeAttack();
                break;
            case WeaponType.Ranged:
                StartRangedAttack();
                break;
        }
    }

    private void StartMeleeAttack()
    {
        if (!playerHealth.UseStamina(meleeStaminaCost)) return;

        isAttacking = true;
        nextAttackTime = Time.time + meleeAttackSpeed;

        // Perform melee attack
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * attackRange, 1f, targetLayer);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(meleeDamage);
            }
        }

        // Play animation
        if (animator != null)
        {
            animator.SetTrigger("MeleeAttack");
        }

        StartCoroutine(ResetAttackState(meleeAttackSpeed));
    }

    private void StartRangedAttack()
    {
        if (!playerHealth.UseStamina(rangedStaminaCost)) return;

        isAttacking = true;
        nextAttackTime = Time.time + rangedAttackSpeed;

        // Spawn projectile
        if (projectileSpawnPoint != null)
        {
            GameObject projectile = CreateProjectile();
            if (projectile != null)
            {
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = projectileSpawnPoint.forward * projectileSpeed;
                }
            }
        }

        // Play animation
        if (animator != null)
        {
            animator.SetTrigger("RangedAttack");
        }

        StartCoroutine(ResetAttackState(rangedAttackSpeed));
    }

    private GameObject CreateProjectile()
    {
        // This should be implemented based on your projectile prefab and pooling system
        // For now, we'll just return null
        return null;
    }

    private IEnumerator ResetAttackState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
    }

    public void EquipWeapon(GameObject weapon, WeaponType type)
    {
        // Unequip current weapon if any
        if (currentWeapon != null)
        {
            currentWeapon.SetActive(false);
        }

        // Equip new weapon
        currentWeapon = weapon;
        currentWeaponType = type;
        if (currentWeapon != null)
        {
            currentWeapon.SetActive(true);
            currentWeapon.transform.SetParent(weaponHolder);
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, 1f);
    }
}

public enum WeaponType
{
    Melee,
    Ranged
}

public interface IDamageable
{
    void TakeDamage(float damage);
} 