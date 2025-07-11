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

            NetworkObject arrowObj = Runner.Spawn(ArrowPrefab, spawnPos, spawnRot);
            GameObject arrow = arrowObj.gameObject;

            var sr = arrow.GetComponent<SpriteRenderer>();

            // Lấy sát thương từ player
            var stats = Character.GetComponent<CharacterStats>();
            int damage = stats.strength + stats.finalStrength; // hoặc công thức bạn muốn

            // Truyền damage vào Init:
            var proj = arrow.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Init(direction, speed, damage); // <-- truyền sát thương!
            }

            sr.sprite = Character.Bow.FirstOrDefault(j => j.name.ToLower().Contains("arrow"));

            // Tránh mũi tên va chạm người bắn
            var characterCollider = Character.GetComponent<Collider>();
            if (characterCollider != null)
            {
                Collider arrowCollider = arrow.GetComponent<Collider>();
                Physics.IgnoreCollision(arrowCollider, characterCollider);
            }

            arrow.layer = 31;
            Physics.IgnoreLayerCollision(31, 31, true);
        }

    }
}
