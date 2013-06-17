/* SetRes.cs
 * Copyright Grasshopper 2012
 * ----------------------------
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SetRes : BehaviourBase 
{
#if UNITY_ANDROID || UNITY_IPHONE
    protected override void Start()
    {
        base.Start();

        transform.localScale = new Vector3( ( 4f / 3f ) / ( (float)Screen.width / Screen.height ), 1f, 1f );
    }
#endif
}
