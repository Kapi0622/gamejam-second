using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading; 

namespace UI
{
    public class SceneNavigator : SingletonMonoBehaviour<SceneNavigator>
    {
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        protected override void Awake()
        {
            base.Awake();
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.alpha = 0f;
        }

        // 引数に CancellationToken を追加
        public async UniTask ChangeSceneAsync(string nextSceneName, CancellationToken ct = default)
        {
            try 
            {
                
                await FadeAsync(1f, ct);

                // 今あるManager以外のシーンを削除
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.name != "Manager")
                    {
                        // WithCancellation(ct) をつけることで、読み込み中もキャンセル可能になる
                        await SceneManager.UnloadSceneAsync(scene).WithCancellation(ct);
                    }
                }

                await SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive).WithCancellation(ct);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextSceneName));

                // フェードイン
                await FadeAsync(0f, ct);
            }
            catch (System.OperationCanceledException)
            {
                // 途中でボタンが消えたりした時は、ここで安全に止まる
                Debug.Log("シーン遷移がキャンセルされました");
            }
        }

        private async UniTask FadeAsync(float targetAlpha, CancellationToken ct)
        {
            float startAlpha = fadeCanvasGroup.alpha;
            float elapsed = 0;

            while (elapsed < fadeDuration)
            {
                
                ct.ThrowIfCancellationRequested();

                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                await UniTask.Yield();
            }
            fadeCanvasGroup.alpha = targetAlpha;
        }
    }
}