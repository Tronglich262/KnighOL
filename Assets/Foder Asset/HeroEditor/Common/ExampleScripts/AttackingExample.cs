using Assets.HeroEditor.Common.CharacterScripts;
using Assets.HeroEditor.Common.CharacterScripts.Firearms;
using Assets.HeroEditor.Common.CharacterScripts.Firearms.Enums;
using Fusion;
using HeroEditor.Common.Enums;
using System;
using UnityEngine;

namespace Assets.HeroEditor.Common.ExampleScripts
{
    public class AttackingExample : NetworkBehaviour
    {
        public Character Character;
        public BowExample BowExample;
        public Firearm Firearm;
        public Transform ArmL;
        public Transform ArmR;
        public KeyCode FireButton;
        public KeyCode ReloadButton;
        [Header("Check to disable arm auto rotation.")]
        public bool FixedArm;

        public override void Spawned()
        {
            Character = GetComponent<Character>();
            BowExample = GetComponent<BowExample>(); //  LẤY TRÊN CHÍNH PLAYER
        }


        public void Update()
        {
            if (!Object.HasInputAuthority)
                return;


            if (Character.Animator.GetInteger("State") >= (int)CharacterState.DeathB) return;

            switch (Character.WeaponType)
            {
                case WeaponType.Melee1H:
                case WeaponType.Melee2H:
                case WeaponType.MeleePaired:
                    if (Input.GetKeyDown(FireButton))
                    {
                        RPC_Slash();
                    }
                    break;

                case WeaponType.Bow:
                    BowExample.ChargeButtonDown = Input.GetKeyDown(FireButton);
                    BowExample.ChargeButtonUp = Input.GetKeyUp(FireButton);
                    break;

                case WeaponType.Firearms1H:
                case WeaponType.Firearms2H:
                    Firearm.Fire.FireButtonDown = Input.GetKeyDown(FireButton);
                    Firearm.Fire.FireButtonPressed = Input.GetKey(FireButton);
                    Firearm.Fire.FireButtonUp = Input.GetKeyUp(FireButton);
                    Firearm.Reload.ReloadButtonDown = Input.GetKeyDown(ReloadButton);
                    break;

                case WeaponType.Supplies:
                    if (Input.GetKeyDown(FireButton))
                    {
                        RPC_PlaySupplyAnim();
                    }
                    break;
            }

            if (Input.GetKeyDown(FireButton))
            {
                Character.GetReady();
            }
        }

        private void LateUpdate()
        {
            if (Character == null) return;
            if (Character.WeaponType != WeaponType.Bow) return;
            Debug.Log("LateUpdate rotate: " + BowExample.AimAngle);

            // ❗ KHÔNG check InputAuthority
            // ❗ MỌI CLIENT ĐỀU RENDER THEO AimAngle

            Transform arm = ArmL;
            Transform weapon = Character.BowRenderers[3].transform;

            Vector2 target = GetTargetFromAimAngle(BowExample.AimAngle);

            RotateArm(arm, weapon, target, -40f, 40f);
        }




        private void ApplyBowAim(float angle)
        {
            angle = Mathf.Clamp(angle, -40f, 40f);

        }
        private Vector2 GetTargetFromAimAngle(float angle)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
            return (Vector2)ArmL.position + dir * 1000f;
        }





        public float AngleToTarget;
        public float AngleToArm;

        public void RotateArm(Transform arm, Transform weapon, Vector2 target, float angleMin, float angleMax)
        {
            target = arm.transform.InverseTransformPoint(target);

            var angleToTarget = Vector2.SignedAngle(Vector2.right, target);
            var angleToArm = Vector2.SignedAngle(weapon.right, arm.transform.right) * Math.Sign(weapon.lossyScale.x);
            var fix = weapon.InverseTransformPoint(arm.transform.position).y / target.magnitude;

            AngleToTarget = angleToTarget;
            AngleToArm = angleToArm;

            fix = Mathf.Clamp(fix, -1, 1);
            var angleFix = Mathf.Asin(fix) * Mathf.Rad2Deg;
            var angle = angleToTarget + angleFix + arm.transform.localEulerAngles.z;
            angle = NormalizeAngle(angle);
            angle = Mathf.Clamp(angle, angleMin, angleMax);

            if (float.IsNaN(angle)) Debug.LogWarning(angle);

            arm.transform.localEulerAngles = new Vector3(0, 0, angle + angleToArm);
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }

        // --- Networked attack methods (Fusion) ---

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_Slash()
        {
            Character.Slash(); // All clients will see the slash animation
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_PlaySupplyAnim()
        {
            string anim = Time.frameCount % 2 == 0 ? "UseSupply" : "ThrowSupply";
            Character.Animator.Play(anim, 0);
        }
    }
} 