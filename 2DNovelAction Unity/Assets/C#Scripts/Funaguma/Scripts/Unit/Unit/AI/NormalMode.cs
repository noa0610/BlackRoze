using System.Collections.Generic;

namespace BlackRose
{
    public class NormalMode : IAIState
    {
        private AIController _parent;
        private enum State
        {
            N_Idle,
            N_Shoot,
            N_Move,
            N_Jump,
            N_Dash,
            N_Skill
        }
        private enum Trigger
        {
            Shoot,
            Move,
            Jump,
            Dash,
            Skill
        }

        public List<IState> Register()
        {
            var res = new List<IState>();
            _parent.Translation.Add(State.N_Idle.ToString(), new()
            {
                { Trigger.Shoot.ToString(), "ShootMode" },
                { Trigger.Move.ToString(), "MoveMode" },
                { Trigger.Jump.ToString(), "JumpMode" },
                { Trigger.Dash.ToString(), "DashMode" },
                { Trigger.Skill.ToString(), "SkillMode" }
            });
            return res;
        }

        public string OnShoot()
        {
            throw new System.NotImplementedException();
        }

        public string OnMove()
        {
            throw new System.NotImplementedException();
        }

        public string OnJump()
        {
            throw new System.NotImplementedException();
        }

        public string OnDash()
        {
            throw new System.NotImplementedException();
        }

        public string OnSkill()
        {
            throw new System.NotImplementedException();
        }

        public NormalMode(AIController parent)
        {
            _parent = parent;
        }
    }
}