using BlackRose.Core.Models.Systems;
using BlackRose.Core.Models.Units;
using UnityEngine;

namespace BlackRose.Core.UI
{
    public class FollowUnit : MonoBehaviour, IPlayerFollower
    {
        [Header("References")]
        [SerializeField, Tooltip("追従するユニットを指定")] private UnitBase _target;
        [SerializeField, Tooltip("カメラ移動範囲コンポーネント")] private CameraBounds _cameraBounds;

        [Header("Settings")]
        [SerializeField, Tooltip("追従スピード (大きいほど速い)")] private float _smoothSpeed = 5f;
        [SerializeField, Tooltip("カメラの Z 座標")] private float _cameraZ = -10f;

        [SerializeField] private Transform _camT;

        private void Reset()
        {
            // Inspector から None の場合は自動取得
            _camT = GetComponent<Camera>()?.transform;
            if (!_cameraBounds)
                _cameraBounds = GetComponent<CameraBounds>();
        }

        private void Awake()
        {
            // キャッシュされていなければここで取得
            if (_camT == null)
                _camT = GetComponent<Camera>().transform;
        }

        private void LateUpdate()
        {
            if (_target == null || _camT == null) return;

            Vector3 currentPos = _camT.position;
            Vector3 targetPos = _target.transform.position;

            // まずターゲット位置をバウンス内にクランプ
            if (_cameraBounds != null)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, _cameraBounds.Left.x, _cameraBounds.Right.x);
                targetPos.y = Mathf.Clamp(targetPos.y, _cameraBounds.Down.y, _cameraBounds.Up.y);
            }

            Vector3 desiredPos = new Vector3(targetPos.x, targetPos.y, _cameraZ);

            // Clamp した位置へ常にスムーズに移動
            Vector3 smoothedPos = Vector3.Lerp(currentPos, desiredPos, _smoothSpeed * Time.deltaTime);

            _camT.position = smoothedPos;
        }

        public void SetTarget(UnitBase target)
        {
            _target = target;
        }
    }
}
// unicode