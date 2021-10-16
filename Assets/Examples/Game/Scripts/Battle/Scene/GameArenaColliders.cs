using Prg.Scripts.Common.Unity;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Scene
{
    /// <summary>
    /// Creates reversed box collider around given template <c>Sprite</c> that provides the area to be "boxed" by colliders.
    /// </summary>
    /// <remarks>
    /// Wall collider parent, "wall" thickness, tag and layer are configurable.
    /// </remarks>
    public class GameArenaColliders : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private SpriteRenderer templateSprite;
        [SerializeField] private Transform colliderParent;
        [SerializeField] private float wallThickness;
        [SerializeField, TagSelector] private string wallTopTag;
        [SerializeField, LayerSelector] private int wallTopLayer;
        [SerializeField, TagSelector] private string wallBottomTag;
        [SerializeField, LayerSelector] private int wallBottomLayer;
        [SerializeField, TagSelector] private string wallLeftTag;
        [SerializeField, LayerSelector] private int wallLeftLayer;
        [SerializeField, TagSelector] private string wallRightTag;
        [SerializeField, LayerSelector] private int wallRightLayer;

        [Header("Live Data"), SerializeField] private BoxCollider2D wallTop;
        [SerializeField] private BoxCollider2D wallBottom;
        [SerializeField] private BoxCollider2D wallLeft;
        [SerializeField] private BoxCollider2D wallRight;

        private void Awake()
        {
            makeWalls();
        }

        private void makeWalls()
        {
            wallTop = createWall("wallTop", colliderParent,  wallTopTag, wallTopLayer).GetComponent<BoxCollider2D>();
            wallBottom = createWall("wallBottom", colliderParent, wallBottomTag, wallBottomLayer).GetComponent<BoxCollider2D>();
            wallLeft = createWall("wallLeft", colliderParent, wallLeftTag, wallLeftLayer).GetComponent<BoxCollider2D>();
            wallRight = createWall("wallRight", colliderParent, wallRightTag, wallRightLayer).GetComponent<BoxCollider2D>();

            if (wallThickness == 0)
            {
                throw new UnityException("wall thickness can not be zero");
            }
            var size = templateSprite.size;
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

        private static GameObject createWall(string name, Transform parent, string tag, int layer)
        {
            var wall = new GameObject(name) { isStatic = true };
            wall.transform.SetParent(parent);
            if (!string.IsNullOrEmpty(tag))
            {
                wall.tag = tag;
            }
            wall.layer = layer;
            wall.isStatic = true;
            wall.AddComponent<BoxCollider2D>();
            return wall;
        }
    }
}