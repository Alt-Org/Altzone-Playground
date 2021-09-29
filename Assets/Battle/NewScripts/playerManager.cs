using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.NewPlayer 
{
    public class playerManager : MonoBehaviour
    {
        // Player movement settings.
        // Speed at which the player moves.
        [Header("Player Move Settings"), SerializeField, Min(float.Epsilon)] private float playerMoveSpeed;
        // Distance from mouse at which the player teleports to the mouse position to avoid constant overshooting. Should be short.
        [SerializeField] private float teleDist;
        // Distance from mouse at which the player moves at half speed to avoid constant overshooting. Should be short, but not as short as teleDist.
        [SerializeField] private float slowDist;

        // A number of settings for keeping track of the players head squishing as the shield loses HP.
        // Head is used to change out the sprite to one of the sprites below.
        // Headcollider is the collider of the head. radi is used to store an array of numbers to adjust the headcolliders radius.
        [Header("Player Squash Settings"), SerializeField] private SpriteRenderer head;
        [SerializeField] private Sprite hp4psrite;
        [SerializeField] private Sprite hp3psrite;
        [SerializeField] private Sprite hp2psrite;
        [SerializeField] private Sprite hp1psrite;
        [SerializeField] private Sprite hp0psrite;
        [SerializeField] private CircleCollider2D headcollider;
        [SerializeField] private float[] radi = new float[]{0.6f, 0.7f, 0.85f, 1.05f, 1.2f};

        // Things that aren't managed in the editor, but instead assigned by the script.
        // Where the mouse is pointed.        
        [Header("Live Things"), SerializeField] private Vector2 mousePosition;
        // The camera that looks at this player.
        [SerializeField] private Camera playerCam;
        // Distance between Player and mousePosition.
        [SerializeField] private float dist;
        // Player transform mostly used to get his position.
        [SerializeField] private Transform playerTrans;
        // Players rigidbody on which we can apply movement.
        [SerializeField] private Rigidbody2D rb;
        // An array of the sprites set in Player Squash Settings for easy store and access.
        [SerializeField] private Sprite[] sprites;
        
        // What direction player is going in. TBH, I dunno how this works.
        private Vector2 direction;
        // Players original movement speed.
        private float orgPlMvSp;
        // A float that is used to count down time.
        public float targetTime = 0.0f;

        // A function that changes the players sprite as it gets squished between the shields.
        public void squishPlayer(int health)
        {            
            head.sprite = sprites[health];
            headcollider.radius = radi[health];
        }

        // A function that stops the player and changes its head color for a certain time.
        // Source = 1 is a ball hitting the shield at 0 health.
        // Source = 2 is the ball being on the players side. (playerSideStopper)
        public void playerStop(int source) {
            if(source == 1)
            // If a 0hp shield has been hit.
            {
                targetTime = 1.0f;
            }
            // If ball is on the players side and it isn't already stunned by something more long lasting.
            else if((source == 2) && (targetTime <= 0.5f)) 
            {
                targetTime = 0.5f;
            }            
        }
        
        // Doing some prepping things as the player is enabled.
        void OnEnable()
        {
            playerTrans = GetComponent<Transform>();
            rb = GetComponent<Rigidbody2D>();
            playerCam = Camera.main;
            orgPlMvSp = playerMoveSpeed;
            sprites = new Sprite[]{hp0psrite, hp1psrite, hp2psrite, hp3psrite, hp4psrite};
        }

        // Update is called once per frame
        void Update()
        {
            //Counting down the player stopping timer.            
            targetTime -= Time.deltaTime;

            // Recoloring the players head to black if there is target time left, otherwise leave it at the green.
            if (targetTime > 0.0f)
            {                
                head.color = Color.black;
            }
            else
            { 
                head.color = new Color(0.0f, 0.5f, 0.0f, 1f);
            }

            // If the mouse is pressed down.
            if (Input.GetButton("Fire1"))
            {
                // Get mouse position from the camera and put it in a variable.
                mousePosition = playerCam.ScreenToWorldPoint(Input.mousePosition);
                Vector2 temp = playerTrans.position;
                // Getting a direction for the player to move toward and applying movement.
                direction = (mousePosition - temp).normalized;
                rb.velocity = new Vector2(direction.x * playerMoveSpeed, direction.y * playerMoveSpeed);
                //Acquiring the distance between player and mouse for the following 'if, esle if' chain.
                dist = Vector2.Distance(mousePosition, playerTrans.position);    

                // Stopping the player if there is targetTime left
                if (targetTime > 0.0f)
                {
                    playerMoveSpeed = 0;
                } 
                // Teleporting player if distance is less then teleport Distance.
                else if (dist < teleDist)
                {
                    playerTrans.position = mousePosition;
                }
                // Slowing down the player if his distance is less than the teleport distance.
                else if (dist < slowDist)
                {
                    playerMoveSpeed = orgPlMvSp/2;
                } 
                // Giving the player his usual movement speed.
                else {
                    playerMoveSpeed = orgPlMvSp;
                }
            } 
            // Stopping the player when mouse is not down.
            else {
                rb.velocity = Vector2.zero; 
            }
        }
    }
}
