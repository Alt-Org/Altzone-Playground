using UnityEngine;

namespace Examples.Game.Scripts
{
    public class PlayerColor : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color disabledColor;
        [SerializeField] private SpriteRenderer _highLightSprite;

        public void setNormalColor()
        {
            _sprite.color = normalColor;
        }

        public void setDisabledColor()
        {
            _sprite.color = disabledColor;
        }

        public void setHighLightColor(Color color)
        {
            if (color.a == 0)
            {
                color.a = 1;
            }
            _highLightSprite.color = color;
        }
    }
}