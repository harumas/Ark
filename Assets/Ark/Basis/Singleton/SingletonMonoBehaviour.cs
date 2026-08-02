using UnityEngine;

namespace Ark
{
    /// <summary>
    /// MonoBehaviourをシングルトン化するクラス
    /// </summary>
    /// <typeparam name="T">派生型</typeparam>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T _Instance;

        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);

                    if (_Instance == null)
                    {
                        Debug.LogError(typeof(T) + " をアタッチしているGameObjectはありません");
                    }
                }

                return _Instance;
            }
        }

        protected virtual void Awake()
        {
            // 既に他のインスタンスが存在する場合は、重複したGameObjectごと破棄する
            if (_Instance != null && _Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _Instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDestroy()
        {
            // ドメインリロードを無効化している場合、破棄済みの参照が残り続けるのを防ぐ
            if (_Instance == this)
            {
                _Instance = null;
            }
        }
    }
}
