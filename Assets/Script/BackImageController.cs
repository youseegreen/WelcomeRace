using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackImageController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        int t = GameObject.Find("GameManager").GetComponent<DataMessenger>().PlayTimes;
        Renderer renderer = GetComponent<Renderer>();
        t %= 5;
        switch (t) {
            case 0:
                renderer.material = GameObject.Find("back1").GetComponent<Renderer>().material;
                break;
            case 1:
                renderer.material = GameObject.Find("back2").GetComponent<Renderer>().material;
                break;
            case 2:
                renderer.material = GameObject.Find("back3").GetComponent<Renderer>().material;
                break;
            case 3:
                renderer.material = GameObject.Find("back4").GetComponent<Renderer>().material;
                break;
            default:
                renderer.material = GameObject.Find("back5").GetComponent<Renderer>().material;
                break;
        }
	}

}
