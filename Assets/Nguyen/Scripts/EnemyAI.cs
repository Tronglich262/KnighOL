using UnityEngine;
using Fusion;

public class EnemyAI : NetworkBehaviour
{
    [Header("AI Settings")]
    public float patrolDistance = 5f;
    public float moveSpeed = 2f;
    public float detectRange = 4f;
    public LayerMask playerLayer;
    public float attackCooldown = 1f;

    private Vector3 _startPos;
    private bool _movingRight = true;
    private Rigidbody rb;
    private Animator animator;

    [Networked] private TickTimer attackCooldownTimer { get; set; }

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        _startPos = transform.position;
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            Transform player = DetectPlayer();

            if (player != null)
            {
                Attack(player);
            }
            else
            {
                Patrol();
            }
        }
    }

    private void Patrol()
    {
        Vector3 target = _startPos + Vector3.right * (_movingRight ? patrolDistance : -patrolDistance);
        float dir = Mathf.Sign(target.x - transform.position.x);

        Vector3 velocity = new Vector3(dir * moveSpeed, rb.linearVelocity.y, 0);
        rb.linearVelocity = velocity;

        bool isMovingNow = Mathf.Abs(velocity.x) > 0.1f;
        RPC_SetMoveState(isMovingNow);
        RPC_Flip(dir > 0);

        if (Mathf.Abs(transform.position.x - target.x) < 0.1f)
        {
            _movingRight = !_movingRight;
        }
    }

    private void Attack(Transform player)
    {
        if (!attackCooldownTimer.ExpiredOrNotRunning(Runner)) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > 0.5f)
        {
            Vector3 velocity = new Vector3(dirToPlayer.x * moveSpeed, rb.linearVelocity.y, 0);
            rb.linearVelocity = velocity;

            RPC_SetMoveState(true);
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            RPC_SetMoveState(false);
        }

        RPC_Flip(dirToPlayer.x > 0);
        attackCooldownTimer = TickTimer.CreateFromSeconds(Runner, attackCooldown);
        RPC_PlayAttackAnimation();
    }

    private Transform DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange, playerLayer);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                return hit.transform;
            }
        }
        return null;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("AttackTrigger");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetMoveState(bool moving)
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", moving);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Flip(bool faceRight)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? -1 : 1);
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
