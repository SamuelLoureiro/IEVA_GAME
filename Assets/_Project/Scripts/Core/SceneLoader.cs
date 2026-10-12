using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VARG.UI;

namespace VARG.Core
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        [SerializeField] private SceneTransitionUI _transitionUI;
        private bool _isLoading;

        public bool IsLoading => _isLoading;

        public void SetTransitionUI(SceneTransitionUI transitionUI)
        {
            _transitionUI = transitionUI;
        }

        public void Load(string sceneName)
        {
            if (_isLoading) return;
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            _isLoading = true;

            if (_transitionUI != null)
            {
                yield return _transitionUI.FadeIn();
            }

            var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOperation != null)
            {
                while (!asyncOperation.isDone)
                {
                    var progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                    if (_transitionUI != null)
                    {
                        _transitionUI.UpdateProgress(progress);
                    }
                    yield return null;
                }
            }

            if (_transitionUI != null)
            {
                yield return _transitionUI.FadeOut();
            }

            _isLoading = false;
        }
    }
}
