using UnityEngine;
using System.Collections;

public class Results : MonoBehaviour, ITouchable {
 
// ========================================
//               VARIABLES
// ========================================

    // Results options
    public GUIStyle backStyle;
    public bool showResults;
    public bool showBackButton;
    public int winningPlayer;
    PlayerScoreBezel playerScoreBezel;
    

    // Handles to GameObjects corresp to diff player win screens
    public GameObject redWins;
    public GameObject blueWins;
    public GameObject greenWins;
    public GameObject yellowWins;
    public GameObject orangeWins;
    public GameObject purpleWins;


// ========================================
//              FUNCTIONS
// ========================================

	// INITIALIZATION
    // --------------
	void Start () {
	    showResults = false;
        showBackButton = false;
        winningPlayer = -1;
	}
	
	// UPDATES
    // -------
	void Update () {
	
	}
    
    public void StartTouch(TouchInfo info) {
            
    }
        
    public void UpdateTouch(TouchInfo info) {
            
    }
     
    public void EndTouch(TouchInfo info) {
        showBackButton = true;
    }
    

    // GAME OVER GUI FUNCTION
    // ----------------------
    void OnGUI() {
        if(showResults) {
            
            Vector3 resultsScreenPosition = new Vector3(0.0f, 0.0f, -1.5f);
            
            switch(winningPlayer) {
			
                case 0:
                    redWins.transform.position = resultsScreenPosition;
                    break;
                case 1:
                    blueWins.transform.position = resultsScreenPosition;
                    break;
                case 2:
                    greenWins.transform.position = resultsScreenPosition;
                    break;
                case 3:
                    yellowWins.transform.position = resultsScreenPosition;
                    break;
                case 4:
                    purpleWins.transform.position = resultsScreenPosition;
                    break;
                case 5:
                    orangeWins.transform.position = resultsScreenPosition;
                    break;
            }
            
           
            //Need to match GUI.Button to button visual
            
            if(showBackButton) {
            
                if(GUI.Button(new Rect(165,
                                       110,
                                       100,
                                       100),
                                        "",
                                       backStyle)) 
                {
                    Debug.Log("Clicked Back");
                    Application.LoadLevel("Scene");
    				Vector3 getAwayPosition = new Vector3(-100f, -100f, -100f);
                    GameObject.Find ("Red Wins").transform.position = getAwayPosition;
    				GameObject.Find ("Green Wins").transform.position = getAwayPosition;
    				GameObject.Find ("Blue Wins").transform.position = getAwayPosition;
    				GameObject.Find ("Yellow Wins").transform.position = getAwayPosition;
    				GameObject.Find ("Purple Wins").transform.position = getAwayPosition;
    				GameObject.Find ("Orange Wins").transform.position = getAwayPosition;                
                }
            }
        }
    }
}
