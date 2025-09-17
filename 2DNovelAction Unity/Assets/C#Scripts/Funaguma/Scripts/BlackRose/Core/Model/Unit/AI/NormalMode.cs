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
        [SerializeField] private float _skillCT = 4f;
        [SerializeField] private float _ct;
        [SerializeField] private Warp _warpState = new Warp();

        public Vector2 WarpPreDir { get; private set; }

        public override void Register()
        {
            if (TimeHolders.Register(nameof(_skillCT), _skillCT))
                Debug.Log("登録！");

            // Idle
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                AIStates.Idle,
                    (Triggers.shootInput, NormalState.N_Shoot, ""),
                    (Triggers.jumpInput, NormalState.N_Jump, ""),
                    (Triggers.skillInput, NormalState.N_Skill, "")
                );

            // Move
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                AIStates.Move,
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
                    (Triggers.shootInput, NormalState.N_Shoot, ""),
                    (Triggers.skillInput, NormalState.N_Skill, "")
                );

            // Skill
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                NormalState.N_Skill,
                    (Triggers.skillFinished, AIStates.Idle, "")
                );


            SM.AddState(NormalState.N_Jump, _jump);
            SM.AddState(NormalState.N_Skill, _warpState);
            _warpState.OnCompleted += () =>
            {
                Debug.Log("==================");
                SM.ChangeState(Triggers.skillFinished);
                Debug.Log("Current State: " + SM.CurrentState.key);
            };
        }

        public override void OnSkill(InputValue value)
        {
            if (!TimeHolders.IsFinished(nameof(_skillCT), true)) return;

            if (value.isPressed)
            {
                _preWarpDemo.SetActive(true);
            }
            else
            {
                // TODO : ワープ実行
                _preWarpDemo.SetActive(false);
                SM.ChangeState(Triggers.skillInput);
                TimeHolders.Start(nameof(_skillCT));
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
            _warpState.SetPos(_preWarpDemo.transform.position);
        }

        public override void InvokeShoot()
        {
        }

        public override void InvokeHalfShoot()
        {
        }

        public override void InvokeFullShoot()
        {
        }

        public override void Update(float deltaTime)
        {
            if (TimeHolders.TryGetRemaining(nameof(_skillCT), out var rm)) _ct = rm;
        }
    }
}