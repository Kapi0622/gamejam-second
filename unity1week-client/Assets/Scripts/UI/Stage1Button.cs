using UnityEngine;
using UI; 
using Cysharp.Threading.Tasks;

public class Stage1Button : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        // Instance経由で命令を送る！
        SceneNavigator.Instance.ChangeSceneAsync("Stage1").Forget();
    }
}