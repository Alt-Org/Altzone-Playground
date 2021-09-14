using UnityEngine;

namespace Altzone.Nelinpeli
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Korostaja : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Korotus(bool tila)
        {
            spriteRenderer.enabled = tila;
        }
    }
}
