using System;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    // 概要:
    // IStopableObject は、ゲームプレイ中のオブジェクトに「一時停止」や「再開」「破棄」の処理を提供するためのインターフェースです。
    // 主に、ゲームの一時停止機能や、リソースの管理（Dispose）などで利用されます。
    public interface IStopableObject
    {
        // ゲームプレイを一時停止する処理
        void GamePlay_Pose();

        // 一時停止からゲームプレイを再開する処理
        void GamePlay_Continue();

        // オブジェクトの終了・破棄処理
        void Dispose();
    }

    // 概要:
    // IUnit は、ゲーム内の「ユニット（キャラクター、エネミー、NPCなど）」に共通する機能・情報を提供するためのインターフェースです。
    // ステータス（HPなど）や Transform を通じて、検索処理や描画・移動などの操作に使われることを想定しています。
    public interface IUnit
    {
        // ユニットのステータス（HP、攻撃力、移動速度など）を保持
        UnitStatus UnitStatus { get; set; }

        StateFlags StateFlags { get; }

        // Unityの Transform コンポーネント（位置や回転、スケール情報などを管理）
        Transform Transform { get; }
    }
}
// unicode