#if UNITY_EDITOR
using Examples.Game.Scripts.Battle.Room;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Test
{
    public class BrickManagerTest : MonoBehaviour
    {
        public int brickId;
        public bool delete;
        public BrickManager brickManager;

        private void Update()
        {
            if (delete)
            {
                brickManager = FindObjectOfType<BrickManager>();
                if (brickManager is IBrickManager manager)
                {
                    manager.deleteBrick(brickId);
                    brickId = 0;
                    delete = false;
                }
            }
        }
    }
}
#endif