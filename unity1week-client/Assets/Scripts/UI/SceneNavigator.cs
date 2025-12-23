using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace UI
{
    public class SceneNavigator : SingletonMonoBehaviour<SceneNavigator>
    {
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;
        
        protected override void Awake()
        {
            // 基盤のAwake（DontDestroyOnLoadなど）を実行する
            base.Awake();
            
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.alpha = 0f;
        }

        public async UniTask ChangeSceneAsync(string nextSceneName)
        {
            await FadeAsync(1f);
            
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                // "Manager" 以外のシーンは消去
                if (scene.name != "Manager")
                {
                    await SceneManager.UnloadSceneAsync(scene);
                }
            }

            // 次のシーンを読み込む
            await SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
    
            // 新しいシーンを「アクティブ」に設定（これで次からは GetActiveScene で取れるようになる）
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextSceneName));

            await FadeAsync(0f);
        }

        private async UniTask FadeAsync(float targetAlpha)
        {
            float startAlpha = fadeCanvasGroup.alpha;
            float elapsed = 0;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                await UniTask.Yield();
            }
            fadeCanvasGroup.alpha = targetAlpha;
        }
    }
}