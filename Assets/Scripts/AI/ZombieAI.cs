using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Zombie Type")]
    [SerializeField] private ZombieType zombieType = ZombieType.Walker;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float runSpeed = 5f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private Vector3 startPosition;
    private float currentWaitTime;
    private float currentAttackCooldown;

    private enum ZombieType
    {
        Walker,
        Runner,
        Tank
    }

    private enum AIState
    {
        Patrol,
        Chase,
        Attack,
        Wait
    }

    private AIState currentState = AIState.Patrol;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Set speeds based on zombie type
        switch (zombieType)
        {
            case ZombieType.Walker:
                agent.speed = moveSpeed;
                break;
            case ZombieType.Runner:
                agent.speed = runSpeed;
                break;
            case ZombieType.Tank:
                agent.speed = moveSpeed * 0.8f;
                break;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distanceToStart = Vector3.Distance(transform.position, startPosition);

        // State machine
        switch (currentState)
        {
            case AIState.Patrol:
                HandlePatrol();
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = AIState.Chase;
                    agent.speed = runSpeed;
                }
                break;

            case AIState.Chase:
                HandleChase();
                if (distanceToPlayer <= attackRange)
                {
                    currentState = AIState.Attack;
                    currentAttackCooldown = attackCooldown;
                }
                else if (distanceToPlayer > detectionRange * 1.5f)
                {
                    currentState = AIState.Patrol;
                    agent.speed = moveSpeed;
                }
                break;

            case AIState.Attack:
                HandleAttack();
                if (distanceToPlayer > attackRange)
                {
                    currentState = AIState.Chase;
                }
                break;

            case AIState.Wait:
                HandleWait();
                break;
        }

        // Update animator
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
            animator.SetBool("IsAttacking", currentState == AIState.Attack);
        }
    }

    private void HandlePatrol()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += startPosition;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1);
            agent.SetDestination(hit.position);
        }
    }

    private void HandleChase()
    {
        agent.SetDestination(player.position);
    }

    private void HandleAttack()
    {
        // Look at player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, 10f * Time.deltaTime);

        // Attack cooldown
        if (currentAttackCooldown <= 0)
        {
            // Perform attack
            if (player.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                playerHealth.TakeDamage(attackDamage);
            }
            currentAttackCooldown = attackCooldown;
        }
        else
        {
            currentAttackCooldown -= Time.deltaTime;
        }
    }

    private void HandleWait()
    {
        if (currentWaitTime <= 0)
        {
            currentState = AIState.Patrol;
        }
        else
        {
            currentWaitTime -= Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Visualize attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Visualize patrol radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPosition, patrolRadius);
    }
} 