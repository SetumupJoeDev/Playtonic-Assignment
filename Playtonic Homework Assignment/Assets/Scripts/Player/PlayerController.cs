using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    #region Player Movement Variables

    [Header("Player Movement")]

    [Tooltip("The speed at which the player will move through the world.")]
    public float m_playerMoveSpeed;

    #region Jumping

    [Tooltip("The amount of force behind the player's jumps.")]
    public float m_playerJumpForce;

    [Tooltip("The transform from which a raycast will be used to determine if the player is on the ground.")]
    public Transform m_groundChecker;

    [Tooltip("The layer mask that the ground checking raycast will use.")]
    public LayerMask m_groundLayer;

    //A boolean that determines whether or not the player is currently on the ground
    private bool m_isGrounded;

    #endregion

    #region Gravity

    [Tooltip("The amount of gravitational force applied to the player.")]
    public float m_gravityForce;

    [Tooltip("A multiplier used to scale the effects of gravity on the player.")]
    public float m_gravityMultiplier;

    #endregion

    #region Input & Velocity

    //The value of the input being applied by the player along the X axis
    private float m_xAxisInput;

    //The value of the input being applied by the player along the Z axis
    private float m_zAxisInput;

    //The player's current vertical velocity, calculated when jumping or having gravity applied
    private Vector3 m_verticalVelocity;

    //The player's current horizontal velocity, calculated using the X and Z axis inputs
    private Vector3 m_horizontalVelocity;

    #endregion

    //The character controller component attached to this object, used for player movement
    private CharacterController m_characterController;

    #endregion

    #region Camera Control Variables

    private float m_cameraXInput;

    #endregion

    #region Berries

    [Header("Berries")]

    [Tooltip("The current number of berries held by the player")]
    public int m_berryCount;

    [Tooltip("The UI element that tracks and displays the player's current berry count.")]
    public TextMeshProUGUI m_berryCounter;

    [Tooltip("An array containing a pool of berries that the player can fire.")]
    public GameObject[] m_berryPool;

    [Tooltip("The position that berries will be thrown from.")]
    public Transform m_berryThrowPoint;

    [Tooltip("The sound that will play when the player hits the target.")]
    public AudioClip m_targetHitSound;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //Finds the CharacterController attached to the player and assigns it
        m_characterController = gameObject.GetComponent<CharacterController>( );

        //Subscribes the CollectBerry method to the OnBerryCollected event
        EventManager.current.OnBerryCollected += CollectBerry;

        //Subscribes the TargetHit method to the OnTargetWasHitEvent
        EventManager.current.OnTargetWasHit += TargetHit;

        //Locks the cursor to the center of the screen to ensure the player can't click outside of the game window
        Cursor.lockState = CursorLockMode.Locked;

        //Sets the cursor to be invisible so it doesn't block the game view
        Cursor.visible = false;

    }

    // Update is called once per frame
    public virtual void Update()
    {
        MovementControls( );

        CombatControls( );

        ApplyGravity( );

        CameraControls( );

        //If the player presses any button mapped to Cancel, the game closes
        if( Input.GetButtonDown("Cancel") )
        {
            Application.Quit( );
        }

    }

    public virtual void MovementControls( )
    {

        //Reads the values of the player's current input along both the X and Z axes and assigns them accordingly
        m_xAxisInput = Input.GetAxis( "Horizontal" );
        m_zAxisInput = Input.GetAxis( "Vertical" );

        //Calculates the player's movement velocity based on the above values multiplied by the respective directional vectors
        m_horizontalVelocity = transform.right * m_xAxisInput + transform.forward * m_zAxisInput;

        //Moves the player using the attached CharacterController's Move method
        //Passes in the player's current velocity multiplied by their move speed, using deltaTime to make movement framerate independant
        m_characterController.Move( m_horizontalVelocity * m_playerMoveSpeed * Time.deltaTime );

        //If the player is currently grounded and presses the Jump button, their vertical velocity is calculated and they are moved upwards
        if( Input.GetButtonDown( "Jump" ) && m_isGrounded )
        {
            //Calculates the player's vertical velocity, using the square root of their jump force multiplied by the inverse of the gravity multiplier, multiplied by the gravity force
            m_verticalVelocity.y = Mathf.Sqrt( m_playerJumpForce * -m_gravityMultiplier * m_gravityForce );
            //Moves the player using the CharacterController's Move method using the newly calculated velocity multiplied by the player's move speed and deltaTime
            m_characterController.Move( m_verticalVelocity * m_playerMoveSpeed * Time.deltaTime );
            //Sets this to false so that the player cannot jump multiple times in the air
            m_isGrounded = false;
        }

        //If the player is not grounded, a raycast is sent out to find the floor
        if( !m_isGrounded )
        {
            //If the raycast finds something on the ground layer, the player is set as grounded and they can jump again
            if(Physics.Raycast( m_groundChecker.position , Vector3.down , 0.25f,  m_groundLayer ) )
            {
                m_isGrounded = true;
            }
        }

    }

    public virtual void CombatControls( )
    {
        //When the player presses the button mapped to Fire1, the fire method is called
        if ( Input.GetButtonDown( "Fire1" ) )
        {
            Fire( );
        }
    }

    public virtual void CameraControls( )
    {

        //Reads the value of the player's current input along the Mouse X axis and assigns it
        m_cameraXInput = Input.GetAxis( "Mouse X" );

        //Rotates the player character around the Y axis using the input saved above
        transform.Rotate( Vector3.up * m_cameraXInput );

    }

    public virtual void Fire( )
    {
        //If the player has any berries, they can throw one
        if ( m_berryCount > 0 )
        {
            //Loops through the array of berries in the pool, checking if any are inactive
            foreach ( GameObject berry in m_berryPool )
            {
                //If it finds an inactive berry, that berry is set to active and placed at the throw positon
                if ( !berry.activeSelf )
                {

                    //Activates the berry so that it is visible in the scene
                    berry.SetActive( true );

                    //Places the berry at the throw point, so they launch as though the player has thrown them
                    berry.transform.position = m_berryThrowPoint.position;

                    //Sets the transform parent as null so that it does not move around with the player
                    berry.transform.SetParent( null );

                    //Calls the launch method in the berry projectile script to calculate its flight path
                    berry.GetComponent<BerryProjectile>( ).Launch( );

                    //Reduces the player's berry count by one
                    m_berryCount--;

                    //Updates the player's berry counter UI to reflect the new value
                    m_berryCounter.text = m_berryCount.ToString( );

                    //Returns out of the loop so that only one berry is fired at a time
                    return;
                }
            }
        }
    }

    public virtual void ApplyGravity( )
    {

        //Adds the value of the player's gravity force to their current movement Y velocity to simulate gravity
        m_verticalVelocity.y += m_gravityForce * Time.deltaTime;

        //Moves the player using the CharacterController's movement method
        //Passes in the player's current velocity multiplied by deltaTime to make gravity simulation framerate independant
        m_characterController.Move( m_verticalVelocity * m_gravityMultiplier * Time.deltaTime );

    }

    private void CollectBerry( int treeID )
    {
        
        //Increments the value of the player's berry count by one to reflect the newly collected berry
        m_berryCount++;

        //Updates the value of the berry counter's text to reflect the new berry count
        m_berryCounter.text = m_berryCount.ToString( );

    }

    private void TargetHit( )
    {
        //Plays a sound at the player's position to tell them they have hit the target
        AudioSource.PlayClipAtPoint( m_targetHitSound , transform.position );
    }

    private void OnControllerColliderHit( ControllerColliderHit hit )
    {

        //Finds the rigidbody attached to the object the character controller collided with and assigns it to a local variable
        Rigidbody body = hit.collider.attachedRigidbody;

        //If the object doesn't have a rigidbody, the method returns
        if( body == null || body.isKinematic )
        {
            return;
        }

        //Ignores any objects directly below the player
        if(hit.moveDirection.y < -0.03 )
        {
            return;
        }

        //Calculated the direction to push the object based on the current movement velocity, ignoring the Y
        Vector3 pushDirection = new Vector3( hit.moveDirection.x, 0, hit.moveDirection.z);

        //Pushes the object collided with in the direction calculated above, using the player's movespeed as the force
        body.velocity = pushDirection * m_playerMoveSpeed;

    }

}
