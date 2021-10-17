using UnityEngine;

namespace Altzone.Scripts.Test
{
    // See: Simple Hex-Based Game Design for Unity 3d
    // https://www.youtube.com/watch?v=td3O1tkbqYQ
    public class HexDemo : MonoBehaviour
    {
        [Header("Room Properties (Furniture)"), SerializeField] private GameObject[] roomPropPrefabs;
        [SerializeField, Range(0f, 1f)] private float tileFillRatio;

        [Header("Hex Tile Prefabs"), SerializeField] private GameObject[] tilePrefabs;

        [Header("Hex Tile Settings"), SerializeField] private Vector3 startPosition;
        [SerializeField] private int mapWidth;
        [SerializeField] private int mapHeight;
        [SerializeField] private float tileOffsetX; // 0.882
        [SerializeField] private float tileOffsetZ; // 0.764

        private void Start()
        {
            var prefabIndex = -1;
            var parentTransform = transform;
            for (var x = 0; x < mapWidth; ++x)
            {
                for (var y = 0; y < mapHeight; ++y)
                {
                    var xAdjust = y % 2 == 1 ? tileOffsetX / 2f : 0f;
                    var position = new Vector3(startPosition.x + x * tileOffsetX + xAdjust, startPosition.y, startPosition.z + y * tileOffsetZ);
                    prefabIndex = ++prefabIndex % tilePrefabs.Length;
                    var instance = Instantiate(tilePrefabs[prefabIndex], position, Quaternion.identity);
                    instance.name = $"Hex({x},{y})";
                    instance.transform.parent = parentTransform;
                    var isAddRoomProp = roomPropPrefabs.Length > 0 && Random.Range(0f, 1f) < tileFillRatio;
                    if (isAddRoomProp)
                    {
                        addRoomProp(instance.transform);
                    }
                }
            }
        }

        private void addRoomProp(Transform tile)
        {
            var propIndex = Random.Range(0, roomPropPrefabs.Length);
            var prefab = roomPropPrefabs[propIndex];
            var position = tile.position;
            var instance = Instantiate(prefab, position, Quaternion.identity);
            instance.transform.parent = tile;
            instance.name = prefab.name;
        }
    }
}