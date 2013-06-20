using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour, ITouchable {
 
    public GUIStyle withMeStyle;
    public GUIStyle againstMeStyle;
    
    public bool titleSplash;
    public bool showStartButtons;
    
 // Use this for initialization
 void Start () {
    
    titleSplash = true;
    showStartButtons = true;
 
    }
 
 // Update is called once per frame
 void Update () {
 
 }
    
 public void StartTouch(TouchInfo info) {
        
 }
    
 public void UpdateTouch(TouchInfo info) {
        
 }
 
 public void EndTouch(TouchInfo info) {
    if(titleSplash) {
        showStartButtons = true;
        titleSplash = false;
    }
 }
 
void OnGUI() {
    
/**        GameObject titleScreen = GameObject.Find("Title Screen");
        
        if(titleSplash || showStartButtons) {
            
//            titleScreen.transform.position = new Vector3(-0.15f, -0.06f, -6.7f);
              titleScreen.transform.position = new Vector3(-100f, -100f, -100f);
            
            
            //The "button" is just the rect - needs to be adjusted to fit button visual
           if(GUI.Button(new Rect(95,
                                   110,
                                   100,
                                   100),
                                    "",
                                   withMeStyle)) 
            {
                Debug.Log("Clicked With");
                Application.LoadLevel("Scene");
            }
           
            
        }
        
        else {
            
            titleScreen.transform.position = new Vector3 (100, 100, 100);
            
        }
        
        if(showStartButtons) {
            
            if(GUI.Button(new Rect(170,
                                   110,
                                   100,
                                   100),
                                    "",
                                   againstMeStyle)) 
            {
                Debug.Log("Clicked Against");
                showStartButtons = false;
            }
            
            if(GUI.Button(new Rect(242,
                                   110,
                                   100,
                                   100),
                                   "",
                                   againstMeStyle)) 
            {
                Debug.Log("Clicked Against");
            }

        }
*/            
    }

}
