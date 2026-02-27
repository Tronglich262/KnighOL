using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using HeroEditor.Common.Enums;
using UnityEngine;

namespace Assets.HeroEditor.Common.ExampleScripts
{
    public class AttackingExample : NetworkBehaviour
    {
        public Character Character;
        public MovementExample Movement;
        public BowExample Bow;

        public float AttackRange = 2f;

        [Networked] private NetworkObject TargetEnemy { get; set; }
        [Networked] private NetworkBool IsChasing { get; set; }
        [Networked] private NetworkBool IsAttacking { get; set; }

        public override void Spawned()
        {
            if (Character == null) Character = GetComponent<Character>();
            if (Movement == null) Movement = GetComponent<MovementExample>();
            if (Bow == null) Bow = GetComponent<BowExample>();
        }

        // =========================
        // USE SKILL (CALLED FROM BUFF SYSTEM)
        // =========================
        public void UseSkill(NetworkObject target)
        {
            if (!Object.HasInputAuthority) return;
            if (IsAttacking) return;
            if (target == null) return;

            // 🏹 Nếu là cung → bắn luôn
            if (Character.WeaponType == WeaponType.Bow)
            {
                RPC_BowAttack(target);
            }
            else
            {
                // 🗡 Melee → chase
                RPC_StartChase(target);
            }
        }

        // =========================
        // BOW
        // =========================
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_BowAttack(NetworkObject target)
        {
            if (IsAttacking) return;

            IsAttacking = true;
            TargetEnemy = target;

            RPC_PlayBow(target);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayBow(NetworkObject target)
        {
            if (Bow != null)
            {
                Bow.AttackTarget(target.GetComponent<Enemy>());
            }

            Invoke(nameof(EndAttack), 0.5f);
        }

        // =========================
        // MELEE CHASE
        // =========================
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_StartChase(NetworkObject enemyObj)
        {
            if (enemyObj == null) return;

            TargetEnemy = enemyObj;
            IsChasing = true;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;
            if (TargetEnemy == null) return;
            if (IsAttacking) return;

            if (!IsChasing) return;

            Vector3 pos = transform.position;
            Vector3 targetPos = TargetEnemy.transform.position;

            float dist = Vector3.Distance(pos, targetPos);

            if (dist > AttackRange)
            {
                Vector3 dir = (targetPos - pos).normalized;
                dir.y = 0;
                Movement.AutoMove(dir);
            }
            else
            {
                IsChasing = false;
                Movement.ForceStop();
                StartMeleeAttack();
            }
        }

        void StartMeleeAttack()
        {
            if (IsAttacking) return;

            IsAttacking = true;

            RPC_PlayMelee();

            // Delay đúng frame chém
            Invoke(nameof(ApplyMeleeDamage), 0.3f);
        }
        void ApplyMeleeDamage()
        {
            if (!Object.HasStateAuthority) return;
            if (TargetEnemy == null) return;

            var enemy = TargetEnemy.GetComponent<EnemyCore>();
            if (enemy == null) return;

            var stats = GetComponent<CharacterStats>();

            int statPart = stats.strength + stats.finalStrength;
            int baseDamage = UnityEngine.Random.Range(80, 110);

            int damage = statPart + Mathf.RoundToInt(baseDamage * 1.2f);

            enemy.RPC_RequestHit(damage, Object.InputAuthority);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        void RPC_PlayMelee()
        {
            Character.GetReady();
            Character.Slash();

            Invoke(nameof(EndAttack), 0.6f);
        }

        // =========================
        // CANCEL WHEN MOVE
        // =========================
        private void Update()
        {
            if (!Object.HasInputAuthority) return;

            if (IsChasing || IsAttacking)
            {
                if (Input.GetKey(KeyCode.LeftArrow) ||
                    Input.GetKey(KeyCode.RightArrow) ||
                    Input.GetKey(KeyCode.UpArrow))
                {
                    RPC_Cancel();
                }
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_Cancel()
        {
            CancelAll();
        }

        void CancelAll()
        {
            IsChasing = false;
            IsAttacking = false;
            TargetEnemy = null;

            Movement.ForceStop();
        }

        void EndAttack()
        {
            if (!Object.HasStateAuthority) return;

            IsAttacking = false;
            TargetEnemy = null;
        }
    }
}