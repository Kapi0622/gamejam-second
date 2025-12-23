using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace UI
{
    public class ManagerRoot : MonoBehaviour
    {
        private void Awake()
        {
            //シーン切り替えで消さないようにする
            DontDestroyOnLoad(gameObject);
        }
        // Startは普通のvoidにして、その中でUniTaskを投げっぱなしにする
        private void Start()
        {
            InitializeGame().Forget(); 
        }

        private async UniTaskVoid InitializeGame() 
        {
            try 
            {
                // ここでシーンを読み込む
                await SceneManager.LoadSceneAsync("Title", LoadSceneMode.Additive);
                
                Debug.Log("Titleシーンの読み込み成功！");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"エラーが発生：{e.Message}");
            }
        }
    }
}