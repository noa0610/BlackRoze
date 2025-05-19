using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlackRose
{
    public interface IStatusManager
    {
        public class StatusAmount
        {
            public readonly float defaultAmount;
            /// <summary>
            /// 現在の値に作用する為、HPなどの頻繁に変動する値以外では基本的に不要
            /// </summary>
            public float currentAmount;
            public float temporaryChanged;
            public float temporaryRatio = 1f; // 足し算・引き算で管理

            public float CurrentMax => (defaultAmount + temporaryChanged) * Math.Max(0, temporaryRatio);

            public StatusAmount(float defaultAmount)
            {
                this.defaultAmount = defaultAmount;
                currentAmount = defaultAmount;
            }

        }
        Dictionary<Status, StatusAmount> StatusPair { get; }

        Action DeadCallBack { get; set; }
        void TakeDamage(float damage);

        StatusAmount GetStatusAmount(Status status);
        /// <summary>
        /// 既に存在する場合は上書きされる
        /// </summary>
        /// <param name="status">追加したいステータス</param>
        /// <param name="amount">値</param>
        void AddStatus(Status status, float amount);

        /// <summary>
        /// ステータスを削除できるが、HP等の必須ステータスは残すこと
        /// Note:無敵にしたい場合はTakeDamageIncreaseを0にすればいい
        /// </summary>
        /// <param name="status"></param>
        void RemoveStatus(Status status);

        /// <summary>
        /// 値を更新することが明示的なメソッド
        /// 存在しないステータスを参照するとエラーログを返すが何もしない
        /// </summary>
        void UpdateStatus(Status status, float amount);

        /// <summary>
        /// 指定したステータスを増減させる倍率を足せる
        /// 0未満にはならない
        /// </summary>
        void AddRatio(Status status, float ratio);

        /// <summary>
        /// 指定したステータスのratioをリセット
        /// </summary>
        /// <param name="status"></param>
        void ResetRatio(Status status);

        /// <summary>
        /// 0未満にはならない
        /// </summary>
        void AddChanged(Status status, float amount);

        /// <summary>
        /// 指定したステータスの最大値の加減算をリセット
        /// </summary>
        /// <param name="status"></param>
        void ResetChanged(Status status);
    }
}
// unicode