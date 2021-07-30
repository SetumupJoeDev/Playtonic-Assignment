using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    #region Camera Control Variables

    [Header("Camera Controls")]

    [Tooltip("The speed at which the camera rotates around the player.")]
    public float m_cameraRotationSpeed;

    [Tooltip("The value by which the camera's movement will be smoothed using a Slerp method.")]
    public float m_cameraSmoothingValue;

    [Tooltip("The transform of the game's player character that this camera will look at.")]
    public Transform m_cameraFocusTransform;

    //The current value of the input being applied by the player along the X axis
    private float m_yAxisInput;

    //The distance between the player character and the camera this script is attached to
    private Vector3 m_offsetFromPlayer;

    //The current saved rotation of the player's camera
    private Quaternion m_cameraAngle;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //Calculates the initial offset the camera has from its focal point
        m_offsetFromPlayer = transform.position - m_cameraFocusTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        CameraControls( );
    }

    public void CameraControls( )
    {

        //Reads the player's current input along the Y axis of the mouse or right joystick on the gamepad and assigns it
        m_yAxisInput = Input.GetAxis( "Mouse Y" );

        //Calculates the angle of rotation the camera should have, based on the player's input and the camera's rotation speed
        m_cameraAngle = Quaternion.AngleAxis( m_yAxisInput * m_cameraRotationSpeed, Vector3.right );

        //Recalculates the camera's offset from its focal point using the newly calculated angle above
        m_offsetFromPlayer = m_cameraAngle * m_offsetFromPlayer;

        //Calculates the position that the camera needs to move to using its focal point's current position and the newly calculated offset above
        Vector3 targetCameraPos = m_cameraFocusTransform.position + m_offsetFromPlayer;

        //Uses a spherical linear interpolation to smoothly move the camera into its new positon over time, instead of an instant snap
        transform.position = Vector3.Slerp( transform.position , targetCameraPos , m_cameraSmoothingValue );

        //Rotates the camera to focus on it's focal point's current position
        transform.LookAt( m_cameraFocusTransform );

    }

}
