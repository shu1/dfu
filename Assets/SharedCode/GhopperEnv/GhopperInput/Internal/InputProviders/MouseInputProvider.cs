/* MouseInputProvider.cs
 * Copyright Grasshopper 2013
 * ----------------------------
 * Takes mouse input and parses into more generic form. Imitates multiple touches with different mouse buttons (in the same position, though)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseInputProvider : InputProvider 
{
    public override int maxSupportedInputs {
        get {
            return 3;  
        }
    }

    public override void FetchFrameInputs()
    {

    }

    public override InputInfo GetInputState(int touchID)
    {
        return new InputInfo( Input.GetMouseButton( touchID ), Input.mousePosition );
    }
}
