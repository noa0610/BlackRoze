using Cysharp.Threading.Tasks;
using HighElixir;
using UnityEngine;

namespace BlackRose
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class GoalPost : MonoBehaviour
    {
        [SerializeField] private float _waitTime = 3f;
        [SerializeField] private string _toSceneName = "Title";
        private SceneLoaderAsync _loader = new();
        public bool _allowGoal = true;
        public async UniTask Goal()
        {
            if (_allowGoal && !SceneLoaderAsync.IsLoading)
            {
                UnitManager.instance.Pause(true, false);
                await UniTask.WaitForSeconds(_waitTime);
                _loader.StartSceneLoad(_toSceneName);
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag.Equals("Player"))
                _ = Goal();
        }
    }
}