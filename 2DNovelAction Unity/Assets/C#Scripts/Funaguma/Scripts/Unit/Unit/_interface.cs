using UnityEngine;

namespace BlackRose
{
    // 概要:
    // IStopableObject は、ゲームプレイ中のオブジェクトに「一時停止」や「再開」の処理を提供するためのインターフェースです。
    // 主に、ゲームの一時停止機能や、リソースの管理（Dispose）などで利用されます。
    public interface IStopableObject
    {
        // ゲームプレイを一時停止する処理
        void GamePlay_Pose();

        // 一時停止からゲームプレイを再開する処理
        void GamePlay_Continue();
    }
}
// unicode