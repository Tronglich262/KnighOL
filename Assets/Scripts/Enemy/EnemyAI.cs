using UnityEngine;
using Fusion;

public class EnemyAI : NetworkBehaviour
{
    [Header("Movement")]
    public float patrolDistance = 5f;
    public float moveSpeed = 2f;
    public float acceleration = 15f;

    [Header("Combat")]
    public float detectRange = 4f;      // nhìn thấy
    public float aggroRange = 1.2f;     // vào rất gần → aggro
    public float attackRange = 0.6f;
    public float attackCooldown = 1f;
    public LayerMask playerLayer;

    private Animator animator;

    private Vector3 startPos;
    private bool movingRight;

    private float detectTimer;
    private float currentVelX;
    private float facing = 1;

    [Networked] private TickTimer attackCooldownTimer { get; set; }

    // 🔥 MMO: aggro tách riêng
    private EnemyAggroSystem aggro;
    private EnemyDebuffManager debuffManager;

    // =========================
    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        aggro = GetComponent<EnemyAggroSystem>();
        debuffManager = GetComponent<EnemyDebuffManager>();

        startPos = transform.position;
        movingRight = Random.value > 0.5f;
    }

    // =========================
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (debuffManager != null && debuffManager.CannotAct)
        {
            Stop();
            ApplyMovement();
            UpdateAnimator();
            return;
        }

        UpdateDetection();

        if (aggro.CurrentTarget != null)
            UpdateChaseAndAttack();
        else
            UpdatePatrol();

        ApplyMovement();
        UpdateAnimator();
        UpdateFacing();
    }

    // =====================================================
    // DETECT (chỉ để aggro gần – KHÔNG ghi đè threat)
    // =====================================================
    private void UpdateDetection()
    {
        detectTimer -= Runner.DeltaTime;
        if (detectTimer > 0) return;
        detectTimer = 0.25f;

        if (aggro.CurrentTarget != null) return;

        var hits = Physics2D.OverlapCircleAll(transform.position, detectRange, playerLayer);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist <= aggroRange)
            {
                // aggro nhẹ (MMO-style proximity)
                aggro.AddThreat(
                    hit.GetComponent<NetworkObject>().InputAuthority,
                    1f,
                    hit.transform
                );
                break;
            }
        }
    }

    // =====================================================
    // PATROL (GIỮ NGUYÊN)
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
    // CHASE + ATTACK (DÙNG AGGRO)
    // =====================================================
    private void UpdateChaseAndAttack()
    {
        Transform target = aggro.CurrentTarget;
        if (target == null) return;

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
    // MOVEMENT (GIỮ NGUYÊN)
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
    // ANIMATION (GIỮ NGUYÊN)
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

}
