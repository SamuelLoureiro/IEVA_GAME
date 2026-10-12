using UnityEngine;

namespace VARG.Core
{
    public static class PersistentBootstrapper
    {
        private const string SystemsPrefabPath = "Systems/[Systems]";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureSystemsExist()
        {
            if (GameManager.Instance != null) return;

            var prefab = Resources.Load<GameObject>(SystemsPrefabPath);
            if (prefab != null)
            {
                var instance = Object.Instantiate(prefab);
                instance.name = "[Systems]";
                Object.DontDestroyOnLoad(instance);
            }
        }
    }
}
