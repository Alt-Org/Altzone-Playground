using Photon.Pun;
using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class OnlinePlayerHighlight : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Live Data"), SerializeField] private PhotonView photonView;

        private void Awake()
        {
            spriteRenderer.enabled = false;
            photonView = PhotonView.Get(this);
        }

        private void OnEnable()
        {
            spriteRenderer.enabled = photonView.IsMine;
        }

        private void OnDisable()
        {
            spriteRenderer.enabled = false;
        }
    }
}