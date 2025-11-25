using HighElixir;
using HighElixir.StateMachine;
using HighElixir.Unity;

namespace BlackRose.Core.Models.Units.State
{
    public class LockedShoot : State<AIController>
    {
        private SearchAndFire _search;
        public override void Enter()
        {
            _search = Cont.GetComponent<SearchAndFire>();
        }

        public override void Update(float deltaTime)
        {
            if (Interval.Check(5))
                _search.Search();
        }
    }
}