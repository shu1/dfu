/* TwistRecogniser.cs
 * Copyright Grasshopper 2013 (adapted with permission from FingerGestures copyright William Ravaine)
 * ----------------------------
 * A gesture of a twist. (Rotate gesture on iphone)
 * Continous (sent for moving fingers -> fingers lifted)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TwistGestureInfo : ContinuousGestureInfo
{
    public float deltaRotation { get; private set; }
    public float totalRotation { get; private set; }

    ITwistable[] twistables;

    public override void Init( TouchGestureRecogniserBase recogniser, TouchGroup touches, int clusterID )
    {
        base.Init( recogniser, touches, clusterID );

        if ( touchCount == 2 )
            deltaRotation = totalRotation = 0;

        if ( startedOver )
            twistables = startedOver.GetComponentsWithInterface<ITwistable>();
        else
            twistables = new ITwistable[0];
    }

    public override void Update( TouchGroup usingTouches )
    {
        base.Update( usingTouches );

        if ( touchCount == 2 )
            deltaRotation = TwistRecogniser.SignedAngularGap( _touches[0], _touches[1], _touches[0].screenPos - _touches[0].deltaScreenPos, _touches[1].screenPos - _touches[1].deltaScreenPos );
        else
            deltaRotation = 0;

        totalRotation += deltaRotation;        
    }
    
    public override void NotifyRecognised()
    {
        foreach ( var twistable in twistables )
            twistable.StartTwist( this );
    }

    public override void NotifyUpdate()
    {
        foreach ( var twistable in twistables )
            twistable.UpdateTwist( this );
    }

    public override void NotifyComplete()
    {
        foreach ( var twistable in twistables )
            twistable.EndTwist( this );
    }
}

public interface ITwistable
{
    void StartTwist( TwistGestureInfo info );   // fingers started moving around each other
    void UpdateTwist( TwistGestureInfo info );  
    void EndTwist( TwistGestureInfo info ); // fingers lifted/added
}

/// <summary>
/// Twist Gesture Recognizer (formerly known as rotation gesture)
///   Two fingers moving around a pivot point in circular/opposite directions
/// </summary>
public class TwistRecogniser : ContinuousGestureRecogniser<TwistGestureInfo>
{
#if UNITY_IOS
	Dictionary<int, TwistGestureInfo> dummy = new Dictionary<int, TwistGestureInfo>( new IntComparer() );
#endif
    /// <summary>
    /// Rotation DOT product treshold - this controls how tolerant the twist gesture detector is to the two fingers
    /// moving in opposite directions.
    /// Setting this to -1 means the fingers have to move in exactly opposite directions to each other.
    /// this value should be kept between -1 and 0 excluded.
    /// </summary>
    public float tolerance = -0.7f;

    /// <summary>
    /// Minimum amount of rotation required to start the rotation gesture (in degrees)
    /// </summary>
    public float minRotation = 1.0f;

    public static event TouchGestureEvent<TwistGestureInfo> TwistStarted;
    public static event TouchGestureEvent<TwistGestureInfo> TwistUpdated;
    public static event TouchGestureEvent<TwistGestureInfo> TwistEnded;

    protected override TouchGestureEvent<TwistGestureInfo> recognisedGestureEvent { get { return TwistStarted; } }
    protected override TouchGestureEvent<TwistGestureInfo> updatedGestureEvent { get { return TwistUpdated; } }
    protected override TouchGestureEvent<TwistGestureInfo> completedGestureEvent { get { return TwistEnded; } }

    protected override void Awake()
    {
        base.Awake( );

        if ( requiredFingerCount != 2 )
        {
            DebugExtras.Log( "Only two fingers supported now" );
            requiredFingerCount = 2;        
        }
    }

    protected override bool CanBegin( TwistGestureInfo gesture )
    {
        if( !base.CanBegin( gesture ) )
            return false;
       
        TouchGroup touches = gesture.GetTouches();
        if( !touches.AllMoving() )
            return false;

        if( !touches.AreMovingInOppositeDirection( tolerance ) )
            return false;

        // check if we went past the minimum rotation amount treshold
        if( Mathf.Abs( gesture.totalRotation ) < minRotation )
            return false;

        return true;
    }

    protected override void OnBegin( TwistGestureInfo gesture )
    {
    }

    protected override GestureState OnRecognise(TwistGestureInfo gesture)
    {
        if( gesture.touchCount != requiredFingerCount )
        {
            // fingers were lifted?
//            if( touches.Count < RequiredFingerCount )
//                return GestureRecognitionState.Ended;

            // more fingers added, gesture failed
            return GestureState.Ended;
        }

        // dont do anything if both fingers arent moving
//        TouchGroup touches = gesture.GetTouches();
//        if( !touches.AllMoving() )
//            return GestureState.InProgress;

        return GestureState.InProgress;
    }

    #region Utils

    // return signed angle in degrees between current finger position and ref positions
    public static float SignedAngularGap( TouchInfo touch0, TouchInfo touch1, Vector2 refPos0, Vector2 refPos1 )
    {
        Vector2 curDir = ( touch0.screenPos - touch1.screenPos ).normalized;
        Vector2 refDir = ( refPos0 - refPos1 ).normalized;

        // perpendicular dot product
        float perpDot = ( refDir.x * curDir.y ) - ( refDir.y * curDir.x );
        return Mathf.Atan2( perpDot, Vector2.Dot( refDir, curDir ) );
    }
    #endregion
}
