using System.Linq;
using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using UnityEngine;

namespace Assets.HeroEditor.Common.ExampleScripts
{
    /// <summary>
    /// Bow shooting behaviour (charge/release bow, create arrow). Now networked with Fusion.
    /// </summary>
    public class BowExample : NetworkBehaviour
    {
        public Character Character;
        public AnimationClip ClipCharge;
        public Transform FireTransform;
        public GameObject ArrowPrefab;
        public bool CreateArrows;

        [HideInInspector] public bool ChargeButtonDown;
        [HideInInspector] public bool ChargeButtonUp;

        private float _chargeTime;

        public void Update()
        {
            if (!HasInputAuthority) return;

            if (ChargeButtonDown)
            {
                _chargeTime = Time.time;
                RPC_StartCharge();
                ChargeButtonDown = false;
            }

            if (ChargeButtonUp)
            {
                bool charged = Time.time - _chargeTime > ClipCharge.length;
                RPC_ReleaseCharge(charged);
                ChargeButtonUp = false;
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_StartCharge()
        {
            Character.Animator.SetInteger("Charge", 1);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        private void RPC_ReleaseCharge(bool charged)
        {
            Character.Animator.SetInteger("Charge", charged ? 2 : 3);

            if (charged && CreateArrows && Object.HasStateAuthority)
            {
                CreateArrow();
            }
        }

        private void CreateArrow()
        {
            Vector3 spawnPos = FireTransform.position;
            Quaternion spawnRot = Quaternion.identity;

            float speed = 18.75f * Random.Range(0.85f, 1.15f);
            Vector3 direction = FireTransform.right * Mathf.Sign(Character.transform.lossyScale.x);

            // Spawn arrow qua mạng
            NetworkObject arrowObj = Runner.Spawn(ArrowPrefab, spawnPos, spawnRot);
            GameObject arrow = arrowObj.gameObject;

            var sr = arrow.GetComponent<SpriteRenderer>();
            var rb = arrow.GetComponent<Rigidbody>();

            // Gán sprite cho mũi tên từ dữ liệu cung
            sr.sprite = Character.Bow.FirstOrDefault(j => j.name.ToLower().Contains("arrow"));

            // Gán vận tốc theo hướng bắn (client khác sync qua NetworkTransform)
            rb.linearVelocity = direction * speed;

            // Tránh mũi tên va chạm người bắn
            var characterCollider = Character.GetComponent<Collider>();
            if (characterCollider != null)
            {
                Collider arrowCollider = arrow.GetComponent<Collider>();
                Physics.IgnoreCollision(arrowCollider, characterCollider);
            }

            // Layer riêng cho projectile
            arrow.layer = 31;
            Physics.IgnoreLayerCollision(31, 31, true);
        }
    }
}
