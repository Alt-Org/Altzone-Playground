using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;


namespace Altzone.NewPlayer 
{
    public class playerManager : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields


        // A number of settings to keep track of how the player ought to move.
        // Speed at which the player moves.
        [Header("Player Move Settings"), SerializeField] private float playerMoveSpeed;
        // Distance from mouse at which the player teleports to the mouse position to avoid constant overshooting. Should be short.
        [SerializeField] private float teleDist;
        // Distance from mouse at which the player moves at half speed to avoid constant overshooting. Should be short, but not as short as teleDist.
        [SerializeField] private float slowDist;

        // A number of settings relating to the catching and launching of the ball when it hits the players head.
        // A transform component that's the location where the ball ought to be held.
        [Header("Ball Launch Settings"), SerializeField] private Transform ballHoldPosition;

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
        // Teammates transform so we can get the direction and distance for ballLauncher();
        [SerializeField] private Transform teamMateTrans;
        // Players rigidbody on which we can apply movement.
        [SerializeField] private Rigidbody2D rb;
        // An array of the sprites set in Player Squash Settings for easy store and access.
        [SerializeField] private Sprite[] sprites;

        #endregion


        #region Private Unserialized Fields

        // What direction player is going in. TBH, I dunno how this works. - Joni
        private Vector2 direction;
        // Players original movement speed.
        private float orgPlMvSp;
        // A float that is used to count down time.
        private float targetTime = 0.0f;

        #endregion


        #region Public Fields

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #endregion


        #region Public Methods


        /// <summary>
        /// A function that changes the players sprite as it gets squished between the shields.
        /// </summary>
        public void squishPlayer(int health)
        {            
            head.sprite = sprites[health];
            headcollider.radius = radi[health];
        }

        
        /// <summary>
        /// A function that stops the player and changes its head color for a certain time.
        /// Source = 1 is a ball hitting the shield at 0 health (shieldManager).
        /// Source = 2 is the ball being on the players side (playerSideStopper).
        /// </summary>
        public void playerStop(int source) {
            // If a 0hp shield has been hit.
            if(source == 1)
            {
                targetTime = 1.0f;
            }
            // If ball is on the players side and it isn't already stunned by something more long lasting.
            else if((source == 2) && (targetTime <= 0.5f)) 
            {
                targetTime = 0.5f;
            }
            // If something needs to allow the players to move.
            else if (source == 0)
            {
                targetTime = 0.0f;
            }
        }
        
        #endregion


        #region Monobehaviour Callbacks

        void Start()
        {
            playerTrans = GetComponent<Transform>();
            rb = GetComponent<Rigidbody2D>();
            playerCam = Camera.main;
            orgPlMvSp = playerMoveSpeed;
            sprites = new Sprite[]{hp0psrite, hp1psrite, hp2psrite, hp3psrite, hp4psrite};

            // #Important
            // used in gameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {
                playerManager.LocalPlayerInstance = this.gameObject;
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);

            
            // Lastly, setting up which 'Player-#' tag this one should get and setting up the player correctly according to it.
            for (int i = 1; i < 5; i++)
            {
                // For loop goes through numbers 1-4 as 'I' and checks if there is an item with the tag 'Pelaaja-i'
                var playercheck = GameObject.FindWithTag($"Pelaaja-{i}");
                
                // If we found no object with this tag, it means there is no player of that number.
                if(playercheck is null)
                {
                    // Setting this players tag to be the missing player.
                    this.gameObject.tag = $"Pelaaja-{i}";

                    // Finding the game object with the tag 'PelaajaAlue-i' which there is expected to be only one of, that being the relevant players spawn.
                    // Then we set the players position to the spawn position.
                    // for some reason, the player is first set correctly, but then gets moved to spawn 1s position.
                    this.transform.position = GameObject.FindGameObjectWithTag($"PelaajaAlue-{i}").transform.position;

                    // Flipping players 1 and 3 to face up.
                    if(i == 1 || i == 3)
                    {
                        this.transform.Rotate(0,0,180,Space.World);
                    }

                    return;
                }
            }
        }
        
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity once every frame.
        /// </summary>
        void Update()
        {
            // We do a skip if this isn't our photonView and the network is connected.
            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            {
                return;
            }

            //Counting down the player stopping & carry timer.
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

                // Getting a direction for the player to move toward and applying velocity.
                direction = (mousePosition - temp).normalized;
                rb.velocity = new Vector2(direction.x * playerMoveSpeed, direction.y * playerMoveSpeed);

                //Acquiring the distance between player and mouse for the following 'if, else if' chain.
                dist = Vector2.Distance(mousePosition, playerTrans.position);

                // Stopping the player if there is targetTime left
                if (targetTime > 0.0f)
                {
                    playerMoveSpeed = 0;
                } 
                // Teleporting player if distance is less then teleport Distance. This helps remove jitter from constantly overshooting the target.
                else if (dist < teleDist)
                {
                    playerTrans.position = mousePosition;
                }
                // Slowing down the player if his distance is less than the slowdown distance. Accomplishes much the same as teleporting.
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

        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(head.color);
            }
            else
            {
                // Network player, receive data
                this.head.color = (Color)stream.ReceiveNext();
            }
        }

        #endregion

    }
    
}
