using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.Objects;
using BlackRose.Core.Models.States;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using static BlackRose.Core.Models.Units.AIController;
using Triggers = BlackRose.Core.Models.Units.AIController.AITriggers;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public class HeavyMode : AIModeBase
    {
        public enum HeavyState
        {
            H_Shoot,
            H_Jump,
            H_Skill
        }
        [Header("Heavy Shoot")]
        [Header("")]
        [SerializeField] private ShootForward _shoot;
        [Header("Reflect")]
        [SerializeField] private Reflect _reflect;
        [SerializeField] private ReflectMono _reflectObj;
        public override void Register()
        {
            SM.AddTransitionsForLayer(
                AIController.Mode.Heavy,
                AIController.AIStates.Idle,
                    (Triggers.shootInput, HeavyState.H_Shoot, ""),
                    (Triggers.jumpInput, HeavyState.H_Jump, ""),
                    (Triggers.skillInput, HeavyState.H_Skill, "")
                );

            SM.AddTransitionsForLayer(
                AIController.Mode.Heavy,
                AIController.AIStates.ShootInterval,
                (Triggers.shootInput, HeavyMode.HeavyState.H_Shoot, "")
                );
            SM.AddState(HeavyState.H_Jump, _jump);
            SM.AddState(HeavyState.H_Shoot, _shoot, AIController.Tags.Shoot.ToString());

            _shoot.onShootComplete.AsObservable().Subscribe(_ =>
            {
                SM.LazyChange(Triggers.shootCompleted);
            });

            SM.AddState(HeavyState.H_Skill, _reflect);

            _reflect.SetGameObject(_reflectObj.gameObject);

            var idle = new Idle_LazyEvent();
            SM.AddState(AIStates.ShootInterval, idle);

            idle.SetTime(0.3f);
            idle.OnCompleted += () =>
            {
                SM.ChangeState(AITriggers.watingTimeHasElapsed);
            };
        }

        public override void OnSkill(InputValue value)
        {
            if (value.isPressed)
            {
                SM.ChangeState(Triggers.skillInput);
            }
            else
            {
                SM.ChangeState(Triggers.skillFinished);
            }
        }

        public override void InvokeShoot()
        {
            _shoot.SetDirection(_parent.ShootDir);
            SM.ChangeState(Triggers.shootInput);
        }

        public override void InvokeHalfShoot()
        {
        }

        public override void InvokeFullShoot()
        {
        }

        public override void ModeChange_C()
        {
            _parent.SwitchModeNormal();
        }

        public override void ModeChange_V()
        {
            _parent.SwitchModeLight();
        }

        public override void SetMuzzle(GameObject obj)
        {
            _shoot.SetGameObject(obj);
        }
    }
}