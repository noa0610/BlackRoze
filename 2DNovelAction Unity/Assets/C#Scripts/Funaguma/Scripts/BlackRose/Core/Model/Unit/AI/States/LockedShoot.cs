using HighElixir;
using HighElixir.StateMachine;

namespace BlackRose.Core.Models.Units.State
{
    public class LockedShoot : State<AIController>
    {
        private SearchAndFire _search;
        private IntervalCounter _counter = new(5);
        public override void Enter()
        {
            _search = Cont.GetComponent<SearchAndFire>();
        }

        public override void Update(float deltaTime)
        {
            if (_counter.Check)
                _search.Search();
        }
    }
}