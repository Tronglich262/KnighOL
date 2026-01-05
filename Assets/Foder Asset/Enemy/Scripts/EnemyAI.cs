using UnityEngine;
using Fusion;

public class EnemyAI : NetworkBehaviour
{
    [Header("Movement")]
    public float patrolDistance = 5f;
    public float moveSpeed = 2f;
    public float acceleration = 15f;

    [Header("Combat")]
    public float detectRange = 4f;      // chỉ nhìn thấy
    public float aggroRange = 1.2f;     // vào quá gần → aggro
    public float attackRange = 0.6f;
    public float attackCooldown = 1f;
    public LayerMask playerLayer;

    private Animator animator;

    private Vector3 startPos;
    private bool movingRight;

    private Transform target;
    private bool isAggro;

    private float detectTimer;
    private float currentVelX;
    private float facing = 1;

    [Networked] private TickTimer attackCooldownTimer { get; set; }

    // =========================
    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        startPos = transform.position;
        movingRight = Random.value > 0.5f;
    }

    // =========================
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        UpdateDetection();

        if (isAggro && target != null)
            UpdateChaseAndAttack();
        else
            UpdatePatrol();

        ApplyMovement();
        UpdateAnimator();
        UpdateFacing();
    }

    // =====================================================
    // DETECT + AGGRO LOGIC (CHUẨN GAME OL)
    // =====================================================
    private void UpdateDetection()
    {
        detectTimer -= Runner.DeltaTime;
        if (detectTimer > 0) return;
        detectTimer = 0.25f;

        // =========================
        // 1️⃣ ĐÃ AGGRO → GIỮ TARGET
        // =========================
        if (isAggro && target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);

            // chạy quá xa → mất aggro
            if (dist > detectRange * 1.5f)
            {
                target = null;
                isAggro = false;
            }
            return;
        }

        // =========================
        // 2️⃣ CHƯA AGGRO → CHỈ QUAN SÁT
        // =========================
        var hits = Physics.OverlapSphere(transform.position, detectRange, playerLayer);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);

            // ❗ CHỈ AGGRO KHI VÀO RẤT GẦN
            if (dist <= aggroRange)
            {
                target = hit.transform;
                isAggro = true;
                break;
            }
        }
    }

    // =====================================================
    // PATROL
    // =====================================================
    private void UpdatePatrol()
    {
        float patrolX = startPos.x + (movingRight ? patrolDistance : -patrolDistance);
        float dx = patrolX - transform.position.x;

        if (Mathf.Abs(dx) > 0.15f)
            Move(Mathf.Sign(dx));
        else
        {
            Stop();
            movingRight = !movingRight;
        }
    }

    // =====================================================
    // CHASE + ATTACK
    // =====================================================
    private void UpdateChaseAndAttack()
    {
        float dx = target.position.x - transform.position.x;
        float distance = Mathf.Abs(dx);

        if (distance > attackRange)
            Move(Mathf.Sign(dx));
        else
        {
            Stop();
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (!attackCooldownTimer.ExpiredOrNotRunning(Runner)) return;

        attackCooldownTimer = TickTimer.CreateFromSeconds(Runner, attackCooldown);
        animator.SetTrigger("AttackTrigger");
    }

    // =====================================================
    // MOVEMENT (SMOOTH)
    // =====================================================
    private void Move(float dir)
    {
        facing = dir;
        currentVelX = Mathf.MoveTowards(
            currentVelX,
            dir * moveSpeed,
            acceleration * Runner.DeltaTime
        );
    }

    private void Stop()
    {
        currentVelX = Mathf.MoveTowards(
            currentVelX,
            0,
            acceleration * Runner.DeltaTime
        );
    }

    private void ApplyMovement()
    {
        transform.position += Vector3.right * currentVelX * Runner.DeltaTime;
    }

    // =====================================================
    // ANIMATION
    // =====================================================
    private bool wasMoving;

    private void UpdateAnimator()
    {
        bool isMovingNow = Mathf.Abs(currentVelX) > 0.05f;

        if (isMovingNow && !wasMoving)
            animator.SetTrigger("MoveTrigger");

        wasMoving = isMovingNow;
    }

    private void UpdateFacing()
    {
        if (Mathf.Abs(currentVelX) < 0.01f) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facing > 0 ? -1 : 1);
        transform.localScale = scale;
    }

    // =====================================================
    // AGGRO KHI BỊ ĐÁNH (BẮT BUỘC)
    // =====================================================
    public void ForceAggro(Transform attacker)
    {
        target = attacker;
        isAggro = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
#endif
}
