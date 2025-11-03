using BlackRose.Core.Models.Helper;
using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.States;
using HighElixir;
using System.Collections.Generic;
using UnityEngine;
namespace BlackRose.Core.Models.Units
{
    [RequireComponent(typeof(SearchAssistanceMono))]
    public partial class Enemy_rasubosu2 : UnitBase
    {
        [SerializeField] private float closeRangeDistance = 5f; // 近距離判定の距離
        private Transform playerTransform;
        protected override void Start()
        {
            base.Start();
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
