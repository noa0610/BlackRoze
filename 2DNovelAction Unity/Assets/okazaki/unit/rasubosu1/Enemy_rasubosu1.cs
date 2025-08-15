using Unity.VisualScripting;
using UnityEngine;

namespace BlackRose
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public class Enemy_rasubosu1 : UnitBase
    {
        public enum States
        {
            none,
            idle,// 待機
            dead,// 死亡
            attackidle, // 攻撃待機
            armpunch, // アームパンチ
            diffusebeamgun, // 拡散ビーム砲
            firewall, // ファイアウォール
            headSeparationShot, // 頭部分離ショット
        }
        private enum Triggers
        {
            None,
            Attackcooldown, // 攻撃クールダウンした
            Attack1, // 攻撃１
            Attack2, // 攻撃２
            Attack3, // 攻撃３
            Attack4, // 攻撃４
            Attack1end, // 攻撃１した
            Attack2end, // 攻撃２した
            Attack3end, // 攻撃３した
            Attack4end, // 攻撃４した
            shockwaveend, // ショックウェーブした
            Event1, // イベント1が終わった
            Playerdead, // プレイヤーが死亡
            Died,          // 死亡した（HPが０になった）
        }
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            var idleTrigger = new[]                                // 待機ステートのトリガー
            {
                (Triggers.Event1, States.attackidle),              // イベント1が終わったで攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var attackidleTrigger = new[]                          // 攻撃待機ステートのトリガー
            {
                (Triggers.Playerdead, States.idle),                // プレイヤーが死亡で待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.Attack1, States.armpunch),               // アームパンチでアームパンチへ
                (Triggers.Attack2, States.diffusebeamgun),         // 拡散ビーム砲で拡散ビーム砲へ
                (Triggers.Attack3, States.firewall),               // ファイアウォールでファイアウォールへ
                (Triggers.Attack4, States.headSeparationShot),     // 頭分離ショットで頭分離ショットへ
            };
            var armpunchTrigger = new[]                            // アームパンチステートのトリガー        
            {
                (Triggers.Attack1end, States.attackidle),          // アームパンチ終了で攻撃待機へ         
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var diffusebeamgunTrigger = new[]                      //拡散ビーム砲ステートのトリガー          
            {
                (Triggers.Attack2end, States.attackidle),          // 拡散ビーム砲終了で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var firewallTrigger = new[]                            // ファイアウォールステートのトリガー
            {
                (Triggers.Attack3end, States.attackidle),          // ファイアウォール終了で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var headSeparationShotTrigger = new[]                  // 頭分離ショットステートのトリガー           
            {
                (Triggers.Attack4end, States.attackidle),          // 頭分離ショット終了で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.attackidle, attackidleTrigger)
                .AddTransmissions(States.armpunch, armpunchTrigger)
                .AddTransmissions(States.diffusebeamgun, diffusebeamgunTrigger)
                .AddTransmissions(States.firewall, firewallTrigger)
                .AddTransmissions(States.headSeparationShot, headSeparationShotTrigger);
            // 待機
            var idle = new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0);
            _stateMachine.AddState(States.idle, idle);
            // 死亡
            var died = new Idle().SetAnimeTrigger("died").SetCancelableProgress(0);
            died.OnAnimeationCompleted.AddListener(() =>
            {
                UnitManager.instance.RemoveUnit(this);
                Destroy(gameObject);
            });
            _stateMachine.AddState(States.dead, died);
            // 攻撃待機
            var attackidle = new Idle_LazyEvent(5f).SetAnimeTrigger("attackidle").SetCancelableProgress(0);
            attackidle.LazyEvent.AddListener(Attackjudgement);
            _stateMachine.AddState(States.attackidle, attackidle);
            // アームパンチ
            var armpunch = new Idle().SetAnimeTrigger("idle").SetCancelableProgress(0);
            _stateMachine.AddState(States.armpunch, armpunch);
            // 拡散ビーム砲
            var diffusebeamgun = new Idle().SetAnimeTrigger("diffusebeamgun").SetCancelableProgress(0);
            _stateMachine.AddState(States.diffusebeamgun, diffusebeamgun);
            // ファイアウォール
            var firewall = new Idle().SetAnimeTrigger("firewall").SetCancelableProgress(0);
            _stateMachine.AddState(States.firewall, firewall);
            // 頭分離ショット
            var headSeparationShot = new Idle().SetAnimeTrigger("headSeparationShot").SetCancelableProgress(0);
            _stateMachine.AddState(States.headSeparationShot, headSeparationShot);
        }
        private SearchAssistanceMono _searchAssistance;
        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (_searchAssistance.Execute("", list, out var ui))
            {
                // 最短距離のプレイヤーを狙う
                ui.Sort((a, b) =>
                {
                    var diffA = a.Transform.position - transform.position;
                    var diffB = b.Transform.position - transform.position;
                    return diffA.sqrMagnitude
                        .CompareTo(diffB.sqrMagnitude);
                });
            }
        }
        protected override void Awake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
            base.Awake();
        }
        private void FixedUpdate()
        {
            if (_isPlaying) SearchPlayer();
        }
        private void Attackjudgement()
        {
            int attackIndex = Random.Range(0, 4); // 0〜3 の間でランダム

            switch (attackIndex)
            {
                case 0:
                    Attack1();
                    break;
                case 1:
                    Attack2();
                    break;
                case 2:
                    Attack3();
                    break;
                case 3:
                    Attack4();
                    break;
            }
        }

        void Attack1()
        {
            Debug.Log("アームパンチ");
            _stateMachine.ChangeState(Triggers.Attack1);
        }

        void Attack2()
        {
            Debug.Log("拡散ビーム砲");
            _stateMachine.ChangeState(Triggers.Attack2);
        }
        void Attack3()
        {
            Debug.Log("ファイアウォール");
            _stateMachine.ChangeState(Triggers.Attack3);
        }
        void Attack4()
        {
            Debug.Log("頭分離ショット");
            _stateMachine.ChangeState(Triggers.Attack4);
        }
    }
}