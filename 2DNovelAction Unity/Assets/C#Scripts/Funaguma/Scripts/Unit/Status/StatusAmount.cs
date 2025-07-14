using System;
namespace BlackRose
{
    public class StatusAmount
    {
        public float defaultAmount;
        /// <summary>
        /// 現在の値に作用する為、HPなどの頻繁に変動する値以外では基本的に不要
        /// </summary>
        public float currentAmount;
        public float temporaryChanged;
        public float temporaryRatio = 1f; // 足し算・引き算で管理
        private bool _isDirty = true;
        private float _changedAmount = 0f;

        public float ChangedMax
        {
            get
            {
                if (_isDirty)
                {
                    _changedAmount = defaultAmount + temporaryChanged;
                    _changedAmount = Math.Max(0, _changedAmount * temporaryRatio);
                    _isDirty = false;
                }
                return _changedAmount;
            }
        }

        public StatusAmount(float defaultAmount)
        {
            this.defaultAmount = defaultAmount;
            currentAmount = defaultAmount;
        }

    }
}