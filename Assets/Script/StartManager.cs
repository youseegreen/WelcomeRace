using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//シーンマネジメントを有効にする

public class StartManager : MonoBehaviour {

    KinectManager KM;
	// Use this for initialization
	void Start () {
        KM = GetComponent<KinectManager>();
        GameObject.Find("GameManager").GetComponent<DataMessenger>().Name = null;
	}
	
	// Update is called once per frame
	void Update () {
        if (KM.GetPlayer1ID() == 0) return;

        string playerName;
        GameObject.Find("name").GetComponent<NameManager>().GetName(out playerName);
        GameObject.Find("GameManager").GetComponent<DataMessenger>().Name = playerName;

        SceneManager.LoadScene("Main");//Mainシーンをロードする
    }
}
