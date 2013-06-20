using UnityEngine;
using System.Collections;

public class OneShotParticle : MonoBehaviour {
	ParticleSystem ps;

	void Start() {
		ps = GetComponent<ParticleSystem>();
	}
	
	void Update() {
		if (!ps.IsAlive()) {
			Destroy(ps);
			Destroy(gameObject);
		}
	}
}
