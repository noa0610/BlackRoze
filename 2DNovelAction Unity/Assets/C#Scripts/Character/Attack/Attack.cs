using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private AttackData _attackData; // 攻撃データ
    public AttackData AttackData { set { _attackData = value; } } // 攻撃データを設定

    private Collider2D _collider; // コライダー
    private float AttackDamage = 0.0f; // ダメージ
    private bool _isHit = false; // ヒットフラグ

    private List<int> _hitIDList = new List<int>(); // ヒットしたオブジェクトのIDリスト

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        _collider.enabled = false; // コライダーを無効化
    }

    public void Init()
    {
        _isHit = false; // ヒットフラグをリセット
        _collider.enabled = false; // コライダーを無効化
    }

    public void HitOn()
    {
        _collider.enabled = true; // コライダーを有効化
        _isHit = true; // ヒットフラグをセット
    }
    public void HitOff()
    {
        _collider.enabled = false; // コライダーを無効化
        _isHit = false; // ヒットフラグをリセット
    }
    public void IDListClear()
    {
        _hitIDList.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isHit == false) return; // ヒットフラグが立っていない場合は処理を終了
        if (_hitIDList.Contains(other.GetInstanceID())) return; // 既にヒットしたオブジェクトの場合は処理を終了

        // if (other.CompareTag("Player") || other.CompareTag("Enemy")) // ヒットしたオブジェクトがプレイヤーまたは敵の場合
        // {
        //     var damage = _attackData.GetDamage(); // ダメージを取得
        //     other.GetComponent<Health>().Damage(damage); // ヒットしたオブジェクトにダメージを与える
        // }
        // else if (other.CompareTag("Breakable")) // ヒットしたオブジェクトが壊せるオブジェクトの場合
        // {
        //     other.GetComponent<Breakable>().Break(); // 壊す処理を実行
        // }

        _hitIDList.Add(other.GetInstanceID()); // ヒットしたオブジェクトのIDをリストに追加
    }
}
