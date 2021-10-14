using UnityEngine;

namespace Examples.Game.Scripts.PlayerPrefab
{
    /// <summary>
    /// Manages some player colors during a game.
    /// </summary>
    public class PlayerColor : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private SpriteRenderer headSprite;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color disabledColor;
        [SerializeField] private SpriteRenderer _highLightSprite;

        public void setNormalColor()
        {
            headSprite.color = normalColor;
        }

        public void setDisabledColor()
        {
            headSprite.color = disabledColor;
        }

        public void setHighLightColor(Color color)
        {
            if (color.a == 0)
            {
                Debug.LogWarning("setHighLightColor is fully transparent, alpha is zero: " + gameObject.name);
            }
            _highLightSprite.color = color;
        }
    }
}