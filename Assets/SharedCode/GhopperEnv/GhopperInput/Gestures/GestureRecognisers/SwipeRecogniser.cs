/* SwipeRecogniser.cs
 * Copyright Grasshopper 2013 (adapted with permission from FingerGestures copyright William Ravaine)
 * ----------------------------
 * A gesture of a swipe
 * Discrete (sent swipe has moved a given distance
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwipeGestureInfo : DiscreteGestureInfo
{
    /// <summary>
    /// Total swipe vector
    /// </summary>
    public Vector2 moved { get { return position - startPosition; } }

    /// <summary>
    /// Gets the frame velocity
    /// </summary>
    public float velocity { get; private set; }

    /// <summary>
    /// Current direction of swipe
    /// </summary>
    public SwipeDirection direction { get; private set; }

    /// <summary>
    /// How far (in degrees), swipe has moved since start angle
    /// </summary>
    /// <value>The deviation.</value>
    public float deviation { get; private set; }
 
    Vector2 lastPos;
    float lastAngle;
    ISwipeable[] startSwipeable;

    public override void Init( TouchGestureRecogniserBase recogniser, TouchGroup touches, int clusterID )
    {
        base.Init(recogniser, touches, clusterID);

        lastPos = position;
        direction = SwipeDirection.None;
        deviation = 0;

        if ( startedOver )
            startSwipeable = startedOver.GetComponentsWithInterface<ISwipeable>();
        else
            startSwipeable = new ISwipeable[0];
    }

    public override void Update(TouchGroup usingTouches)
    {
        lastPos = position;
        base.Update(usingTouches);

        velocity = Mathf.Lerp( velocity, ( position - lastPos ).magnitude / Time.deltaTime, Time.deltaTime * 10f );

        float newAngle = Mathf.Atan2( moved.y, moved.x );
        deviation += newAngle - lastAngle;
        lastAngle = newAngle;

        direction = SwipeRecogniser.GetSwipeDirection( moved, 1f );
    }

    protected override TouchInfo GetOriginTouch()
    {
        Vector2 avgScreenPos = _touches.GetAvgScreenPos();
        return new TouchInfo( -1, touchCount == 0 ? new RaycasterInfo() : _touches[0].raycastInfo, avgScreenPos, 0, 0, 0 );
    }

    public override void NotifyComplete()
    {
        foreach ( var swipeable in startSwipeable )
            swipeable.CompleteSwipe( this );
    }
    
    public override void NotifyFailed()
    {
        foreach ( var swipeable in startSwipeable )
            swipeable.FailSwipe( this );
    }
}

/// <summary>
/// ISwipeable objects will have these methods called
/// </summary>
public interface ISwipeable
{
    void FailSwipe( SwipeGestureInfo swipeInfo );   // When swipe moves too slow or too far before finger(s) are lifted
    void CompleteSwipe( SwipeGestureInfo swipeInfo ); // When swipe ends after having moved a given distance
}

/// <summary>
/// Swipe Gesture Recognizer
///   A quick drag motion and release in a single direction
/// </summary>
public class SwipeRecogniser : DiscreteGestureRecogniser<SwipeGestureInfo>
{
#if UNITY_IOS
	Dictionary<int, SwipeGestureInfo> dummy = new Dictionary<int, SwipeGestureInfo>( new IntComparer() );
#endif

    /// <summary>
    /// Directions to restrict the swipe gesture to
    /// </summary>
    public SwipeDirection validDirections = SwipeDirection.All;  //FIXME: public

    /// <summary>
    /// Minimum distance the finger must travel in order to produce a valid swipe in screen space
    /// </summary>
    [Range( 0, 1 )]
    public float minDistance = 0.01f;

    /// <summary>
    /// Finger travel distance beyond which the swipe recognition will fail. In screen space
    /// Setting this to 0 disables the MaxDistance constraint
    /// </summary>
    [Range( 0, 1 )]
    public float maxDistance = 0;

    /// <summary>
    /// Minimum speed the finger motion must maintain in order to generate a valid swipe
    /// </summary>
    public float minVelocity = 0.1f;

    /// <summary>
    /// Amount of tolerance used when determining whether or not the current swipe motion is still moving in a valid direction
    /// The maximum angle, in degrees, that the current swipe direction is allowed to deviate from its initial direction
    /// </summary>
    public float maxDeviation = 25.0f; // degrees

    public static event TouchGestureEvent<SwipeGestureInfo> SwipeCompleted;
    public static event TouchGestureEvent<SwipeGestureInfo> SwipeFailed;

    protected override TouchGestureEvent<SwipeGestureInfo> completedGestureEvent { get { return SwipeCompleted; } }
    protected override TouchGestureEvent<SwipeGestureInfo> failedGestureEvent { get { return SwipeFailed; } }

    protected override bool CanBegin( SwipeGestureInfo gesture )
    {
        if( !base.CanBegin( gesture ) )
            return false;

        TouchGroup touches = gesture.GetTouches();
        if( touches.GetAvgDistFromStart().sqrMagnitude < 1f )
            return false;

        // all touches must be moving
        if( !touches.AllMoving() )
            return false;

        // if multiple touches, make sure they're all going in roughly the same direction
        if( !touches.AreMovingInSameDirection( 0.35f ) )
            return false;

        return true;
    }    

    protected override void OnBegin(SwipeGestureInfo gesture)
    {

    }

    protected override GestureState OnRecognise( SwipeGestureInfo gesture )
    {
        if( gesture.touchCount != requiredFingerCount )
        {
            // more fingers were added - fail right away
            if( gesture.touchCount > requiredFingerCount )
                return GestureState.Failed;

            //
            // fingers were lifted-off
            //

            // didn't swipe far enough
            if( ( gesture.moved ).magnitude * InputMaster.pixelToScreenRatio < minDistance )
                return GestureState.Failed;

            // get approx swipe direction
            return GestureState.Ended;
        }
        
        float distance = ( gesture.moved ).magnitude * InputMaster.pixelToScreenRatio;

        // moved too far
        if( maxDistance > minDistance && distance > maxDistance )
        {
            //DebugExtras.LogWarning( "Too far: " + distance );
            return GestureState.Failed;
        }
        
        // we're going too slow
        if( distance > minDistance && gesture.velocity * InputMaster.pixelToScreenRatio < minVelocity )
        {
            DebugExtras.LogWarning( "Too slow: " + gesture.velocity * InputMaster.pixelToScreenRatio );
            return GestureState.Failed;
        }
        
        /*
        FingerGestures.SwipeDirection newDirection = FingerGestures.GetSwipeDirection( Move.normalized, DirectionTolerance );

        // we went in a bad direction
        if( !IsValidDirection( newDirection ) || ( direction != FingerGestures.SwipeDirection.None && newDirection != direction ) )
            return GestureState.Failed;

        direction = newDirection;
        */

        // check if we have deviated too much from our initial direction
        if( distance > minDistance * 2f )
        {
            // accumulate delta angle

            if( Mathf.Abs( gesture.deviation ) > maxDeviation )
            {
                //DebugExtras.LogWarning( "Swipe: deviated too much from initial direction (" + gesture.Deviation + ")" );
                return GestureState.Failed;
            }
        }

        return GestureState.InProgress;
    }

    /// <summary>
    /// Return true if the input direction is supported
    /// </summary>
    public bool IsValidDirection( SwipeDirection dir )
    {
        if( dir == SwipeDirection.None )
            return false;

        return ( ( validDirections & dir ) == dir );
    }

    /// <summary>
    /// Extract a swipe direction from a direction vector and a tolerance percent 
    /// </summary>
    /// <param name="dir">A normalized swipe motion vector (must be normalized!)</param>
    /// <param name="tolerance">Percentage of tolerance (0..1)</param>
    /// <returns>The constrained swipe direction identifier</returns>
    public static SwipeDirection GetSwipeDirection( Vector2 dir, float tolerance )
    {
        // max +/- 22.5 degrees around reference angle
        float maxAngleDelta = Mathf.Max( Mathf.Clamp01( tolerance ) * 22.5f, 0.0001f );
        
        // get the angle formed by the dir vector (0 = right, 90 = up, 180 = left, 270 = down)
        float angle = Mathf.Rad2Deg * Mathf.Atan2( dir.y, dir.x );
        while ( angle < 0 ) angle += 360f;
        while ( angle > 360f - 22.5f ) angle -= 360f;
        
        for( int i = 0; i < 8; ++i )
        {
            float refAngle = 45.0f * i;
            
            if( angle <= refAngle + 22.5f )
            {
                float minAngle = refAngle - maxAngleDelta;
                float maxAngle = refAngle + maxAngleDelta;
                
                if( angle >= minAngle && angle <= maxAngle )
                    return AngleToDirectionMap[i];
                
                break;
            }
        }
        
        // not a valid direction / not within tolerance zone
        return SwipeDirection.None;
    }

    static readonly SwipeDirection[] AngleToDirectionMap = new SwipeDirection[]
    {
        SwipeDirection.Right,               // 0
        SwipeDirection.UpperRightDiagonal,  // 45
        SwipeDirection.Up,                  // 90
        SwipeDirection.UpperLeftDiagonal,   // 135
        SwipeDirection.Left,                // 180
        SwipeDirection.LowerLeftDiagonal,   // 225
        SwipeDirection.Down,                // 270
        SwipeDirection.LowerRightDiagonal,  // 315
    };
}

[System.Flags]
public enum SwipeDirection
{
    /// <summary>
    /// Moved to the right
    /// </summary>
    Right = 1 << 0,
    
    /// <summary>
    /// Moved to the left
    /// </summary>
    Left = 1 << 1,
    
    /// <summary>
    /// Moved up
    /// </summary>
    Up = 1 << 2,
    
    /// <summary>
    /// Moved down
    /// </summary>
    Down = 1 << 3,
    
    /// <summary>
    /// North-East diagonal
    /// </summary>
    UpperLeftDiagonal = 1 << 4,
    
    /// <summary>
    /// North-West diagonal
    /// </summary>
    UpperRightDiagonal = 1 << 5,
    
    /// <summary>
    /// South-East diagonal
    /// </summary>
    LowerRightDiagonal = 1 << 6,
    
    /// <summary>
    /// South-West diagonal
    /// </summary>
    LowerLeftDiagonal = 1 << 7,
    
    //--------------------
    
    None = 0,
    Vertical = Up | Down,
    Horizontal = Right | Left,
    Cross = Vertical | Horizontal,
    UpperDiagonals = UpperLeftDiagonal | UpperRightDiagonal,
    LowerDiagonals = LowerLeftDiagonal | LowerRightDiagonal,
    Diagonals = UpperDiagonals | LowerDiagonals,
    All = Cross | Diagonals,
}