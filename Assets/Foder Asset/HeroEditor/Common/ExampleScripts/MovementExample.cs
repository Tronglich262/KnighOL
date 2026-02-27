using Assets.HeroEditor.Common.CharacterScripts;
using Fusion;
using Unity.Jobs;
using UnityEngine;

namespace Assets.HeroEditor.Common.ExampleScripts
{
    public class MovementExample : NetworkBehaviour
    {
        public Character Character;
        public CharacterController Controller;

        private Vector3 _velocity = Vector3.zero;

        [Networked] private float NetworkedScaleX { get; set; }
        [Networked] private Vector2 NetworkedDirection { get; set; }
        [Networked] private CharacterState NetworkedState { get; set; }
        [Networked] public NetworkBool LockFacing { get; set; }
        public float CurrentScaleX => NetworkedScaleX;
        PlayerMapState mapState;
        //
        [Header("Movement Settings")]
        public float baseMoveSpeed = 6f;   // đổi số này để tăng tốc
        public float jumpForce = 12f;

        private CharacterStats stats;

        // dùng cho buff tốc độ sau này
        [Networked] private float SpeedMultiplier { get; set; }
        //
        public static MovementExample Instante;
        public bool checktoggle = false;
        public void Awake()
        {
            Instante = this;
            mapState = GetComponent<PlayerMapState>();

        }
        public override void Spawned()
        {
            stats = GetComponent<CharacterStats>();

            if (HasStateAuthority)
            {
                SpeedMultiplier = 1f; // mặc định không buff
            }
            if (Controller == null)
            {
                Controller = Character.gameObject.AddComponent<CharacterController>();
                Controller.center = new Vector3(0, 1.125f);
                Controller.height = 3.4f;
                Controller.radius = 0.75f;
                Controller.minMoveDistance = 0;
            }

            Character.Animator.SetBool("Ready", true);

            if (Object.HasStateAuthority)
            {
                NetworkedScaleX = 1; // Mặc định hướng phải
            }

            // Debug.Log($"[Spawned] Client {Runner.LocalPlayer}, Player {Object.Id}: Initial localScale = {Character.transform.localScale}, Initial State = {(int)NetworkedState}");
        }

        private void Update()
        {
            if (!HasInputAuthority) return;

            // 🚫 FREEZE → KHÔNG NHẬN INPUT
            if (mapState != null && mapState.FreezeMovement)
                return;

            Vector2 direction = Vector2.zero;

            if (!checktoggle)
            {
                if (Input.GetKey(KeyCode.LeftArrow)) direction.x = -1;
                if (Input.GetKey(KeyCode.RightArrow)) direction.x = 1;
                if (Input.GetKey(KeyCode.UpArrow)) direction.y = 1;
            }

            NetworkedDirection = direction;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_UpdateScaleX(float scaleX)
        {
            Character.transform.localScale = new Vector3(scaleX, 1, 1);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_UpdateState(CharacterState state)
        {
            NetworkedState = state;
            Character.Animator.SetInteger("State", (int)state);
        }

        public override void FixedUpdateNetwork()
        {
            if (Controller == null || !Controller.enabled)
                return;

            // 🔒 FREEZE → ĐỨNG IM TUYỆT ĐỐI
            if (mapState != null && mapState.FreezeMovement)
            {
                _velocity = Vector3.zero;
                NetworkedDirection = Vector2.zero;
                Controller.Move(Vector3.zero);
                return;
            }

            Vector2 direction = NetworkedDirection;

            if (LockFacing && direction == Vector2.zero)
            {
                Controller.Move(Vector3.zero);
                return;
            }

            if (HasStateAuthority && !LockFacing && direction.x != 0)
            {
                float newScaleX = Mathf.Sign(direction.x);
                if (NetworkedScaleX != newScaleX)
                {
                    NetworkedScaleX = newScaleX;
                    Rpc_UpdateScaleX(NetworkedScaleX);
                }
            }

            Character.transform.localScale = new Vector3(NetworkedScaleX, 1, 1);

            if (Controller.isGrounded)
            {
                _velocity = new Vector3(
                    GetFinalMoveSpeed() * direction.x,
                    jumpForce * direction.y
                );
                if (HasInputAuthority)
                {
                    if (direction != Vector2.zero)
                        SetState(CharacterState.Run);
                    else if (NetworkedState < CharacterState.DeathB)
                        SetState(CharacterState.Idle);
                }
            }
            else
            {
                if (HasInputAuthority)
                    SetState(CharacterState.Jump);
            }

            _velocity.y -= 25 * Runner.DeltaTime;
            Controller.Move(_velocity * Runner.DeltaTime);
        }


        private void SetState(CharacterState newState)
        {
            if (NetworkedState != newState)
            {
                NetworkedState = newState;
                if (HasStateAuthority)
                {
                    Rpc_UpdateState(newState); // Gửi RPC để đồng bộ trạng thái
                }
            }
        }
        public void ForceFaceX(float signX)
        {
            if (!HasStateAuthority) return;

            signX = Mathf.Sign(signX);
            if (signX == 0) return;

            if (NetworkedScaleX != signX)
            {
                NetworkedScaleX = signX;
                Rpc_UpdateScaleX(NetworkedScaleX);
            }
        }
        public void AutoMove(Vector3 worldDir)
        {
            if (!HasStateAuthority) return;

            Vector2 dir = new Vector2(Mathf.Sign(worldDir.x), 0);

            NetworkedDirection = dir;

            // ép state Run
            if (NetworkedState != CharacterState.Run)
            {
                NetworkedState = CharacterState.Run;
                Rpc_UpdateState(CharacterState.Run);
            }
        }
        public void ForceStop()
        {
            if (!HasStateAuthority) return;

            NetworkedDirection = Vector2.zero;
            _velocity = Vector3.zero;

            NetworkedState = CharacterState.Idle;
            Rpc_UpdateState(CharacterState.Idle);
        }

        float GetFinalMoveSpeed()
        {
            float speed = baseMoveSpeed;

            // 1️⃣ Agility cộng thẳng (tăng chậm, ổn định)
            speed += stats.finalAgility * 0.1f;

            // 2️⃣ Speed stat nhân %
            speed *= 1f + (stats.speed * 0.01f);

            // 3️⃣ Buff / debuff nhân sau cùng
            speed *= SpeedMultiplier;

            return Mathf.Clamp(speed, 3f, 15f);
        }
    }
}