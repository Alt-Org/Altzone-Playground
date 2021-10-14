using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Examples.Game.Scripts.PlayerPrefab
{
    /// <summary>
    /// Manages some debug player colors during a game over network.
    /// </summary>
    public class PlayerColor : MonoBehaviour
    {
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase + 6;

        [Header("Settings"), SerializeField] private SpriteRenderer headSprite;
        [SerializeField] private Color normalColor; // #0
        [SerializeField] private Color disabledColor; // #1
        [SerializeField] private Color ghostColor; // #2
        [SerializeField] private SpriteRenderer _highLightSprite;

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, data => receiveColorIndex(data.CustomData));
        }

        private void receiveColorIndex(object data)
        {
            var colorIndex = (int)data;
            switch (colorIndex)
            {
                case 0:
                    _setNormalColor();
                    break;
                case 1:
                    _setDisabledColor();
                    break;
                case 2:
                    _setGhostColor();
                    break;
                default:
                    throw new UnityException($"invalid color index: {colorIndex}");
            }
        }

        private void sendColorIndex(int index)
        {
            photonEventDispatcher.RaiseEvent(photonEventCode, index);
        }

        public void setNormalColor()
        {
            sendColorIndex(0);
            _setNormalColor();
        }

        public void setDisabledColor()
        {
            sendColorIndex(1);
            _setDisabledColor();
        }

        public void setGhostColor()
        {
            sendColorIndex(2);
            _setGhostColor();
        }

        private void _setNormalColor()
        {
            headSprite.color = normalColor;
        }

        private void _setDisabledColor()
        {
            headSprite.color = disabledColor;
        }

        private void _setGhostColor()
        {
            headSprite.color = ghostColor;
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