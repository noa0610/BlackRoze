using BlackRose.Core.Models.States;
using BlackRose.Core.Models.Helper;
using HighElixir;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using BlackRose.Datas.Definitions;
namespace BlackRose.Core.Models.Units
{
    public partial class Diifusebeamgun
    {

        private enum States
        {
            none,
            idle,
            split,
            dead,
            splitidle,

            // Update is called once per frame
        }
        public enum Triggers
        {
            None,
            FoundPlayer,
            Split,
            SplitEnd,
            Dead
        }
        protected override void RegisterStats()
        {

            var idleTrigger = new[]
            {
                (Triggers.FoundPlayer, States.split, ""),
                (Triggers.Dead, States.dead, "")
            };
            var splitTrigger = new[]
            {
                (Triggers.SplitEnd, States.splitidle,""),
                (Triggers.Dead, States.dead,"")
            };
            var splitidleTrigger = new[]
            {
                (Triggers.Split, States.split,""),
                (Triggers.Dead, States.dead,"")
            };

            _stateMachine
                .AddTransitions(States.idle, idleTrigger)
                .AddTransitions(States.split, splitTrigger)
                .AddTransitions(States.splitidle, splitidleTrigger);

            // ステート登録
            // 待機 
            var idle = new Idle_LazyChange(Triggers.FoundPlayer.ToString(), 2, true);
            _stateMachine.AddState(States.idle, idle);
            // 分裂
            _multiShoot.onShootComplete.AddListener(() =>
            {
                PlaySE(_SpreadSEName, _SpreadSEVolume);
                _stateMachine.LazyChange(Triggers.SplitEnd);
            });

            _stateMachine.AddState(States.split, _multiShoot);


            // 分裂待機
            var splitidle = new Idle_LazyChange(Triggers.Split.ToString(), 5, true);
            _stateMachine.AddState(States.splitidle, splitidle);
            // 死亡
            _stateMachine.AddState(States.dead, new Idle());
        }
    }
}