#if UNITY_EDITOR
using UnityEngine;

namespace Examples.Game.Scripts.Config
{
    public class RuntimeGameConfigTest : MonoBehaviour
    {
        public bool synchronizeAll;
        public bool synchronizeFeatures;
        public bool synchronizeVariables;

        private void Update()
        {
            if (synchronizeAll)
            {
                synchronizeAll = false;
                GameConfigSynchronizer.synchronize(What.All);
                return;
            }
            if (synchronizeFeatures)
            {
                synchronizeFeatures = false;
                GameConfigSynchronizer.synchronize(What.Features);
                return;
            }
            if (synchronizeVariables)
            {
                synchronizeVariables = false;
                GameConfigSynchronizer.synchronize(What.Variables);
            }
        }
    }
}
#endif