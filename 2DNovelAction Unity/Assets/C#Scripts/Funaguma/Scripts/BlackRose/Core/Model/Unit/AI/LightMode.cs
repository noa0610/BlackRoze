using BlackRose.Core.Models.Helper;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using Triggers = BlackRose.Core.Models.Units.AIController.AITriggers;
using BlackRose.Core.Models.States;

namespace BlackRose.Core.Models.Units
{
    [Serializable]
    public class LightMode : AIModeBase
    {
        public enum LightStates
        {
            L_Shoot,
            L_Jump,
            L_Skill,
        }
        [SerializeField] private float _warpRange;
        [SerializeField] private GameObject _preWarpDemo;

        // ステート
        // Note : 仮置きのステートは StateComp で代用
        [SerializeField] private StateComp _warp;

        public Vector2 WarpPreDir { get; private set; }
        public override void Register()
        {
            SM.AddTransitionsForLayer(
                AIController.Mode.Light,
                AIController.AIStates.Idle,
                    (Triggers.shootInput, LightStates.L_Shoot, ""),
                    (Triggers.jumpInput, LightStates.L_Jump, ""),
                    (Triggers.skillInput, LightStates.L_Skill, "")
                );

            SM.AddState(LightStates.L_Jump, _jump);
            SM.AddState(LightStates.L_Skill, _warp);
        }

        public override void OnSkill(InputValue value)
        {
            if (value.isPressed)
            {
                _preWarpDemo.SetActive(true);
            }
            else
            {
                // TODO : ワープ実行
                SM.ChangeState(Triggers.skillInput);
            }
        }

        public override void OnInputMove(Vector2 dir)
        {
            WarpPreDir = dir;
        }

        public override void FixedUpdate(float deltaTime)
        {
            _preWarpDemo.transform.position = _parent.transform.position + (Vector3)WarpPreDir * _warpRange;
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