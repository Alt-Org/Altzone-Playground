using UnityEngine;
using UnityEngine.SceneManagement;

namespace Altzone.Scripts.Apu
{
    public class LoadLevel : MonoBehaviour
    {
        public string levelName;

        private void Start()
        {
            SceneManager.LoadScene(levelName);
        }
    }
}