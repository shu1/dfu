/* TouchDebugPoint.cs
 * Copyright Grasshopper 2012
 * ----------------------------
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchDebugPoint : BehaviourBase 
{
    TouchInfo touch;
    GUIText text;

    protected override void Awake()
    {
        base.Awake();

        text = GetComponentInChildren<GUIText>();
    }

    public void Init( TouchInfo touch )
    {
        this.touch = touch;
    }

    protected override void Update()
    {
        base.Update( );

        if ( touch != null )
        {
            if ( touch.isDown )
            {
                transform.position = new Vector3( touch.screenPos.x / Screen.width, touch.screenPos.y / Screen.height, 0 );

                text.text = string.Format( "touch: {0}\ntime: {1}\nx: {2} y: {3}", 
                                          touch.touchID.ToString( "d2" ), touch.timeHeld.ToString( "f1" ), transform.position.x.ToString( "f2" ), transform.position.y.ToString( "f2" ) );
            }
            else
                Destroy( gameObject );
        }
    }
}
