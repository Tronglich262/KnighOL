using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using UnityEngine;
using HeroEditor.Common.Enums;

namespace Assets.HeroEditor.Common.ExampleScripts
{
    public class BowExample : NetworkBehaviour
    {
        [Header("Refs")]
        public Character Character;
        public Transform FireTransform;

        [Header("Arrow")]
        public NetworkPrefabRef ArrowPrefab;
        public float ArrowSpeed = 18f;

        [Header("Aim")]
        [Networked] public float AimAngle { get; private set; }

        public float MinAngle = -40f;
        public float MaxAngle = 40f;

        public override void Spawned()
        {
            if (Character == null) Character = GetComponent<Character>();
            if (FireTransform == null) FireTransform = transform.Find("FireTransform");
        }

        // =========================
        // CALLED BY SKILL BUTTON
        // =========================
        public void AttackTarget(Enemy enemy)
        {
            if (!Object.HasInputAuthority) return;
            if (enemy == null) return;

            Vector3 dir = enemy.transform.position - FireTransform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            angle = Mathf.Clamp(angle, MinAngle, MaxAngle);

            RPC_StartBowAttack(angle);
        }

        // =========================
        // INPUT → STATE AUTHORITY
        // =========================
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_StartBowAttack(float angle)
        {
            AimAngle = angle;

            // Kéo cung
            RPC_SetCharge(1);

            // Nhả sau delay giống HeroEditor
            Invoke(nameof(ReleaseBow), 0.35f);
        }

        private void ReleaseBow()
        {
            RPC_SetCharge(2);
            SpawnArrow(); // 🔥 SPAWN Ở ĐÂY
        }

        // =========================
        // SPAWN ARROW (SERVER)
        // =========================
        private void SpawnArrow()
        {
            if (!Object.HasStateAuthority) return;

            // ===== hướng chính xác =====
            Vector2 dir = Quaternion.Euler(0, 0, AimAngle) * Vector2.right;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // ===== spawn + xoay =====
            NetworkObject arrowObj = Runner.Spawn(
                ArrowPrefab,
                FireTransform.position,
                Quaternion.Euler(0, 0, angle)
            );

            GameObject arrow = arrowObj.gameObject;

            // ===== velocity =====
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = arrow.transform.right * ArrowSpeed;
            }

            // ===== sprite =====
            SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
            if (sr != null && Character.Bow != null)
            {
                sr.sprite = Character.Bow.Find(s => s.name.ToLower().Contains("arrow"));
            }

            // ===== damage owner =====
            ArrowDamage dmg = arrow.GetComponent<ArrowDamage>();
            if (dmg != null)
            {
                dmg.Init(Object.InputAuthority);
            }

            // ===== ignore collision =====
            Collider arrowCol = arrow.GetComponent<Collider>();
            Collider charCol = Character.GetComponent<Collider>();
            if (arrowCol && charCol)
            {
                Physics.IgnoreCollision(arrowCol, charCol);
            }
        }



        // =========================
        // SYNC ANIM
        // =========================
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SetCharge(int value)
        {
            if (Character == null) return;
            Character.Animator.SetInteger("Charge", value);
        }

        // =========================
        // APPLY AIM (HEROEDITOR)
        // =========================
        private void LateUpdate()
        {
            if (Character == null) return;
            if (Character.WeaponType != WeaponType.Bow) return;

            var armL = Character.BodyRenderers.Find(r => r.name == "ArmL")?.transform;
            var bow = Character.BowRenderers != null && Character.BowRenderers.Count > 3
                ? Character.BowRenderers[3].transform
                : null;

            if (armL == null || bow == null) return;

            Vector2 dir = Quaternion.Euler(0, 0, AimAngle) * Vector2.right;
            Vector2 target = (Vector2)armL.position + dir * 1000f;

            RotateArm(armL, bow, target, MinAngle, MaxAngle);
        }

        private void RotateArm(Transform arm, Transform weapon, Vector2 target, float angleMin, float angleMax)
        {
            target = arm.transform.InverseTransformPoint(target);

            float angleToTarget = Vector2.SignedAngle(Vector2.right, target);
            float angleToArm = Vector2.SignedAngle(weapon.right, arm.transform.right) * Mathf.Sign(weapon.lossyScale.x);

            float fix = weapon.InverseTransformPoint(arm.position).y / target.magnitude;
            fix = Mathf.Clamp(fix, -1, 1);

            float angleFix = Mathf.Asin(fix) * Mathf.Rad2Deg;
            float angle = angleToTarget + angleFix + arm.localEulerAngles.z;

            angle = NormalizeAngle(angle);
            angle = Mathf.Clamp(angle, angleMin, angleMax);

            arm.localEulerAngles = new Vector3(0, 0, angle + angleToArm);
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }
    }
}
