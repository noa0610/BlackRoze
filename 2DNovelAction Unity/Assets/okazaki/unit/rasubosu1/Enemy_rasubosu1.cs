using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using UnityEngine;
using HighElixir;
using System.Collections.Generic;
using BlackRose.Datas.Definitions;
namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public class Enemy_rasubosu1 : UnitBase
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private GameObject bulletPrefab;
        private UnitBase _player;
        private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetDict<States>();
        [SerializeField] private BulletData _firewallBulletData; // 必要ならInspectorでセット
        [SerializeField] private LayerMask _firewallTargetLayer; // 必要ならInspectorでセット
        [SerializeField] private Transform _firewallPoints; // 必要ならInspectorでセット
        
        public enum States
        {
            none,
            idle,// 待機
            dead,// 死亡
            attackidle, // 攻撃待機
            armpunch, // アームパンチ
            diffusebeamgun, // 拡散ビーム砲
            firewall, // ファイアウォール
        }
        private enum Triggers
        {
            None,
            FoundPlayer,   // プレイヤーを発見した
            Attackcooldown, // 攻撃クールダウンした
            Attack1, // 攻撃１
            Attack2, // 攻撃２
            Attack3, // 攻撃３
            Attack4, // 攻撃４
            Attack1end, // 攻撃１した
            Attack2end, // 攻撃２した
            Attack3end, // 攻撃３した
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
                (Triggers.FoundPlayer, States.attackidle),              // イベント1が終わったで攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var attackidleTrigger = new[]                          // 攻撃待機ステートのトリガー
            {
                (Triggers.Playerdead, States.idle),                // プレイヤーが死亡で待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.Attack1, States.armpunch),               // アームパンチでアームパンチへ
                (Triggers.Attack2, States.diffusebeamgun),         // 拡散ビーム砲で拡散ビーム砲へ
                (Triggers.Attack3, States.firewall),               // ファイアウォールでファイアウォールへ
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
            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.attackidle, attackidleTrigger)
                .AddTransmissions(States.armpunch, armpunchTrigger)
                .AddTransmissions(States.diffusebeamgun, diffusebeamgunTrigger)
                .AddTransmissions(States.firewall, firewallTrigger);
            // 待機
            var idle = new Idle();
            _stateMachine.AddState(States.idle, idle);
            // 死亡
            var died = new Idle()
                .SetWaitTick<Idle>(25, () =>
            {
                UnitManager.instance.RemoveUnit(this);
                Destroy(gameObject);
            });
            _stateMachine.AddState(States.dead, died);
            // 攻撃待機
            var attackidle = new Idle_LazyEvent(3f);
            attackidle.OnCompleted += Attackjudgement;
            _stateMachine.AddState(States.attackidle, attackidle);
            // アームパンチ
            var armpunch = new Idle();
            _stateMachine.AddState(States.armpunch, armpunch);
            // 拡散ビーム砲
            var diffusebeamgun = new Idle();
            _stateMachine.AddState(States.diffusebeamgun, diffusebeamgun);
            // ファイアウォール
            var firewall = new ShootForward(_firewallBulletData, _firewallTargetLayer)
            .SetDirection(Vector2.right)
            .SetMuzzle(_firewallPoints? _firewallPoints.gameObject : gameObject);
            // 弾発射完了時にショックウェーブ終了トリガーを発火
            firewall.onShootComplete.AddListener(() =>
            {
                _stateMachine.LazyChange(Triggers.Attack3end);
            });
            _stateMachine.AddState(States.firewall, firewall);
        }
        private SearchAssistanceMono _searchAssistance;


        private void SearchPlayer()
        {
            var list = UnitManager.instance.GetUnitList();
            if (IsMatchingState(States.idle) && _searchAssistance.Execute("yellow", list, out var units))
            {
                _player = units.GetUnitNearest(transform.position);
                _stateMachine.ChangeState(Triggers.FoundPlayer);
            }
        }

        protected override void BeforeAwake()
        {
            _searchAssistance = GetComponent<SearchAssistanceMono>();
        }

        protected override void AfterFixedUpdate()
        {
            SearchPlayer();
            // 見た目の向き変更など既存処理
            if (_player != null)
            {
                Direction = (_player.Transform.position - transform.position).normalized;
                if (Direction.x != 0)
                {
                    var scale = transform.localScale;
                    scale.x = Mathf.Abs(scale.x) * (Direction.x > 0 ? 1 : -1);
                    transform.localScale = scale;
                }
            }
        }
        private bool IsMatchingState(States state)
        {
            return _stateMachine.CurrentState.key == _stateNames[state];
        }
        public void Attackjudgement()
        {
            int attackIndex = Random.Range(0, 3); // 0〜3 の間でランダム

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
            }
        }

        void Attack1()
        {
            Debug.Log("アームパンチ");
            _stateMachine.ChangeState(Triggers.Attack3);
        }

        void Attack2()
        {
            Debug.Log("拡散ビーム砲");
            _stateMachine.ChangeState(Triggers.Attack3);
        }
        void Attack3()
        {
            Debug.Log("ファイアウォール");
            _stateMachine.ChangeState(Triggers.Attack3);
        }

    }
    
}