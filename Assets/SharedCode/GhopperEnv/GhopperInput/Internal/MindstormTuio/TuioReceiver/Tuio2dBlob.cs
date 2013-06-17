/*
Unity3d-TUIO connects touch tracking from a TUIO to objects in Unity3d.

Copyright 2011 - Mindstorm Limited (reg. 05071596)

Author - Simon Lerpiniere

This file is part of Unity3d-TUIO.

Unity3d-TUIO is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Unity3d-TUIO is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser Public License for more details.

You should have received a copy of the GNU Lesser Public License
along with Unity3d-TUIO.  If not, see <http://www.gnu.org/licenses/>.

If you have any questions regarding this library, or would like to purchase 
a commercial licence, please contact Mindstorm via www.mindstorm.com.
*/

/*
November 2012
Tuio2DBlob class for TUIO 1.1 blobs created by Ien Cheng based on
Unity3d-TUIO's Tuio2DCursor class.
*/

using System;
using System.Collections.Generic;
using OSC;

/// <summary>
/// Handles all data required for a TUIO cursor ready to send or receive on OSC.
/// </summary>
namespace Tuio
{
	public class Tuio2DBlob
	{
	    int sessionID;
	    float posX, posY, angle, width, height, area, velX, velY, rotationA, motionAcc, rotationAcc;
	
	    #region Constructor
	    public Tuio2DBlob()
	    {
	
	    }
	
	    public Tuio2DBlob(OSCMessage message)
	    {
	        this.SessionID = Convert.ToInt32(message.Values[1]);
	        this.PositionX = Convert.ToSingle(message.Values[2]);
	        this.PositionY = Convert.ToSingle(message.Values[3]);
	        this.Angle = Convert.ToSingle(message.Values[4]);
	        this.Width = Convert.ToSingle(message.Values[5]);
	        this.Height = Convert.ToSingle(message.Values[6]);
	        this.Area = Convert.ToSingle(message.Values[7]);
	        this.VelocityX = Convert.ToSingle(message.Values[8]);
	        this.VelocityY = Convert.ToSingle(message.Values[9]);
	        this.Rotation = Convert.ToSingle(message.Values[10]);
	        this.MotionAcceleration = Convert.ToSingle(message.Values[11]);
	        this.RotationAcceleration = Convert.ToSingle(message.Values[12]);
	    }
	    #endregion
	
	    #region Get OSC Message
	    public OSCMessage GetMessage()
	    {
	        OSCMessage msg = new OSCMessage("/tuio/2Dblb");
	        msg.Append("set");
	        msg.Append(this.SessionID);
	        msg.Append(this.PositionX);
	        msg.Append(this.PositionY);
	        msg.Append(this.Angle);
	        msg.Append(this.Width);
	        msg.Append(this.Height);
	        msg.Append(this.Area);
	        msg.Append(this.VelocityX);
	        msg.Append(this.VelocityY);
	        msg.Append(this.Rotation);
	        msg.Append(this.MotionAcceleration);
	        msg.Append(this.RotationAcceleration);
	        return msg;
	    }
	    #endregion
	
	    #region SessionID
	    public int SessionID
	    {
	    	get { return sessionID; }
	    	set { sessionID = value; }
	    }
	    #endregion

	    #region PositionX
	    public float PositionX
	    {
	    	get { return posX; }
	    	set { posX = value; }
	    }
	    #endregion

	    #region PositionY
	    public float PositionY
	    {
	    	get { return posY; }
	    	set { posY = value; }
	    }
	    #endregion

	    #region Angle
	    public float Angle
	    {
	    	get { return angle; }
	    	set { angle = value; }
	    }
	    #endregion

	    #region Width
	    public float Width
	    {
	    	get { return width; }
	    	set { width = value; }
	    }
	    #endregion

	    #region Height
	    public float Height
	    {
	    	get { return height; }
	    	set { height = value; }
	    }
	    #endregion

	    #region Area
	    public float Area
	    {
	    	get { return area; }
	    	set { area = value; }
	    }
	    #endregion

	    #region VelocityX
	    public float VelocityX
	    {
	    	get { return velX; }
	    	set { velX = value; }
	    }
	    #endregion

	    #region VelocityY
	    public float VelocityY
	    {
	    	get { return velY; }
	    	set { velY = value; }
	    }
	    #endregion

	    #region Rotation
	    public float Rotation
	    {
	    	get { return rotationA; }
	    	set { rotationA = value; }
	    }
	    #endregion

	    #region MotionAcceleration
	    public float MotionAcceleration
	    {
	    	get { return motionAcc; }
	    	set { motionAcc = value; }
	    }
	    #endregion

	    #region RotationAcceleration
	    public float RotationAcceleration
	    {
	    	get { return rotationAcc; }
	    	set { rotationAcc = value; }
	    }
	    #endregion

	    public bool IsEqual(Tuio2DBlob blob)
	    {
	    	return this.SessionID == blob.SessionID
		        && this.PositionX == blob.PositionX
		        && this.PositionY == blob.PositionY
		        && this.Angle == blob.Angle
		        && this.Width == blob.Width
		        && this.Height == blob.Height
		        && this.Area == blob.Area
		        && this.VelocityX == blob.VelocityX
		        && this.VelocityY == blob.VelocityY
		        && this.Rotation == blob.Rotation
		        && this.MotionAcceleration == blob.MotionAcceleration
		        && this.RotationAcceleration == blob.RotationAcceleration;
	    }
	}
}