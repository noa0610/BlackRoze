using Cysharp.Threading.Tasks;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace HighElixir
{
    public class SceneLoaderAsync
    {
        public enum Phase
        {
            None,
            LoadStart,      //  ロード開始
            Loading,        // ロード中
            LoadCompleted,  // ロード完了
            SceneChanged    // シーン遷移完了
        }
        private Scene _from;
        private static int _beforeSceneId;
        // シーン遷移許可フラグ
        private static bool _allowTransition = false;
        private static ReactiveProperty<Phase> _phase = new(Phase.None);
        private static FloatReactiveProperty _progress = new(0);
        private static bool _isloading = false;

        public static IObservable<Phase> PhaseObservable => _phase.AsObservable();
        public static IObservable<float> Progress => _progress.AsObservable();
        public static bool IsLoading => _isloading;

        // 外部から呼んで「もう遷移OK！」にするメソッド
        public static void AllowSceneTransition()
        {
            _allowTransition = true;
        }

        /// <summary>
        /// <see cref="ICallDataReceiver">ICallDataReceiver</see>を実装したコンポーネントを移動先のシーンにセットしている場合、任意のデータを引き渡すことができる</param>
        /// </summary>
        public void StartSceneLoad(int sceneId, bool changeDirectly = true, object data = null)
        {
            _from = SceneManager.GetActiveScene();
            _beforeSceneId = _from.buildIndex;
            _allowTransition = changeDirectly;
            LoadById(sceneId, data);
        }
        public void StartSceneLoad(string sceneName, bool changeDirectly = true, object data = null)
        {
            _from = SceneManager.GetActiveScene();
            _beforeSceneId = _from.buildIndex;
            _allowTransition = changeDirectly;
            LoadByName(sceneName, data);
        }
        public void SceneChangeBefore(bool changeDirectly = true, object data = null)
        {
            if (_from == null) return;
            StartSceneLoad(_beforeSceneId, changeDirectly, data);
        }
        private async UniTaskVoid LoadSceneAsync(AsyncOperation operation, object data = null, int id = -1, string name = "")
        {
            // ロード開始
            _isloading = true;
            _phase.Value = Phase.Loading;
            _progress.Value = 0f;
            operation.allowSceneActivation = false;
            while (operation.progress < 0.9f)
            {
                _progress.Value = Mathf.Clamp01(operation.progress / 0.9f);
                await UniTask.Yield();
            }

            // ロード完了演出
            _phase.Value = Phase.LoadCompleted;

            // ここで許可が出るまで待機
            await UniTask.WaitUntil(() => _allowTransition);
            operation.allowSceneActivation = true;
            await operation.ToUniTask();
            Scene scene;
            if (id != -1)
                scene = SceneManager.GetSceneByBuildIndex(id);
            else
                scene = SceneManager.GetSceneByName(name);
            SceneManager.SetActiveScene(scene);
            await SceneManager.UnloadSceneAsync(_from).ToUniTask();
            _ = Resources.UnloadUnusedAssets();

            // シーンチェンジ完了
            _phase.Value = Phase.SceneChanged;
            if (data != null)
            {
                foreach (var item in scene.GetRootGameObjects())
                {
                    ExecuteEvents.Execute<ICallDataReceiver>(item, null, (reciever, _) => reciever.Enter(data));
                }
            }
            await UniTask.Yield();
            _phase.Value = Phase.None;
            _isloading = false;
        }
        private void LoadByName(string name, object data)
        {
            var operation = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            LoadSceneAsync(operation, data, name: name).Forget();
        }
        private void LoadById(int id, object data)
        {
            var operation = SceneManager.LoadSceneAsync(id, LoadSceneMode.Additive);
            LoadSceneAsync(operation, data, id: id).Forget();
        }
    }

    public interface ICallDataReceiver : IEventSystemHandler
    {
        void Enter(object data);
    }
}
