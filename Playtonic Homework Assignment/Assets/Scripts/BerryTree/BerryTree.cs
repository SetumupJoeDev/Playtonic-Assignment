using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryTree : MonoBehaviour
{

    [Header("Berries")]

    [Tooltip("The array containing the berries growing on the tree.")]
    public GameObject[] m_berryArray;

    [Tooltip("The time delay between a berry being collected and it regrowing.")]
    public float m_berryRegrowDelay;

    [Tooltip("The unique identifier for this tree.")]
    public int m_treeID;

    private void Start( )
    {
        //Subscribes the method for checking this trees berries to the event listener's OnBerryCollected event
        EventManager.current.OnBerryCollected += CheckForCollectedBerries;

        foreach ( GameObject berry in m_berryArray )
        {
            //Assigns the script attached to the current berry for later use
            Berry berryController = berry.GetComponent<Berry>();

            //Sets the current berry's parent tree ID as this tree's ID
            berryController.m_parentTreeID = m_treeID;
        }

    }

    public virtual void CheckForCollectedBerries( int treeID )
    {
        //If the tree the berry was collected from has the same ID as this tree, this tree checks its berries
        if ( treeID == m_treeID )
        {
            //Loops through each berry attached to this tree and checks if they are active
            foreach ( GameObject berry in m_berryArray )
            {
                //Saves the berry's attached script for later use
                Berry berryController = berry.GetComponent<Berry>();

                //If the current berry is cannot currently be collected, it has been collected, and so it must be regrown
                if ( !berryController.m_canBeCollected && !berryController.m_isGrowing )
                {
                    //Starts a coroutine to regrow the berry with a short time delay
                    StartCoroutine( WaitToRegrowBerry( berry ) );

                }
            }
        }
    }

    public virtual IEnumerator WaitToRegrowBerry( GameObject berryToGrow )
    {

        //Waits for the duration of the berry regrowth delay before re-enabling the berry
        yield return new WaitForSeconds( m_berryRegrowDelay );

        //Sets the berry as active again so it can be seen and animated
        berryToGrow.SetActive( true );

        //Plays the berry's regrowth animation
        berryToGrow.GetComponent<Animator>( ).SetTrigger( "GrowBerry" );

    }

}
