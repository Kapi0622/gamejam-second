using UnityEngine;

namespace UI
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // ヒエラルキーの中から自分と同じ種類の実体を探す
                    //unity6にしたことでFindObjectOfTypeが古くなり、二つのメソッドに分かれたらしい
                    _instance = Object.FindAnyObjectByType<T>();

                    if (_instance == null)
                    {
                        Debug.LogWarning(typeof(T) + " が見つかりません");
                    }
                }
                return _instance;
            }
        }
        protected virtual void Awake()
        {
            if (this != Instance)
            {
                // すでに実体があるなら自分を消して重複を防ぐ
                Destroy(this.gameObject);
                return;
            }

            // 基本的には消さない設定にする
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
