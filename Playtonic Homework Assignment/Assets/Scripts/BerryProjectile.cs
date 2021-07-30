using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryProjectile : MonoBehaviour
{

    [Header("Projectile Flight")]

    [Tooltip("The speed at which the projectile flies.")]
    public float m_flightSpeed;

    [Tooltip("The transform of the target in the scene that this projectile will fly towards.")]
    public Transform m_targetTransform;

    [Tooltip("The angle at which this projectile is launched upward when fired.")]
    public float m_launchAngle = 45.0f;

    [Tooltip("The amount of gravitational force applied to this object while in flight.")]
    public float m_gravityForce = 9.81f;

    //The projectile's velocity in the X direction
    private float m_xVelocity;

    //The projectile's velocity in the Y direction
    private float m_yVelocity;

    //The amount of time that has elapsed since the projectile was launched
    private float m_flightTimeElapsed;

    //The total amount of time that the projectile will be in flight before it reaches the target
    private float m_totalFlightTime;

    //The parent object of this projectile while is is inactive and pooled
    private Transform m_poolingParent;

    //The velocity of the projectile while it is in flight
    private float m_flightVelocity;


    public virtual void Start( )
    {
        //Assigns the pooling parent as the projectile's current transform parent
        m_poolingParent = transform.parent;
        //Sets the projectile as inactive until it is fired
        gameObject.SetActive( false );
    }

    public void Launch( )
    {
        //Calculates the distance between the projectile and its target
        float distanceToTarget = Vector3.Distance(transform.position, m_targetTransform.position);

        //Calculates a velocity float using the value calculated above, the launch angle, and the force of gravity
        m_flightVelocity = distanceToTarget / ( Mathf.Sin( 2 * m_launchAngle * Mathf.Deg2Rad )  / m_gravityForce );

        //Extracts the X and Y values of the velocity calculated
        //Calculates the root of the velocity, multiplying it by the cosine of the launch angle in radians
        m_xVelocity = Mathf.Sqrt(m_flightVelocity) * Mathf.Cos(m_launchAngle * Mathf.Deg2Rad);
        //Calculates the root of the velocity, multiplying it by the sine of the launch angle in radians
        m_yVelocity = Mathf.Sqrt(m_flightVelocity) * Mathf.Sin(m_launchAngle * Mathf.Deg2Rad);

        //Calculates the amount of time it will take the projectile to reach its target by dividing the distance by the speed
        m_totalFlightTime = distanceToTarget / m_xVelocity;

        //Rotates the projectile to look at its target
        transform.rotation = Quaternion.LookRotation( m_targetTransform.position - transform.position );

        //Sets the elapsed flight time to 0 so that it can be tracked correctly
        m_flightTimeElapsed = 0.0f;

    }

    public virtual void FixedUpdate( )
    {
        FlyToTarget( );
    }

    public virtual void FlyToTarget( )
    {

        //If the projectile has not yet reached the end of its flight time, it continues moving along its parabola towards the target
        if( m_flightTimeElapsed < m_totalFlightTime )
        {

            //Moves the projectile along its flight path, using its y velocity, gravity and the amount of time elapsed to calculate where on the parabola it should be
            gameObject.transform.Translate( 0 , ( m_yVelocity - ( m_gravityForce * m_flightTimeElapsed ) ) * Time.deltaTime , m_xVelocity * Time.deltaTime);

            //Increases the value of the time elapsed by the value of deltaTime, the amount of time that has passed since the previous frame
            m_flightTimeElapsed += Time.deltaTime;

        }
        //Otherwise, if the flight time has been reached, the projectile checks to see whether or not it has hit the target
        else if( m_flightTimeElapsed >= m_totalFlightTime )
        {
            //If the projectile is within a close distance to the target when the flight time has elapsed, then the target has been hit
            if( Vector3.Distance(transform.position, m_targetTransform.position) < m_targetTransform.localScale.x )
            {
                TargetHit( );
            }
            //Otherwise, the projectile is simply disabled
            else
            {
                DisableProjectile( );
            }
        }

    }

    public virtual void TargetHit( )
    {
        //Calls the TargetWasHit event so any subscribed methods will be called
        EventManager.current.TargetWasHit( );

        //Disables the projectile
        DisableProjectile( );
    }

    public virtual void DisableProjectile( )
    {
        //Re-parents the projectile to the pooling parent to keep the heirarchy organised
        transform.SetParent( m_poolingParent );
        //Disables the projectile so it is no longer rendered in the scene
        gameObject.SetActive( false );
    }

}
