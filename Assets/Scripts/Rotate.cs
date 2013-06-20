using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

    public float rotationRate = 0.1f;
    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    transform.Rotate(0.0f, rotationRate * Time.deltaTime, 0.0f);
	}
}
