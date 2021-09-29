using UnityEngine;

namespace Examples.Game.Scripts
{
    public class PlayerColor : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color disabledColor;

        public void setNormalColor()
        {
            _sprite.color = normalColor;
        }

        public void setDisabledColor()
        {
            _sprite.color = disabledColor;
        }
    }
}