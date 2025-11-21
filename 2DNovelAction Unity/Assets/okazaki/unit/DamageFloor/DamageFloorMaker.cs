using HighElixir;
using HighElixir.Pools;
using HighElixir.Timers;
using HighElixir.Unity.Pools;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.Objects
{
    public class DamageFloorMaker : MonoBehaviour
    {
        [SerializeField] private DamageFloor _damageFloorPref;

        private ObjectPool<DamageFloor> _pool;

        // ダメージフロアごとにTimeHolder用のキーを持たせる
        private Dictionary<DamageFloor, TimerTicket> _floorTimerDict = new();
        private Dictionary<TimerTicket, DamageFloor> _floorTimerLink = new();
        private int _count = 0;

        /// <summary>
        /// 床を作成
        /// </summary>
        /// <param name="duration">床の持続時間</param>
        /// <param name="maxLength">床の最大長さ</param>
        /// <param name="pos">床の中心(地形に依存する為必ずしも中心にはならない)</param>
        public DamageFloor Create(Vector2 pos, float duration, float maxLength)
        {
            var go = _pool.Get();
            go.transform.position = pos;
            var key = "damageFloor" + _count++;
            var ticket = GlobalTimer.Update.CountDownRegister(duration, key, () => { DestroyFloor(go); });
            GlobalTimer.Update.Start(ticket);
            _floorTimerDict.Add(go, ticket);
            _floorTimerLink.Add(ticket, go);
            go.Generate(pos, maxLength);
            return go;
        }

        public void DestroyFloor(DamageFloor go)
        {
            if (_floorTimerDict.TryGetValue(go, out var key))
            {
                _floorTimerDict.Remove(go);
                _floorTimerLink.Remove(key);
                GlobalTimer.Update.UnRegister(key);
                _pool.Pool.Release(go); // プールに返す
            }
        }

        private void Awake()
        {
            _pool = new ObjectPool<DamageFloor>(_damageFloorPref, 10, transform);
        }
    }
}