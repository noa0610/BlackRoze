using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using AIStates = BlackRose.Core.Models.Units.AIController.AIStates;
using Triggers = BlackRose.Core.Models.Units.AIController.AITriggers;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public class NormalMode : AIModeBase
    {
        public enum NormalState
        {
            N_Shoot,
            N_Jump,
            N_Skill,
        }

        [SerializeField] private float _warpRange;
        [SerializeField] private GameObject _preWarpDemo;
        [SerializeField] private Vector3 _warpDemoDelta = new(0, 1, 0);
        private Warp _warpState = new Warp();

        public Vector2 WarpPreDir { get; private set; }

        public override void Register()
        {
            // Idle
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                AIStates.Idle,
                    (Triggers.shootInput, NormalState.N_Shoot, ""),
                    (Triggers.jumpInput, NormalState.N_Jump, ""),
                    (Triggers.skillInput, NormalState.N_Skill, "")
                );

            // Jump
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                NormalState.N_Jump,
                    (Triggers.falling, AIStates.Fall, ""),
                    (Triggers.landing, AIStates.Idle, "")
                );
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                NormalState.N_Jump,
                    (Triggers.shootInput, NormalState.N_Shoot, "")
                );

            // Skill
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                NormalState.N_Skill,
                    (Triggers.skillFinished, AIStates.Idle, "")
                );


            SM.AddState(NormalState.N_Jump, _jump);
            SM.AddState(NormalState.N_Skill, _warpState);
            _warpState.warped += () =>
            {
                Debug.Log("==================");
                SM.LazyChange(Triggers.skillFinished);
            };
        }

        public override void OnSkill(InputValue value)
        {
            Debug.Log("InputValue press:" + value.isPressed.ToString());
            if (value.isPressed)
            {
                _preWarpDemo.SetActive(true);
                SM.ChangeState(Triggers.skillInput);
            }
            else
            {
                // TODO : ワープ実行
            }
        }
        public override void OnInputMove(Vector2 dir)
        {
            if (dir != Vector2.zero)
                WarpPreDir = dir;
        }

        public override void FixedUpdate(float deltaTime)
        {
            _preWarpDemo.transform.position = _parent.transform.position + _warpDemoDelta + (Vector3)WarpPreDir * _warpRange;
        }

        public override void InvokeShoot()
        {
            throw new NotImplementedException();
        }

        public override void InvokeHalfShoot()
        {
            throw new NotImplementedException();
        }

        public override void InvokeFullShoot()
        {
            throw new NotImplementedException();
        }
    }
}