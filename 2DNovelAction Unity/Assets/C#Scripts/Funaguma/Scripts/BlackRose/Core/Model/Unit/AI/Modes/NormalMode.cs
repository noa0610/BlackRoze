using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using HighElixir.Timers;
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
        private TimerTicket _skillTicket;

        [Header("States")]
        private Warp _warpState = new Warp();
        [SerializeField] private ShootForward _shoot = new ShootForward();
        [SerializeField] private ShootForward _half = new ShootForward();
        [SerializeField] private ShootForward _full = new ShootForward();

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private float _ct;
#endif
        public Vector2 WarpPreDir { get; private set; }

        public override void Register()
        {
            _skillTicket = Timer.CountDownRegister(_skillCT, "ワープCT", initZero: true);

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

            // Interval
            SM.AddTransitionsForLayer(
                AIController.Mode.Normal,
                AIStates.ShootInterval,
                    (Triggers.shootInput, NormalState.N_Shoot, "")
                );

            SM.AddTransitionForLayer(
                AIController.Mode.Normal.ToString(), 
                AIStates.Fall.ToString(), 
                Triggers.skillInput.ToString(), 
                NormalState.N_Skill.ToString(), "");

            SM.AddState(NormalState.N_Jump, _jump);
            SM.AddState(NormalState.N_Skill, _warpState);
            SM.AddState(NormalState.N_Shoot, _shoot);
            SM.AddState(NormalState.N_Half, _shoot);
            SM.AddState(NormalState.N_Full, _shoot);

            // event
            _warpState.OnCompleted += () =>
            {
                Debug.Log("==================");
                if (SM.ChangeState(Triggers.skillFinished))
                    Debug.Log("Warp Completed and State Changed");
                else
                    Debug.Log("Warp Completed but State Change Blocked");
            };

            _shoot.onShootComplete.AddListener(() =>
            {
                SM.LazyChange(Triggers.shootCompleted);
            });
        }

        public override void OnSkill(InputValue value)
        {
            if (!Timer.IsFinished(_skillTicket)) return;

            if (value.isPressed)
            {
                _preWarpDemo.SetActive(true);
            }
            else
            {
                // TODO : ワープ実行
                _preWarpDemo.SetActive(false);
                SM.ChangeState(Triggers.skillInput);
                Timer.Start(_skillTicket);
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
            SM.ChangeState(Triggers.shootInput);
        }

        public override void InvokeHalfShoot()
        {
            _shoot.SetDirection(_parent.Direction);
            SM.ChangeState(Triggers.shootInput);
        }

        public override void InvokeFullShoot()
        {
            _shoot.SetDirection(_parent.Direction);
            SM.ChangeState(Triggers.shootInput);
        }

#if UNITY_EDITOR
        public override void Update(float deltaTime)
        {
            if (Timer.TryGetCurrentTime(_skillTicket, out var rm)) _ct = rm;

        }
#endif
        public override void ModeChange_C()
        {
            _parent.SwitchModeLight();
        }

        public override void ModeChange_V()
        {
            _parent.SwitchModeHeavy();
        }

        public override void SetMuzzle(GameObject obj)
        {
            _shoot.SetGameObject(obj);
        }
    }
}