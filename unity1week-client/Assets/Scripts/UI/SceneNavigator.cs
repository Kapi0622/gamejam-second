using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace UI
{
    public class SceneNavigator : MonoBehaviour
    {
        public static SceneNavigator Instance { get; private set; }

        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // 最初はクリックを通すようにしておく
                fadeCanvasGroup.blocksRaycasts = false;
                fadeCanvasGroup.alpha = 0f;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async UniTask ChangeSceneAsync(string nextSceneName)
        {
            // 1. フェードアウト開始
            fadeCanvasGroup.blocksRaycasts = true; // クリックをブロック開始！
            await FadeAsync(1f);

            // 2. シーン入れ替え
            string currentSceneName = SceneManager.GetActiveScene().name;
            await SceneManager.UnloadSceneAsync(currentSceneName);
            await SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextSceneName));

            // 3. フェードイン開始
            await FadeAsync(0f);
            fadeCanvasGroup.blocksRaycasts = false; // フェードが終わったらクリックを通す！
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