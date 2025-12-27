using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using UnityEngine;

namespace Assets.HeroEditor.Common.ExampleScripts
{
    public class BowExample : NetworkBehaviour
    {
        public Character Character;
        public AnimationClip ClipCharge;
        public Transform FireTransform;
        public GameObject ArrowPrefab;

        public bool CreateArrows = true;
        public bool ChargeButtonDown;
        public bool ChargeButtonUp;

        private float _chargeTime;
        private float _localAimAngle;

        [Networked] public float AimAngle { get; private set; }
        public override void Spawned()
        {
            if (Character == null) Character = GetComponent<Character>();
            if (FireTransform == null) FireTransform = transform.Find("FireTransform"); // sửa path cho đúng
        }

        private void Update()
        {
            if (!Object.HasInputAuthority) return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            Vector2 dir = mouseWorld - FireTransform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (ChargeButtonDown)
            {
                _chargeTime = Time.time;
                RPC_StartCharge();
                ChargeButtonDown = false;
            }

            if (ChargeButtonUp)
            {
                bool charged = Time.time - _chargeTime > 0.3f;
                RPC_Release(charged, FireTransform.position, AimAngle);
                ChargeButtonUp = false;
            }

            RPC_SetAim(angle);
        }
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetAim(float angle)
        {
            AimAngle = angle;
        }
        public override void FixedUpdateNetwork()
        {
            FireTransform.rotation = Quaternion.Euler(0, 0, AimAngle);
        }
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_Release(bool charged, Vector3 pos, float angle)
        {
            AimAngle = angle;

            if (charged)
                CreateArrow(pos, angle);

            RPC_PlayReleaseAnim(charged);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_StartCharge()
        {
            Character.Animator.SetInteger("Charge", 1);
        }



        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayReleaseAnim(bool charged)
        {
            Character.Animator.SetInteger("Charge", charged ? 2 : 3);
        }

        private void CreateArrow(Vector3 spawnPos, float angle)
        {
            Quaternion spawnRot = Quaternion.Euler(0, 0, angle);

            NetworkObject arrowObj = Runner.Spawn(ArrowPrefab, spawnPos, spawnRot);

            Rigidbody rb = arrowObj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = spawnRot * Vector3.right * 18f; // Rigidbody dùng velocity (3D)
        }
    }
}
