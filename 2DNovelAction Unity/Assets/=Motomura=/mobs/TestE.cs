using System.Collections;
using System.Collections.Generic;
using BlackRose;
using UnityEngine;


namespace BlackRose
{
    public class Enemy_Test : UnitBase
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private float _jumpInterval = 2f; // ジャンプ間隔
        [SerializeField] private float _JumpTimer = 0f; // ジャンプタイマー
        private Jump _jumpState; // ジャンプステートのインスタンス
        
        protected override void RegisterStats()
        {
            var status = statusManager.GetStatusAmount(Status.Speed);//移動速度取得
            var state = new MoveOnGround(_rigidbody2D, "ここにアニメーション", status);// 移動ステートのインスタンスを作成
            _jumpState = new Jump(_rigidbody2D, statusManager.GetStatusAmount(Status.JumpPower)); // ジャンプステートのインスタンスを作成
            _stateMachine.AddState("move", state);// ステートマシンに移動ステートを追加
            _stateMachine.AddState("Jump", _jumpState); // ステートマシンにジャンプステートを追加
        }

        protected override string StateDecision()
        {
            if (_JumpTimer >= _jumpInterval)
            {
                _JumpTimer = 0f; // ジャンプタイマーをリセット
                _jumpState.HadLeapt = false; // ジャンプステートのフラグをリセット
                return "Jump"; // ジャンプステートに遷移
            }

            return "move"; // 常に移動ステートに遷移する
        }

        protected override void Update()
        {
            base.Update();
            _JumpTimer = Mathf.Min(_JumpTimer + Time.deltaTime, _jumpInterval); // ジャンプタイマーを更新
        }

        protected override void Awake()
        {
            base.Awake();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            if (_rigidbody2D == null)
            {
                Debug.LogError("Rigidbody2Dが見つかりません。Enemy_TestスクリプトをアタッチしたオブジェクトにRigidbody2Dコンポーネントを追加してください。");
            }
            Direction = Vector2.right; // 初期方向を右に設定
        }
    }

}


