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

        [Header("Aim")]
        [Networked] public float AimAngle { get; private set; }

        // (Các giới hạn giống pack HeroEditor)
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

            Vector3 dir = enemy.transform.position - FireTransform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // clamp để khớp rig HeroEditor
            angle = Mathf.Clamp(angle, MinAngle, MaxAngle);

            RPC_StartBowAttack(angle);
        }

        // =========================
        // STATE AUTHORITY DRIVES AIM + ANIM
        // =========================
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_StartBowAttack(float angle)
        {
            AimAngle = angle;

            // 1) Kéo cung
            RPC_SetCharge(1);

            // 2) Nhả sau delay (giống phím T)
            Invoke(nameof(ReleaseBow), 0.35f);
        }

        private void ReleaseBow()
        {
            RPC_SetCharge(2);
        }

        // =========================
        // SYNC ANIM TO ALL
        // =========================
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SetCharge(int value)
        {
            if (Character == null) return;
            Character.Animator.SetInteger("Charge", value);
        }

        // =========================
        // APPLY AIM TO ARM + BOW (QUAN TRỌNG)
        // =========================
        private void LateUpdate()
        {
            if (Character == null) return;
            if (Character.WeaponType != WeaponType.Bow) return;

            // HeroEditor xoay tay/cung theo AimAngle
            // BowRenderers[3] thường là cung; ArmL là tay trái
            var armL = Character.BodyRenderers.Find(r => r.name == "ArmL")?.transform;
            var bow = Character.BowRenderers != null && Character.BowRenderers.Count > 3
                        ? Character.BowRenderers[3].transform
                        : null;

            if (armL == null || bow == null) return;

            // Tính target ảo từ AimAngle để dùng hàm xoay chuẩn
            Vector2 dir = Quaternion.Euler(0, 0, AimAngle) * Vector2.right;
            Vector2 target = (Vector2)armL.position + dir * 1000f;

            RotateArm(armL, bow, target, MinAngle, MaxAngle);
        }

        // ======= HÀM XOAY GIỐNG HEROEDITOR GỐC =======
        private void RotateArm(Transform arm, Transform weapon, Vector2 target, float angleMin, float angleMax)
        {
            target = arm.transform.InverseTransformPoint(target);

            var angleToTarget = Vector2.SignedAngle(Vector2.right, target);
            var angleToArm = Vector2.SignedAngle(weapon.right, arm.transform.right) * Mathf.Sign(weapon.lossyScale.x);
            var fix = weapon.InverseTransformPoint(arm.transform.position).y / target.magnitude;

            fix = Mathf.Clamp(fix, -1, 1);
            var angleFix = Mathf.Asin(fix) * Mathf.Rad2Deg;
            var angle = angleToTarget + angleFix + arm.transform.localEulerAngles.z;

            angle = NormalizeAngle(angle);
            angle = Mathf.Clamp(angle, angleMin, angleMax);

            arm.transform.localEulerAngles = new Vector3(0, 0, angle + angleToArm);
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }
    }
}
