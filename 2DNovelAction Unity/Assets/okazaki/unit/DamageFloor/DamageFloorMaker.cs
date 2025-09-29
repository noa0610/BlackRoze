using HighElixir;
using HighElixir.Pool;
using HighElixir.Timers;
using System.Collections.Generic;
using UnityEngine;

namespace BlackRose.Core.Models.Objects
{
    public class DamageFloorMaker : MonoBehaviour
    {
        [SerializeField] private DamageFloor _damageFloorPref;

        private Pool<DamageFloor> _pool;

        // ダメージフロアごとにTimeHolder用のキーを持たせる
        private Dictionary<DamageFloor, string> _floorTimerDict = new ();
        private Dictionary<string, DamageFloor> _floorTimerLink = new ();
        private Timer _timer = new();
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
            _floorTimerDict.Add(go, key);
            _floorTimerLink.Add(key, go);
            _timer.CountDownRegister(key, duration, () => { DestroyFloor(go); });
            _timer.Start(key);
            go.Generate(pos, maxLength);
            return go;
        }

        public void DestroyFloor(DamageFloor go)
        {
            if (_floorTimerDict.TryGetValue(go, out var key))
            {
                _floorTimerDict.Remove(go);
                _floorTimerLink.Remove(key);
                _timer.Unregister(key);
                _pool.Release(go); // プールに返す
            }
        }


        public void Update()
        {
            _timer.Update(Time.deltaTime);
        }
        private void Awake()
        {
            _pool = new Pool<DamageFloor>(_damageFloorPref, 10, transform);
        }
    }
}