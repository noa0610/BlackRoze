using BlackRose.Core.Models.Helper;

namespace BlackRose.Core.Models.Units
{
    public class NormalMode : IAIState
    {
        private AIController _parent;
        public enum State
        {
            N_Idle,
            N_Shoot,
            N_Move,
            N_Jump,
            N_Dash,
            N_Skill
        }
        public enum Trigger
        {
            Shoot,
            Move,
            Jump,
            Dash,
            Skill
        }

        public void Register()
        {
            _parent.StateMachine.AddTransmissions(State.N_Idle,
                new[] { 
                    (Trigger.Shoot, State.N_Shoot),
                    (Trigger.Move, State.N_Move),
                    (Trigger.Jump, State.N_Jump),
                    (Trigger.Dash, State.N_Dash),
                    (Trigger.Skill, State.N_Skill)
                });
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