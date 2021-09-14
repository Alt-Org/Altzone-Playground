using UnityEngine;

namespace Altzone.Scripts.Apu
{
    public class GameObjectActivator : MonoBehaviour
    {
        public GameObject target;

        public void ToggleState()
        {
            Debug.Log("ToggleState");
            target.SetActive(!target.activeSelf);
        }

        public void SetEnabled()
        {
            Debug.Log("SetEnabled");
            target.SetActive(true);
        }

        public void SetDisabled()
        {
            Debug.Log("SetDisabled");
            target.SetActive(false);
        }
    }
}
