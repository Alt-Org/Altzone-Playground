using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Nelinpeli
{
    public class StoneColliderControl : MonoBehaviour
    {
        [Header("Live Data")] public StoneScriptAdder master;
        public int layer;
        [SerializeField] private GameObject thisthing;

        void Start()
        {
            thisthing = this.gameObject;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.gameObject.layer == layer)
            {
                thisthing.SetActive(false);
            }
        }        
    }
}
