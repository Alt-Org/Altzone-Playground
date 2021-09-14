using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class ScoreSlave : MonoBehaviour
    {
        [Header("Live Data")] public StoneScriptAdder master;
        public int layer;
        [SerializeField] private bool hit;
        public int player;

        private void Awake() {
            hit = true;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == layer && hit == true)
            {
                master.HPCount(player);
                hit = false;
                StartCoroutine(ScoreAndWait());
            }
        }

        private IEnumerator ScoreAndWait()
        {  
                yield return new WaitForSeconds(0.5f);
                hit = true;
        }
    }
}