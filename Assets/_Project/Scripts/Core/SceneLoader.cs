using UnityEngine;
using UnityEngine.SceneManagement;

namespace VARG.Core
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        public void Load(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }
}
