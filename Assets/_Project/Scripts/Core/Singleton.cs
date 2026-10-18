using UnityEngine;

namespace VARG.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] private bool _dontDestroyOnLoad = true;

        public static T Instance { get; private set; }

        public static bool HasInstance => Instance != null;

        protected virtual bool IsPersistent => _dontDestroyOnLoad;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this as T;
            if (IsPersistent)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
