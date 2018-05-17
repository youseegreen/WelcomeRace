using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//シーンマネジメントを有効にする

public class StartManager : MonoBehaviour {

    //    KinectManager KM;
    // Use this for initialization
    // Use this for initialization
    private ActionJudge AJ;

    void Awake() {
 //       KM = GameObject.Find("GameManager").GetComponent<KinectManager>();
        GameObject.Find("GameManager").GetComponent<DataMessenger>().Name = null;
        AJ = GameObject.Find("Player").GetComponent<ActionJudge>();
        AJ.GetStartSceneObject();
    }

    // Update is called once per frame
    void Update() {
        //      if (KM.GetPlayer1ID() == 0) return;

        if (!AJ.StartPose) return;

        string playerName;
        GameObject.Find("name").GetComponent<NameManager>().GetName(out playerName);
        GameObject.Find("GameManager").GetComponent<DataMessenger>().Name = playerName;
        GameObject.Find("GameManager").GetComponent<DataMessenger>().Name = playerName;
        GameObject.Find("GameManager").GetComponent<DataMessenger>().Name = playerName;
        SceneManager.LoadScene("Main");//Mainシーンをロードする
    }
}
