/* InputProvider.cs
 * Copyright Grasshopper 2013
 * ----------------------------
 * Base class for parsing input direct from a source when requested
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class InputProvider : BehaviourBase 
{
	public virtual int maxSupportedInputs{ get { return 1; } }

    public abstract void FetchFrameInputs();

    public abstract InputInfo GetInputState( int touchID );

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad( gameObject );
    }

    public struct InputInfo
    {
        public bool isDown;
        public Vector2 screenPos;
        public float width, height, angle;

        public InputInfo( bool isDown, Vector2 screenPos )
        {
            this.isDown = isDown;
            this.screenPos = screenPos;

            width = height = angle = 0;
        }

        public InputInfo( bool isDown, Vector2 screenPos, float width, float height, float angle )
        {
            this.isDown = isDown;
            this.screenPos = screenPos;
            this.width = width;
            this.height = height;
            this.angle = angle;
        }
    }
}
