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
    public sealed class NormalMode : AIModeBase
    {
        public enum NormalState
        {
            N_Shoot,
            N_Half,
            N_Full,
            N_Jump,
            N_Skill,
        }

        [Header("Warp")]
        [SerializeField] private float _warpRange;
        [SerializeField] private GameObject _preWarpDemo;
        // プレイヤーの中央になるように調整
        [SerializeField] private Vector3 _warpDemoDelta = new(0, 1, 0);
        [SerializeField] private float _skillCT = 4f;

        [Header("States")]
        [SerializeField] private Warp _warpState = new Warp();
        [SerializeField] private ShootForward _shoot = new ShootForward();

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private float _ct;
#endif
        public Vector2 WarpPreDir { get; private set; }

        public override void Register()
        {
            if (Timer.CountDownRegister(nameof(_skillCT), _skillCT))
                Debug.Log("登録！");

            // Idle
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                AIStates.Idle,
                    (Triggers.shootInput, NormalState.N_Shoot, ""),
                    (Triggers.halfCharge, NormalState.N_Half, ""),
                    (Triggers.fullCharge, NormalState.N_Full, ""),
                    (Triggers.jumpInput, NormalState.N_Jump, ""),
                    (Triggers.skillInput, NormalState.N_Skill, "")
                );
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                AIStates.ShootInterval,
                    (Triggers.shootInput, NormalState.N_Shoot, ""),
                    (Triggers.halfCharge, NormalState.N_Half, ""),
                    (Triggers.fullCharge, NormalState.N_Full, ""),
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

            // Shoot
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                NormalState.N_Shoot,
                    (Triggers.shootCompleted, AIStates.ShootInterval, "")
                );

            // Interval
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                AIStates.ShootInterval,
                    (Triggers.shootInput, NormalState.N_Shoot, "")
                );

            SM.AddState(NormalState.N_Jump, _jump);
            SM.AddState(NormalState.N_Skill, _warpState);
            SM.AddState(NormalState.N_Shoot, _shoot);
            SM.AddState(NormalState.N_Half, _shoot);
            SM.AddState(NormalState.N_Full, _shoot);

            // event
            _warpState.OnCompleted += () =>
            {
                Debug.Log("==================");
                SM.ChangeState(Triggers.skillFinished);
                Debug.Log("Current State: " + SM.CurrentState.key);
            };

            _shoot.onShootComplete.AddListener(() =>
            {
                SM.LazyChange(Triggers.shootCompleted);
            });
        }

        public override void OnSkill(InputValue value)
        {
            if (!Timer.IsFinished(nameof(_skillCT))) return;

            if (value.isPressed)
            {
                _preWarpDemo.SetActive(true);
            }
            else
            {
                // TODO : ワープ実行
                _preWarpDemo.SetActive(false);
                SM.ChangeState(Triggers.skillInput);
                Timer.Start(nameof(_skillCT));
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
            _shoot.SetDirection(_parent.Direction);
            _shoot.SetBullet(_parent.Bullets[0]);
            SM.ChangeState(Triggers.shootInput);
        }

        public override void InvokeHalfShoot()
        {
        }

        public override void InvokeFullShoot()
        {
        }

        public override void Update(float deltaTime)
        {
#if UNITY_EDITOR
            if (Timer.TryGetRemaining(nameof(_skillCT), out var rm)) _ct = rm;
#endif
        }

        public override void ModeChange_C()
        {
            _parent.SwitchModeLight();
        }

        public override void ModeChange_V()
        {
            _parent.SwitchModeHeavy();
        }
    }
}