using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer {
    public class playerMove : MonoBehaviour
    {
        [Header("Player Settings"), SerializeField, Min(float.Epsilon)] private float playerMoveSpeed;
        [SerializeField] private float teleDist;
        [SerializeField] private float slowDist;
        private float orgPlMvSp;
        
        [Header("Live Things"), SerializeField] private Vector2 mousePosition;
        [SerializeField] private Vector2 direction;
        [SerializeField] private Camera playerCam;
        [SerializeField] private float dist;
        [SerializeField] private Transform playerTrans;
        [SerializeField] private Rigidbody2D rb;
        // Start is called before the first frame update
        void OnEnable()
        {
           playerTrans = GetComponent<Transform>();
           rb = GetComponent<Rigidbody2D>();
           playerCam = Camera.main;
           orgPlMvSp = playerMoveSpeed;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButton("Fire1"))
            {
                mousePosition = playerCam.ScreenToWorldPoint(Input.mousePosition);
                Vector2 temp = playerTrans.position;
                direction = (mousePosition - temp).normalized;
                rb.velocity = new Vector2(direction.x * playerMoveSpeed, direction.y * playerMoveSpeed);
                dist = Vector2.Distance(mousePosition, playerTrans.position);
                if (dist < teleDist)
                {
                    playerTrans.position = mousePosition;
                }
                else if (dist < slowDist)
                {
                    playerMoveSpeed = orgPlMvSp/2;
                } else {
                    playerMoveSpeed = orgPlMvSp;
                }
            } else {
                rb.velocity = Vector2.zero; 
            }
        }
    }
}
