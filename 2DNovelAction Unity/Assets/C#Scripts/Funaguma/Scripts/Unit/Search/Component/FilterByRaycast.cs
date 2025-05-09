using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BlackRose
{
    // 2D ���C�L���X�g���g���ăq�b�g�������j�b�g���t�B���^�����O
    public class FilterByRaycast : IFilterComponent
    {
        private readonly Vector2 _origin;
        private readonly Vector2 _direction;
        private readonly float _distance;
        private readonly int _layerMask;

        public FilterByRaycast(Vector2 origin, Vector2 direction, float distance, int layerMask = Physics2D.DefaultRaycastLayers)
        {
            _origin = origin;
            _direction = direction.normalized;
            _distance = distance;
            _layerMask = layerMask;
        }

        public List<IUnit> Execute(List<IUnit> pool)
        {
            // �������[���h�ɑ΂��ă��C�L���X�g�����s
            RaycastHit2D[] hits = Physics2D.RaycastAll(_origin, _direction, _distance, _layerMask);
            var hitUnits = new HashSet<IUnit>();
            foreach (var hit in hits)
            {
                var unit = hit.collider.GetComponent<IUnit>();
                if (unit != null)
                    hitUnits.Add(unit);
            }
            // �v�[�����̃��j�b�g�݂̂ɍi�荞��
            return pool.Where(u => hitUnits.Contains(u)).ToList();
        }
    }
}