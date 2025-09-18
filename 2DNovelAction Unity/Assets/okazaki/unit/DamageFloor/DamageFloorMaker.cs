using HighElixir;
using HighElixir.Pool;
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
        private TimeHolders _timer = new();
        private int _count = 0;

        /// <summary>
        /// 床を作成
        /// </summary>
        public DamageFloor Create(Vector2 pos, float duration)
        {
            var go = _pool.Get();
            go.transform.position = pos;
            var key = "damageFloor" + _count++;
            _floorTimerDict.Add(go, key);
            _floorTimerLink.Add(key, go);
            _timer.Register(key, duration, start:true, s => { DestroyFloor(go); });
            go.Generate(pos);
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