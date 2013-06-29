using UnityEngine;
using System.Collections;

public class ScoreBehavior : MonoBehaviour {
	GameObject[] pegs;
	int score;
	Color blankPegColor;

	// Use this for initialization
	void Start () {
		blankPegColor = Color.white;
		blankPegColor.a = 0.5f;

		pegs = new GameObject[3];
		score = 0;
		int i = 0;
		foreach (Transform child in transform)
		{
		    pegs[i] = child.gameObject;
		    pegs[i].renderer.material.color = blankPegColor;
			++i;
		}
		
	}
	
	public void addPoint(Color argColor) {
		pegs[score].renderer.material.color = argColor;
		++score;
	}
	
	public void subtractPoint() {
		if (score > 0){
			--score;
			pegs[score].renderer.material.color = blankPegColor;
		}
	}
}
