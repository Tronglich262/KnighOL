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

            Enemy enemy = targeting.GetNearestEnemy(transform.position);
            if (enemy == null) return;

            if (Character.WeaponType == WeaponType.Bow)
            {
                BowExample.AttackTarget(enemy);
                return;
            }

            // melee -> request chase on state authority
            RPC_StartChase(enemy.Object);
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

            // IMPORTANT: disable MovementExample on all clients while auto-chasing
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
                transform.position = pos + dir * ChaseSpeed * Runner.DeltaTime;
            }
            else
            {
                // reached range -> stop moving, wait for InputAuthority to request attack
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

            // face target before attacking
            Vector3 dir = TargetEnemy.transform.position - transform.position;
            if (dir.x != 0)
            {
                var s = transform.localScale;
                transform.localScale = new Vector3(Mathf.Sign(dir.x), s.y, s.z);
            }

            // broadcast attack animation (HeroEditor flow you already know works)
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

            // re-enable MovementExample after attack so player can move normally again
            // (only if you want that)
            if (Object.HasStateAuthority)
            {
                RPC_SetMovementEnabled(true);
                TargetEnemy = null;
            }
        }
    }
}
