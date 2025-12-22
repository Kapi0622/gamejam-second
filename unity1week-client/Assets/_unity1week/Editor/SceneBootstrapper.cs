using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace _unity1week.Editor
{
    [InitializeOnLoad]
    public class SceneBootstrapper
    {
        // ここで最初に起動したいシーンのパスを指定
        private const string ManagerScenePath = "Assets/Scenes/Manager.unity";

        static SceneBootstrapper()
        {
            // エディタの設定で、再生時に必ず特定のシーンから始まるようにする
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ManagerScenePath);
            if (sceneAsset != null)
            {
                EditorSceneManager.playModeStartScene = sceneAsset;
                Debug.Log($"<color=cyan>【SceneBootstrapper】再生開始シーンを {ManagerScenePath} に固定</color>");
            }
            else
            {
                Debug.LogError($"【SceneBootstrapper】{ManagerScenePath} が見つかりません。");
            }
        }
    }
}
