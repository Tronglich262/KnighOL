using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using HeroEditor.Common.Enums;
using UnityEngine;

namespace Assets.HeroEditor.Common.ExampleScripts
{
    public class AttackingExample : NetworkBehaviour
    {
        [Header("Refs")]
        public Character Character;
        public BowExample BowExample;
        public MovementExample Movement; // HeroEditor demo movement

        [Header("Melee Settings")]
        public float AttackRange = 2f;
        public float ChaseSpeed = 4f;

        [Networked] private NetworkObject TargetEnemy { get; set; }
        [Networked] private NetworkBool IsChasing { get; set; }
        [Networked] private NetworkBool IsAttacking { get; set; }

        public override void Spawned()
        {
            if (Character == null) Character = GetComponent<Character>();
            if (BowExample == null) BowExample = GetComponent<BowExample>();
            if (Movement == null) Movement = GetComponent<MovementExample>();
        }

        // =========================
        // CALLED BY SKILL BUTTON
        // =========================
        public void UseSkill(int skillIndex)
        {
            if (!Object.HasInputAuthority) return;
            if (IsAttacking) return;

            var targeting = GetComponent<TargetingSystem>();
            if (targeting == null) return;

            Enemy enemy = targeting.CurrentEnemy;

            // ❌ MMO: chưa target thì KHÔNG ĐÁNH
            if (enemy == null)
            {
                Debug.Log("No target selected.");
                return;
            }

            if (Character.WeaponType == WeaponType.Bow)
            {
                BowExample.AttackTarget(enemy);
            }
            else
            {
                RPC_StartChase(enemy.Object);
            }
        }



        // =========================
        // START CHASE (StateAuthority)
        // =========================
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_StartChase(NetworkObject enemyObj)
        {
            if (IsAttacking) return;

            TargetEnemy = enemyObj;
            IsChasing = true;

            // khóa xoay + quay mặt
            if (Movement != null)
            {
                Movement.LockFacing = true;
                float dirX = enemyObj.transform.position.x - transform.position.x;
                Movement.ForceFaceX(dirX);
            }

            // 🔥 QUAN TRỌNG: set animation RUN
            SetRunState();

            RPC_SetMovementEnabled(false);
        }


        // Disable/enable MovementExample everywhere to stop it overriding state/anim/move
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SetMovementEnabled(bool enabled)
        {
            if (Movement != null) Movement.enabled = enabled;
        }

        // =========================
        // MOVE (StateAuthority only)
        // =========================
        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;
            if (!IsChasing || TargetEnemy == null || IsAttacking) return;

            Vector3 targetPos = TargetEnemy.transform.position;
            Vector3 pos = transform.position;

            float dist = Vector3.Distance(pos, targetPos);

            if (dist > AttackRange)
            {
                Vector3 dir = (targetPos - pos).normalized;
                dir.y = 0; // 🔥 KHÓA TRỤC Y
                dir.Normalize();
                // MOVE
                transform.position = pos + dir * ChaseSpeed * Runner.DeltaTime;

                // 🔥 GIỮ RUN TRONG SUỐT QUÁ TRÌNH CHASE
                SetRunState();
            }
            else
            {
                // tới tầm → dừng chase
                IsChasing = false;
            }
        }


        // =========================
        // REQUEST ATTACK (InputAuthority side)
        // This replicates the "press T" style: input authority asks to attack.
        // =========================
        private void Update()
        {
            if (!Object.HasInputAuthority) return;
            if (IsAttacking) return;
            if (TargetEnemy == null) return;

            // Only for melee
            if (Character != null && Character.WeaponType == WeaponType.Bow) return;

            // If we already stopped chasing on host and we are in range -> request attack once
            float dist = Vector3.Distance(transform.position, TargetEnemy.transform.position);
            if (!IsChasing && dist <= AttackRange)
            {
                RPC_RequestAttack();
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestAttack()
        {
            if (IsAttacking) return;
            if (TargetEnemy == null) return;

            IsAttacking = true;

            // dừng run → idle trước khi đánh
            RPC_SetState(CharacterState.Idle);

            RPC_PlayMeleeAttack();
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayMeleeAttack()
        {
            // EXACTLY like your old "press T" feeling:
            // GetReady then Slash
            if (Character == null) return;

            Character.GetReady();
            Character.Slash();

            // unlock after a short time (tune to your anim length)
            Invoke(nameof(UnlockAfterAttack), 0.7f);
        }

        private void UnlockAfterAttack()
        {
            IsAttacking = false;

            if (Object.HasStateAuthority)
            {
                RPC_SetMovementEnabled(true);
                TargetEnemy = null;

                // về idle
                RPC_SetState(CharacterState.Idle);
            }

            if (Movement != null)
            {
                Movement.LockFacing = false;
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_LockFacingAndFace(NetworkObject enemyObj)
        {
            if (enemyObj == null || Movement == null) return;

            Movement.LockFacing = true;

            float dirX = enemyObj.transform.position.x - transform.position.x;
            if (Mathf.Abs(dirX) < 0.01f) return;
            Movement.ForceFaceX(dirX);
        }

        private void SetRunState()
        {
            if (Character == null) return;

            if (Character.Animator.GetInteger("State") != (int)CharacterState.Run)
            {
                RPC_SetState(CharacterState.Run);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SetState(CharacterState state)
        {
            Character.Animator.SetInteger("State", (int)state);
        }

    }
}
