using UnityEngine;
using System.Collections;

public class ScoreBehavior : MonoBehaviour {
	GameObject[] pegs;
	int score;

	// Use this for initialization
	void Start () {
		pegs = new GameObject[5];
		int i = 0;
		foreach (Transform child in transform)
		{
		    pegs[i] = child.gameObject;
			++i;
		}
		
	}
	
//	// Update is called once per frame
//	void Update () {
//	
//	}
	
	public void addPoint(Color argColor) {
		pegs[score].renderer.material.color = argColor;
		++score;
	}
	
	public void subtractPoint() {
		if (score > 0){
			--score;
			pegs[score].renderer.material.color = Color.white;
		}
	}
}
