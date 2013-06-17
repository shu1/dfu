/* DefaultGameEnv.cs
 * Copyright Grasshopper 2012
 * ----------------------------
 * Default, minimum implimentation of GhoperEnv, to be dropped into scene
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DefaultGameEnv : GhopperEnv 
{
    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    public override void OnReady()
    {
        Show();
    }
}
