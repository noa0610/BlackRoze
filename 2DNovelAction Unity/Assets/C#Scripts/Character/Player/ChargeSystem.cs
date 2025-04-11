using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeSystem
{
    private float OneChargeTime = 0.75f; // １段階目のチャージ時間
    private float TwoChargeTime = 2.0f;  // ２段階目のチャージ時間

    public enum ChargeState // チャージ状態
    {
        None,
        Charging,
        Charged1,
        Charged2,
    }
    private ChargeState _chargeState = ChargeState.None; // 現在のチャージ状態
    public ChargeState CurrentChargeState { get {return _chargeState;}} // 現在のチャージ状態を取得

    private float currentChargeTime = 0.0f; // 現在のチャージ時間


    /// <summary>
    /// チャージを開始する処理
    /// </summary>
    public void Charge()
    {
        // チャージ時間を増加させる
        currentChargeTime += Time.deltaTime;

        // チャージ開始状態
        if (currentChargeTime > OneChargeTime && currentChargeTime <= TwoChargeTime)
        {
            _chargeState = ChargeState.Charging;
        }
        // チャージ１段階状態
        else if (currentChargeTime > OneChargeTime)
        {
            _chargeState = ChargeState.Charged1;
        }
        // チャージ２段階状態
        else if (currentChargeTime > TwoChargeTime)
        {
            _chargeState = ChargeState.Charged2;
        }
    }

    /// <summary>
    /// チャージを開始する処理（チャージ速度倍率設定可能）
    /// </summary>
    /// <param name="ChargeSpeedMultiple">チャージ速度の倍率</param>
    public void Charge(float ChargeSpeedMultiple)
    {
        // チャージ時間を増加させる
        currentChargeTime += Time.deltaTime * ChargeSpeedMultiple;

        // チャージ開始状態
        if (currentChargeTime > OneChargeTime && currentChargeTime <= TwoChargeTime)
        {
            _chargeState = ChargeState.Charging;
        }
        // チャージ１段階状態
        else if (currentChargeTime > OneChargeTime)
        {
            _chargeState = ChargeState.Charged1;
        }
        // チャージ２段階状態
        else if (currentChargeTime > TwoChargeTime)
        {
            _chargeState = ChargeState.Charged2;
        }
    }

    /// <summary>
    /// チャージ状態をリセットする処理
    /// </summary>
    public void ReleaseCharge()
    {
        // チャージ状態をリセットする
        _chargeState = ChargeState.None;

        // チャージ時間をリセットする
        currentChargeTime = 0.0f;
    }
}