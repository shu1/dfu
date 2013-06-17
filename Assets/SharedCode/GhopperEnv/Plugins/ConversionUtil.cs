using System;
using UnityEngine;

// static methods for value conversions
public class ConversionUtil {
	
	public static int ColorToInt(Color32 color) {
		return HexToInt( ColorToHex(color) );
	}
	
	public static Color IntToColor(int color) {
		return HexToColor( IntToHex(color) );
	}
	
	public static string ColorToHex(Color32 color) {
		string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		return hex;
	}
	
	public static Color HexToColor(string hex) {
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r,g,b, 255);
	}
	
	public static int HexToInt(string hexValue) {
		return int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
	}
	
	public static string IntToHex(int intVal) {
		return intVal.ToString("X");
	}
}


