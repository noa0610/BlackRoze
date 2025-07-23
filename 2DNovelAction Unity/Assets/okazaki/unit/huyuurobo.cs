//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace BlackRose
//{
//    public class Enemy_Test : UnitBase
//    {
//        [SerializeField] private Rigidbody2D _rigidbody2D;

//        protected override void RegisterStats()
//        {
//            var status = statusManager.GetStatusAmount(Status.Speed);// このキャラクターの移動速度を取得
//            var state = new MoveOnGround(_rigidbody2D, "ここにアニメーション", status); // 移動状態を定義
//            _stateMachine.AddState("move", state); // ステートマシンに移動状態を追加

//        }

//        protected override string StateDecision()
//        {

//            return "move"; // ここでは常に移動状態に遷移するように設定
//        }

//        protected override void Awake()
//        {
//            base.Awake();
//            _rigidbody2D = GetComponent<Rigidbody2D>();
//            if (_rigidbody2D == null)
//            {
//                Debug.LogError("Rigidbody2D component is missing on the Enemy_Test GameObject.");
//            }
//            Direction = Vector2.right; // 初期方向を右に設定
//        }
//    }
//}