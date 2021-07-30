using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    public static EventManager current;

    private void Awake( )
    {
        current = this;
    }

    public event Action<int> OnBerryCollected;

    public event Action OnTargetWasHit;

    public void BerryCollected( int treeID )
    {
        if( OnBerryCollected != null )
        {
            OnBerryCollected( treeID );
        }
    }

    public void TargetWasHit( )
    {
        if ( OnTargetWasHit != null )
        {
            OnTargetWasHit( );
        }
    }

}
