using UnityEngine;

public class Enemy_Controler : MonoBehaviour
{
    public Transform pointA, pointB;
    public float speed = 3f;
    public float idleTime = 2f;
    public float detectionRange = 5f;
    public float attackRange = 2f;
    public int maxMovesBeforeRest = 3;
    public int maxHealth = 100;

    private int currentHealth;
    private Transform targetPoint;
    private int moveCount = 0;
    private float idleTimer = 0f;
    private bool isIdle = false;
    private bool isDead = false;

    private Animator animator;
    private Transform player;
    private enum BossState { Idle, Run, Attack }
    private BossState currentState = BossState.Idle;

    void Start()
    {
        animator = GetComponent<Animator>();
        targetPoint = pointB;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
        SetState(BossState.Run);
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Attack if close enough
        if (distanceToPlayer <= attackRange)
        {
            SetState(BossState.Attack);
            FaceTarget(player.position);
            return;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            SetState(BossState.Run);
            MoveTowards(player.position);
            return;
        }

        // Idle pause
        if (isIdle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                isIdle = false;
                idleTimer = 0f;
                SetState(BossState.Run);
            }
            return;
        }

        // Move A <-> B
        MoveTowards(targetPoint.position);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            moveCount++;
            targetPoint = (targetPoint == pointA) ? pointB : pointA;

            if (moveCount >= maxMovesBeforeRest)
            {
                moveCount = 0;
                isIdle = true;
                SetState(BossState.Idle);
            }
        }
    }

    void MoveTowards(Vector3 target)
    {
        SetState(BossState.Run);
        FaceTarget(target);
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        if (direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void SetState(BossState state)
    {
        if (currentState == state) return;
        currentState = state;

        animator.SetBool("isIdle", state == BossState.Idle);
        animator.SetBool("isRunning", state == BossState.Run);
        animator.SetBool("isAttacking", state == BossState.Attack);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        SetState(BossState.Idle); // Clear animation states
        animator.SetTrigger("Die");

        // Optional: Disable collider, movement, etc.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Optional: Destroy object after delay
        Destroy(gameObject, 2f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(20); // Gây 20 sát thương khi bị bắn
            Destroy(other.gameObject);
        }
    }
}
