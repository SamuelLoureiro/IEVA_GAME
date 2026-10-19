using UnityEngine.SceneManagement;

namespace VARG.Core
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        private bool _isLoading;

        public bool IsLoading => _isLoading;

        public void Load(string sceneName)
        {
            if (_isLoading) return;
            _isLoading = true;
            SceneManager.LoadScene(sceneName);
            _isLoading = false;
        }
    }
}
