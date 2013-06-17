/* MobileTouchInputProvider.cs
 * Copyright Grasshopper 2013
 * ----------------------------
 * Takes generic Unity touch input and parses into more generic form
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MobileTouchInputProvider : InputProvider 
{
    public override int maxSupportedInputs {
        get {
            return 5;  
        }
    }
    
    public override void FetchFrameInputs()
    {
        
    }
    
    public override InputInfo GetInputState(int touchID)
    {
        if ( Input.touchCount <= touchID )
            return new InputInfo( false, Vector2.zero );

        Touch touch = Input.GetTouch( touchID );
        return new InputInfo( touch.phase != TouchPhase.Canceled && touch.phase != TouchPhase.Ended, touch.position );
    }
}
