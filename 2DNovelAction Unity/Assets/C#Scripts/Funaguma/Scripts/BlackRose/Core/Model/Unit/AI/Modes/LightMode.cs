using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.States;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static BlackRose.Core.Models.Units.AIController;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public class LightMode : AIModeBase
    {
        public enum LightStates
        {
            L_Shoot,
            L_Half,
            L_Full,
            L_Jump,
            L_Skill,
        }
        [SerializeField] private MultiShoot _shoot;
        [SerializeField] private MultiShoot _half;
        [SerializeField] private MultiShoot _full;
        public override void Register()
        {
            // Idle
            SM.AddTransitionsForLayer(
                Mode.Light,
                AIStates.Idle,
                    (AITriggers.shootInput, LightStates.L_Shoot, ""),
                    (AITriggers.halfCharge, LightStates.L_Half, ""),
                    (AITriggers.fullCharge, LightStates.L_Full, ""),
                    (AITriggers.jumpInput, LightStates.L_Jump, ""),
                    (AITriggers.skillInput, LightStates.L_Skill, "")
                );
            SM.AddTransitionsForLayer(
                Mode.Light,
                AIStates.ShootInterval,
                    (AITriggers.shootInput, LightStates.L_Shoot, ""),
                    (AITriggers.halfCharge, LightStates.L_Half, ""),
                    (AITriggers.fullCharge, LightStates.L_Full, ""),
                    (AITriggers.skillInput, LightStates.L_Skill, "")
                );
            // Move
            SM.AddTransitionsForLayer(
                Mode.Light,
                AIStates.Move,
                    (AITriggers.jumpInput, LightStates.L_Jump, ""),
                    (AITriggers.skillInput, LightStates.L_Skill, "")
                );

            // Jump
            SM.AddTransitionsForLayer(
                Mode.Light,
                LightStates.L_Jump,
                    (AITriggers.falling, AIStates.Fall, ""),
                    (AITriggers.landing, AIStates.Idle, "")
                );
            SM.AddTransitionsForLayer(
                Mode.Light,
                LightStates.L_Jump,
                    (AITriggers.shootInput, LightStates.L_Shoot, ""),
                    (AITriggers.skillInput, LightStates.L_Skill, "")
                );

            // Skill
            SM.AddTransitionsForLayer(
                Mode.Light,
                LightStates.L_Skill,
                    (AITriggers.skillFinished, AIStates.Idle, "")
                );

            // Interval
            SM.AddTransitionsForLayer(
                Mode.Light,
                AIStates.ShootInterval,
                    (AITriggers.shootInput, LightStates.L_Shoot, "")
                );

            SM.AddState(LightStates.L_Jump, _jump);
            SM.AddState(LightStates.L_Shoot, _shoot);
            SM.AddState(LightStates.L_Half, _half);
            SM.AddState(LightStates.L_Full, _full);

            _shoot.onShootComplete.AddListener(() =>
            {
                SM.LazyChange(AITriggers.shootCompleted);
            });
            _half.onShootComplete.AddListener(() =>
            {
                SM.LazyChange(AITriggers.shootCompleted);
            });
            _full.onShootComplete.AddListener(() =>
            {
                SM.LazyChange(AITriggers.shootCompleted);
            });
        }

        public override void OnSkill(InputValue value)
        {
        }

        public override void OnInputMove(Vector2 dir)
        {
        }

        public override void FixedUpdate(float deltaTime)
        {
           
        }

        public override void InvokeShoot()
        {
            _shoot.SetDirection(_parent.Direction);
            SM.ChangeState(AITriggers.shootInput);
        }

        public override void InvokeHalfShoot()
        {
            _half.SetDirection(_parent.Direction);
            SM.ChangeState(AITriggers.halfCharge);
        }

        public override void InvokeFullShoot()
        {
            _full.SetDirection(_parent.Direction);
            SM.ChangeState(AITriggers.fullCharge);
        }

        public override void ModeChange_C()
        {
            _parent.SwitchModeHeavy();
        }

        public override void ModeChange_V()
        {
            _parent.SwitchModeNormal();
        }

        public override void SetMuzzle(GameObject obj)
        {
            _shoot.SetGameObject(obj);
            _half.SetGameObject(obj);
            _full.SetGameObject(obj);
        }
    }
}