using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Examples.Game.Scripts
{
    /// <summary>
    /// Manager for bricks.
    /// </summary>
    public class BrickManager : MonoBehaviour
    {
        [SerializeField] private GameObject upperBricks;
        [SerializeField] private GameObject lowerBricks;
        [SerializeField] private int brickCount;

        private readonly Dictionary<int, BrickMarker> bricks = new Dictionary<int, BrickMarker>();

        private void Awake()
        {
            createBrickMarkersFor(upperBricks.transform);
            createBrickMarkersFor(lowerBricks.transform);
        }

        private void createBrickMarkersFor(Transform parentTransform)
        {
            var childCount = parentTransform.childCount;
            for (var i = 0; i < childCount; ++i)
            {
                var child = parentTransform.GetChild(i).gameObject;
                var marker = child.AddComponent<BrickMarker>();
                marker.BrickId = ++brickCount;
                bricks.Add(marker.BrickId, marker);
            }
        }

        public void deleteBrick(int brickId)
        {
            if (bricks.TryGetValue(brickId, out var brickMarker))
            {
                bricks.Remove(brickId);
                brickMarker.destroyBrick();
            }
        }
    }
}