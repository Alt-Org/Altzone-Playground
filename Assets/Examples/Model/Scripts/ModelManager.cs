using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Examples.Model.Scripts
{
    /// <summary>
    /// Manager for model UI.
    /// </summary>
    public class ModelManager : MonoBehaviour
    {
        [SerializeField] private UnitySceneName continueScene;
        [SerializeField] private UnitySceneName cancelScene;

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log($"LoadScene {cancelScene.sceneName}");
                SceneManager.LoadScene(cancelScene.sceneName);
            }
        }

        public void Continue()
        {
            Debug.Log($"LoadScene {continueScene.sceneName}");
            SceneManager.LoadScene(continueScene.sceneName);
        }
    }
}
