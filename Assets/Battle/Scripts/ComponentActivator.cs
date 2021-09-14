using UnityEngine;

namespace Altzone.Scripts.Apu
{
    public class ComponentActivator : MonoBehaviour
    {
        public MonoBehaviour target;

        public void ToggleState()
        {
            Debug.Log("ToggleState");
            target.enabled = !target.enabled;
        }

        public void SetEnabled()
        {
            Debug.Log("SetEnabled");
            target.enabled = true;
        }

        public void SetDisabled()
        {
            Debug.Log("SetDisabled");
            target.enabled = false;
        }
    }
}