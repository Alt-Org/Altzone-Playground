using Prg.Scripts.Common.Photon;
using System.Collections.Generic;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Room
{
    public interface IBrickManager
    {
        void deleteBrick(int brickId);
    }

    /// <summary>
    /// Manager for bricks.
    /// </summary>
    public class BrickManager : MonoBehaviour, IBrickManager
    {
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase + 3;

        [SerializeField] private GameObject upperBricks;
        [SerializeField] private GameObject lowerBricks;
        [SerializeField] private int brickCount;

        private readonly Dictionary<int, BrickMarker> bricks = new Dictionary<int, BrickMarker>();

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            createBrickMarkersFor(upperBricks.transform);
            createBrickMarkersFor(lowerBricks.transform);
        }

        private void Start()
        {
            Debug.Log("Start");
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, data => { onDeleteBrick(data.CustomData); });
        }

        private void sendDeleteBrick(int brickId)
        {
            photonEventDispatcher.RaiseEvent(photonEventCode, brickId);
        }

        private void onDeleteBrick(object data)
        {
            var brickId = (int)data;
            if (bricks.TryGetValue(brickId, out var brickMarker))
            {
                bricks.Remove(brickId);
                brickMarker.destroyBrick();
                Debug.Log($"deleted Brick id={brickId} name={brickMarker.name}");
            }
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

        void IBrickManager.deleteBrick(int brickId)
        {
            Debug.Log($"deleteBrick id={brickId}");
            sendDeleteBrick(brickId);
        }
    }
}