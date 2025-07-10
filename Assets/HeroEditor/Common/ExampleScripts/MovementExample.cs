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
        public static MovementExample Instante;
        public bool checktoggle = false;
        public void Awake()
        {
            Instante = this;
        }
        public override void Spawned()
        {
            if (Controller == null)
            {
                Controller = Character.gameObject.AddComponent<CharacterController>();
                Controller.center = new Vector3(0, 1.125f);
                Controller.height = 2.5f;
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

            Vector2 direction = Vector2.zero;
            if (checktoggle == true)
            {
                return;
            }
            else 
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
           // Debug.Log($"[RPC] Client {Runner.LocalPlayer}, Player {Object.Id}: Set localScale.x = {scaleX}, current localScale = {Character.transform.localScale}");
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_UpdateState(CharacterState state)
        {
            NetworkedState = state;
            Character.Animator.SetInteger("State", (int)state);
           // Debug.Log($"[RPC] Client {Runner.LocalPlayer}, Player {Object.Id}: Set NetworkedState to {(int)state}, Animator State = {(int)Character.Animator.GetInteger("State")}");
        }

        public override void FixedUpdateNetwork()
        {
            Vector2 direction = NetworkedDirection;

            if (HasStateAuthority && direction.x != 0)
            {
                float newScaleX = Mathf.Sign(direction.x); // -1 cho trái, 1 cho phải
                if (NetworkedScaleX != newScaleX)
                {
                    NetworkedScaleX = newScaleX;
                    Rpc_UpdateScaleX(NetworkedScaleX); // Gửi RPC để đồng bộ scale
                  //  Debug.Log($"[StateAuthority] Client {Runner.LocalPlayer}, Player {Object.Id}: Set NetworkedScaleX to {NetworkedScaleX}");
                }
            }

            // Áp dụng localScale trên tất cả client
            Character.transform.localScale = new Vector3(NetworkedScaleX, 1, 1);
           // Debug.Log($"[FixedUpdate] Client {Runner.LocalPlayer}, Player {Object.Id}: NetworkedScaleX = {NetworkedScaleX}, localScale = {Character.transform.localScale}");

            if (Controller.isGrounded)
            {
                _velocity = new Vector3(5 * direction.x, 12 * direction.y);

                if (HasInputAuthority)
                {
                    if (direction != Vector2.zero)
                    {
                        SetState(CharacterState.Run);
                       // Debug.Log($"[InputAuthority] Client {Runner.LocalPlayer}, Player {Object.Id}: Set State to Run, NetworkedState = {(int)NetworkedState}");
                    }
                    else if (NetworkedState < CharacterState.DeathB)
                    {
                        SetState(CharacterState.Idle);
                      //  Debug.Log($"[InputAuthority] Client {Runner.LocalPlayer}, Player {Object.Id}: Set State to Idle, NetworkedState = {(int)NetworkedState}");
                    }
                }
            }
            else
            {
                if (HasInputAuthority)
                {
                    SetState(CharacterState.Jump);
                    //Debug.Log($"[InputAuthority] Client {Runner.LocalPlayer}, Player {Object.Id}: Set State to Jump, NetworkedState = {(int)NetworkedState}");
                }
            }

            _velocity.y -= 25 * Runner.DeltaTime;
            Controller.Move(_velocity * Runner.DeltaTime);

            Character.Animator.SetInteger("State", (int)NetworkedState);
          //  Debug.Log($"[FixedUpdate] Client {Runner.LocalPlayer}, Player {Object.Id}: Set Animator State to {(int)NetworkedState}");
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
              //  Debug.Log($"[SetState] Client {Runner.LocalPlayer}, Player {Object.Id}: NetworkedState changed to {(int)newState}");
            }
        }
    }
}