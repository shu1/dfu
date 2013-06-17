/* GhopperTableEditor.cs
 * Copyright Grasshopper 2012
 * ----------------------------
 *
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class GhopperTableEditor 
{
	[MenuItem("Grasshopper/Setup For Grasshopper")]
	static void TableSetup()
	{
		// table settings
		PlayerSettings.defaultIsFullScreen = false;
		PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
		PlayerSettings.defaultScreenWidth = 1440;
		PlayerSettings.defaultScreenHeight = 1080;
		PlayerSettings.SetAspectRatio( AspectRatio.Aspect4by3, true );
		PlayerSettings.resizableWindow = true;
		
		PlayerSettings.companyName = "GrasshopperNYC";	
		PlayerSettings.runInBackground = true;

		// mobile settings
		PlayerSettings.bundleIdentifier = "com.GrasshopperNYC." + PlayerSettings.productName;
		PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
	}
}
