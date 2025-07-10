using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Assets.HeroEditor.Common.ExampleScripts
{
    /// <summary>
    /// General behaviour for projectiles: bullets, rockets and other.
    /// </summary>
    public class Projectile : NetworkBehaviour
    {
        public List<Renderer> Renderers;
        public GameObject Trail;
        public GameObject Impact;
        public Rigidbody Rigidbody;

        private bool _hasBanged;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                Invoke(nameof(DespawnSelf), 5f);
            }
        }

        public void Update()
        {
            if (Rigidbody != null && Rigidbody.useGravity)
            {
                transform.right = Rigidbody.linearVelocity.normalized;
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!_hasBanged && HasStateAuthority)
            {
                _hasBanged = true;
                RPC_Bang();
            }
        }

        public void OnCollisionEnter(Collision other)
        {
            if (!_hasBanged && HasStateAuthority)
            {
                _hasBanged = true;
                RPC_Bang();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_Bang()
        {
            Bang(null);
        }

        private void Bang(GameObject other)
        {
            ReplaceImpactSound(other);
            Impact.SetActive(true);
            Destroy(GetComponent<SpriteRenderer>());
            Destroy(GetComponent<Rigidbody>());
            Destroy(GetComponent<Collider>());
            Destroy(gameObject, 1);

            foreach (var ps in Trail.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Stop();
            }

            foreach (var tr in Trail.GetComponentsInChildren<TrailRenderer>())
            {
                tr.enabled = false;
            }
        }

        private void ReplaceImpactSound(GameObject other)
        {
            if (other == null) return;

            var sound = other.GetComponent<AudioSource>();

            if (sound != null && sound.clip != null)
            {
                Impact.GetComponent<AudioSource>().clip = sound.clip;
            }
        }

        private void DespawnSelf()
        {
            if (Object != null && Object.IsValid)
            {
                Runner.Despawn(Object);
            }
        }

        // ✅ Add this
        public void Init(Vector3 direction, float speed)
        {
            if (Rigidbody != null)
            {
                Rigidbody.linearVelocity = direction.normalized * speed;
            }
        }
    }
}
