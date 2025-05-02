using System;
using UnityEngine;

namespace Test
{
    // 概要:
    // ShootForward クラスは、プレイヤーやユニットが前方に弾を発射する「攻撃ステート」の挙動を定義するクラスです。
    // IState インターフェースを実装し、状態の開始（Enter）、維持（Stay）、終了（Exit）において処理を分担しています。
    // 
    // 主な機能:
    // - Enter: 弾のプレハブ（Bullet）を指定位置に生成し、ステータス（速度・方向など）を設定して発射
    // - Stay: 攻撃完了時にコールバックイベント（OnShootComplete）を発火
    // - Exit: 攻撃終了時の処理（現時点では特に何もしていない）
    // 
    // 利用方法:
    // - ステートマシンに「shoot」状態として登録し、プレイヤーの攻撃タイミングで切り替え
    // - 弾の情報（BulletObject）は外部から注入（SetBullet）可能な設計
    //
    // 備考:
    // - 弾の生成には BulletObject 内の bulletData.bullet（プレハブ）を使用
    // - 弾の生成位置はプレイヤーの向いている方向にオフセットを加えた前方位置
    // - OnShootComplete を通じて外部に攻撃完了を通知（例: ステート切り替えなどに活用可能）
    // 
    // 拡張のヒント:
    // - 攻撃エフェクトや音声の再生追加
    // - 複数弾・連射・斜め撃ちなどの対応
    // - 弾の生成角度や位置の細かい調整処理の追加

    public class ShootForward : IState
    {
        private BulletObject _bulletObject;         // 発射する弾のデータ
        public event Action OnShootComplete;

        public BulletObject BulletObject => _bulletObject;
        public ShootForward(BulletObject bulletObject)
        {
            _bulletObject = bulletObject;
        }

        public bool Enter(IState previousState, IUnit parent)
        {
            return true;
        }

        public bool Exit(IState nextState, IUnit parent)
        {
            // 攻撃終了時（今は特に処理なし）
            return true;
        }

        public bool Stay(IUnit parent)
        {
            var b = _bulletObject.bulletData.bullet;
            if (b == null)
            {
                Debug.Log("Do not set bullet.");
                return false; // 弾のプレハブが設定されてなかったらエラー扱い
            }

            // 弾の生成位置（プレイヤーのちょっと前）
            Vector3 spawnPos = parent.Transform.position + new Vector3(parent.UnitStatus.direction.x * 1.5f, 0, 0);

            // 弾を生成
            Bullet instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);


            // ステータスをセット（速度、方向、ダメージなど）
            instantiatedBullet.SetBulletStatus(_bulletObject);
            OnShootComplete?.Invoke();
            return true;
        }

        public void SetBullet(BulletObject bullet)
        {
            _bulletObject = bullet;
        }
    }
}
//unicode