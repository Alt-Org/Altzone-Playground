using UnityEngine;

namespace Lobby.Scripts.Game
{
    public class CreateBoxCollider : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private SpriteRenderer areaTemplate;

        [Header("Live Data"), SerializeField] private BoxCollider2D wallTop;
        [SerializeField] private BoxCollider2D wallBottom;
        [SerializeField] private BoxCollider2D wallLeft;
        [SerializeField] private BoxCollider2D wallRight;
        [SerializeField] private float wallThickness;

        private void Awake()
        {
            makeWalls();
        }

        private void makeWalls()
        {
            var _transform = transform;
            wallTop = createWall("wallTop", _transform).GetComponent<BoxCollider2D>();
            wallBottom = createWall("wallBottom", _transform).GetComponent<BoxCollider2D>();
            wallLeft = createWall("wallLeft", _transform).GetComponent<BoxCollider2D>();
            wallRight = createWall("wallRight", _transform).GetComponent<BoxCollider2D>();

            var size = areaTemplate.size;
            var width = size.x / 2f;
            var height = size.y / 2f;
            var wallAdjustment = wallThickness / 2f;

            wallTop.offset = new Vector2(0f, height + wallAdjustment);
            wallTop.size = new Vector2(size.x, wallThickness);

            wallBottom.offset = new Vector2(0f, -height - wallAdjustment);
            wallBottom.size = new Vector2(size.x, wallThickness);

            wallLeft.offset = new Vector2(-width - wallAdjustment, 0f);
            wallLeft.size = new Vector2(wallThickness, size.y);

            wallRight.offset = new Vector2(width + wallAdjustment, 0f);
            wallRight.size = new Vector2(wallThickness, size.y);
        }

        private static GameObject createWall(string name, Transform parent)
        {
            var wall = new GameObject(name) { isStatic = true };
            wall.transform.SetParent(parent);
            wall.isStatic = true;
            wall.AddComponent<BoxCollider2D>();
            return wall;
        }
    }
}