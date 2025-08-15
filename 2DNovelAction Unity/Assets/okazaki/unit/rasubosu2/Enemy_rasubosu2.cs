using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
namespace BlackRose
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public class Enemy_rasubosu2 : UnitBase
    {
        public enum States
        {
            none,
            idle,// 待機
            dead,// 死亡
            warpidle, // ワープ待機
            warp, // ワープ 
            attackidle, // 攻撃待機
            pointermissile, // ポインターミサイル
            crosswave, // クロスウェーブ
            warpShot, // ワープショット
            grappleSlash, // グラップルスラッシュ
            flashBeamSword, // フラッシュビームソード
        }
        private enum Triggers
        {
            None,
            Warpcooldown, // ワープクールダウンした
            Warpreturn, // ワープに戻る
            Warpend, // ワープ終了
            Warpcomplete, // ワープ完了
            Attackcooldown, // 攻撃クールダウンした
            Attack1,       // 攻撃１
            Attack2,       // 攻撃２
            Attack3,       // 攻撃３
            Attack4,       // 攻撃４
            Attack5,       // 攻撃5
            Attack1end,    // 攻撃１した
            Attack2end,    // 攻撃２した
            Attack3end,    // 攻撃３した
            Attack4end,    // 攻撃４した
            Attack5end,    // 攻撃5した
            Event1,        // イベント1が終わった
            Playerdead,    // プレイヤーが死亡
            Died,          // 死亡した（HPが０になった）
        }

        [SerializeField] private float closeRangeDistance = 5f; // 近距離判定の距離
        private Transform playerTransform;
        protected override void RegisterStats()
        {
            // トランスミッショングループを作成
            var idleTrigger = new[]                                // 待機ステートのトリガー
            {
                (Triggers.Event1, States.attackidle),              // イベント1発生で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var warpidleTrigger = new[]                            // ワープ待機ステートのトリガー
            {
                (Triggers.Warpcooldown, States.warp),               // ワープクールダウンでワープへ
                (Triggers.Warpcomplete, States.attackidle),         // ワープ完了で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var warpTrigger = new[]                                // ワープステートのトリガー
            {
                (Triggers.Warpend, States.warp),              // ワープ終了でワープへ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var attackidleTrigger = new[]                          // 攻撃待機ステートのトリガー
            {
                (Triggers.Playerdead, States.idle),                // プレイヤーが死亡で待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
                (Triggers.Attack1, States.pointermissile),         //ポインターミサイルへ 
                (Triggers.Attack2, States.crosswave),              //クロスウェーブへ
                (Triggers.Attack3, States.warpShot),               //ワープショットへ
                (Triggers.Attack4, States.grappleSlash),           // グラップルスラッシュへ
                (Triggers.Attack5, States.flashBeamSword),         // フラッシュビームソードへ
            };
            var pointermissileTrigger = new[]                      //ポインターミサイルステートのトリガー        
            {
                (Triggers.Attack1end, States.attackidle),          //ポインターミサイル終了で攻撃待機へ       
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var crosswaveTrigger = new[]                           //ステートのトリガー           
            {
                (Triggers.Attack2end, States.attackidle),          // クロスウェーブ終了で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var warpShotTrigger = new[]                            // ワープショットステートのトリガー
            {
                (Triggers.Attack3end, States.attackidle),          //  ワープショット終了で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var grappleSlashTrigger = new[]                        //グラップルスラッシュステートのトリガー
            {
                (Triggers.Attack4end, States.attackidle),          // 
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            var flashBeamSwordTrigger = new[]                      // フラッシュビームソードステートのトリガー                  
            {
                (Triggers.Attack5end, States.attackidle),          // フラッシュビームソード終了で攻撃待機へ
                (Triggers.Died, States.dead),                      // 死亡で死へ
            };
            // ステートマシンにStatesの移動先の追加
            _stateMachine
                .AddTransmissions(States.idle, idleTrigger)
                .AddTransmissions(States.attackidle, attackidleTrigger)
                .AddTransmissions(States.warpidle, warpidleTrigger)
                .AddTransmissions(States.warp, warpTrigger)
                .AddTransmissions(States.pointermissile, pointermissileTrigger)
                .AddTransmissions(States.crosswave, crosswaveTrigger)
                .AddTransmissions(States.warpShot, warpShotTrigger)
                .AddTransmissions(States.grappleSlash, grappleSlashTrigger)
                .AddTransmissions(States.flashBeamSword, flashBeamSwordTrigger);
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
            var attackIdle = new Idle_LazyEvent(5f).SetAnimeTrigger("attackidle").SetCancelableProgress(0);
            attackIdle.LazyEvent.AddListener(Attackjudgement);
            _stateMachine.AddState(States.attackidle, attackIdle);
            // ワープ待機
            var warpidle = new Idle_LazyChange(Triggers.Warpcooldown.ToString(), 5, true);
            _stateMachine.AddState(States.warpidle, warpidle);
            // ワープ

            // ポインターミサイル
            var pointermissile = new Idle().SetAnimeTrigger("pointermissile").SetCancelableProgress(0);
            _stateMachine.AddState(States.pointermissile, pointermissile);
            // 
            var crosswave = new Idle().SetAnimeTrigger("crosswave").SetCancelableProgress(0);
            _stateMachine.AddState(States.crosswave, crosswave);
            // 
            var warpShot = new Idle().SetAnimeTrigger("warpShot").SetCancelableProgress(0);
            _stateMachine.AddState(States.warpShot, warpShot);
            // 
            var grappleSlash = new Idle().SetAnimeTrigger("grappleSlash").SetCancelableProgress(0);
            _stateMachine.AddState(States.grappleSlash, grappleSlash);
            // 
            var flashBeamSword = new Idle().SetAnimeTrigger("flashBeamSword").SetCancelableProgress(0);
            _stateMachine.AddState(States.flashBeamSword, flashBeamSword);
        }
        private void Start()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Playerタグの付いたオブジェクトが見つかりませんでした。");
            }
        }
        private void Attackjudgement()
        {
            if (playerTransform == null) return;
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= closeRangeDistance)
            {
                int attackIndex1 = Random.Range(0, 2); // 0〜1 の間でランダム

                switch (attackIndex1)
                {
                    case 0:
                        Attack4();
                        break;
                    case 1:
                        Attack5();
                        break;
                }
            }
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
                case 4:
                    Attack5();
                    break;
            }
        }

        void Attack1()
        {
            Debug.Log("ポインターミサイル");
            _stateMachine.ChangeState(Triggers.Attack1);
        }

        void Attack2()
        {
            Debug.Log("クロスウェーブ");
            _stateMachine.ChangeState(Triggers.Attack2);
        }
        void Attack3()
        {
            Debug.Log("ワープショット");
            _stateMachine.ChangeState(Triggers.Attack3);
        }
        void Attack4()
        {
            Debug.Log("グラップルスラッシュ");
            _stateMachine.ChangeState(Triggers.Attack4);
        }
        void Attack5()
        {
            Debug.Log("フラッシュビームソード");
            _stateMachine.ChangeState(Triggers.Attack5);
        }
    }
}
