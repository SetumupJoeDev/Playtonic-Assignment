using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berry : MonoBehaviour
{

    #region Collection Variables

    [Header("Collection")]

    [Tooltip("A boolean that determines whether or not this berry can currently be collected.")]
    public bool m_canBeCollected;

    [Tooltip("The audioclip that plays when the berry is collected.")]
    public AudioClip m_berryCollected;

    #endregion

    #region Animation

    [Tooltip("The animator belonging to this berry.")]
    public Animator m_berryAnimator;

    [Tooltip("Determines whether or not this berry is currently growing.")]
    public bool m_isGrowing;

    public int m_parentTreeID;

    #endregion

    public void ToggleCanBeCollected( )
    {
        m_canBeCollected = !m_canBeCollected;
    }

    public void ToggleIsGrowing( )
    {
        m_isGrowing = !m_isGrowing;
    }

    private void OnTriggerEnter( Collider other )
    {
        //If the other object was the player, the berry gets collected
        if(other.gameObject.tag == "Player" && m_canBeCollected )
        {
            //Plays an audio clip to give the player feedback and indicate that they have collected a berry
            AudioSource.PlayClipAtPoint( m_berryCollected, transform.position );

            //Toggles the boolean to false so that this berry cannot be collected again until the boolean is toggled back to true
            ToggleCanBeCollected( );

            EventManager.current.BerryCollected( m_parentTreeID );

            //Disables the Berry so it cannot be collected more than once before re-growing
            gameObject.SetActive( false );

        }
    }

}
