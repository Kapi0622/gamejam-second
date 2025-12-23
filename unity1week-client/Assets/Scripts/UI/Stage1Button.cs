using UnityEngine;
using UI; 
using Cysharp.Threading.Tasks;

public class Stage1Button : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        if (SceneNavigator.Instance != null)
        {
            // 「このオブジェクトが壊れたらキャンセル」という信号を渡す
            var ct = SceneNavigator.Instance.GetCancellationTokenOnDestroy();
            SceneNavigator.Instance.ChangeSceneAsync("Stage1", ct).Forget();
        }
    }
}