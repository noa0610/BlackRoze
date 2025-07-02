using UnityEngine;
using UniRx;
using HighElixir.Utilities;
using System;

namespace BlackRose.Core
{
    public class InputRestriction : SingletonBehavior<InputRestriction>
    {
        [Flags]
        public enum RestrictionType
        {
            None = 0,
            Movement = 1 << 0,
            Attack = 1 << 1,
            Pause = 1 << 2,
            Interact = 1 << 3,
            Player = Movement | Attack | Interact,
            All = Player | Pause
        }

        private ReactiveProperty<RestrictionType> _restriction = new ReactiveProperty<RestrictionType>(RestrictionType.None);

        public IObservable<RestrictionType> OnRestrictionChanged => _restriction;
        public void SetRestriction(RestrictionType restrictionType)
        {
            _restriction.Value = restrictionType;
        }

        public void EnterMovieMode()
        {
            SetRestriction(RestrictionType.All);
        }

        public void Release()
        {
            _restriction.Value = RestrictionType.None;
        }
    }
}