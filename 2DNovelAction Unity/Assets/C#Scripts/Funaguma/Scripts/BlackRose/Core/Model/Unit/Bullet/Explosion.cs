using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace BlackRose.Core.Models.Units
{
    public class Explosion : Bullet
    {
        [Header("爆発")]
        [Tooltip("爆風のノックバック")]
        [SerializeField] private float _knockBack = 0f;
        [Tooltip("爆風の持続時間")]
        [SerializeField] private float _time = 1.5f;
        [Tooltip("爆風範囲")]
        [SerializeField, Min(0f)] private float _radius = 5f;
        [Tooltip("未設定（-5）の場合、_statusのdamageを使用")]
        [SerializeField] private float _damage = -5f;
        [Tooltip("爆発遅延")]
        [SerializeField, Min(0)] private float _delay = 0.4f;
        [Tooltip("TriggerSubjectをアタッチ")]
        [SerializeField] private ParticleSystem _explosion;
        [SerializeField] private AudioSource _sfx;
        protected override void Hitted_Target(Collider2D collision)
        {
            var e = Instantiate(_explosion, transform.position, Quaternion.identity);
            StartCoroutine(Play(e));
        }
        private IEnumerator Play(ParticleSystem particle)
        {
            var col = particle.GetComponent<CircleCollider2D>();
            col.radius = _radius;
            col.enabled = false;
            col.gameObject.OnCollisionEnter2DAsObservable().Subscribe(Collision =>
            {
                if (Collision.transform.TryGetComponent<Rigidbody2D>(out var rigidbody))
                {
                    // 爆発の中心から対象オブジェクトへの方向ベクトルを正規化
                    var direction = (Collision.transform.position - particle.transform.position).normalized;
                    // ノックバックの力を計算
                    var force = direction * _knockBack;
                    // Rigidbody2D に力を加える
                    rigidbody.AddForce(force, ForceMode2D.Impulse);
                    if (Collision.transform.TryGetComponent<UnitBase>(out var unit))
                        unit.StatusManager.TakeDamage(_damage == -5 ? _status.damage : _damage);
                }
            }).AddTo(col).AddTo(this);
            yield return new WaitForSeconds(_delay);
            col.enabled = true;// 有効化
            var t = _time;
            particle.Play();
            _sfx?.Play();
            while (t > 0 && particle.IsAlive())
            {
                yield return new WaitForFixedUpdate();
                t -= Time.fixedDeltaTime;
            }
            if (particle != null) particle.Stop();
            _sfx?.Stop();
            col.enabled = false;
            Destroy(particle.gameObject);
        }
    }
}