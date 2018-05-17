using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//シーンマネジメントを有効にする
using UnityEngine.UI;

public class StartManager : MonoBehaviour {

    //    KinectManager KM;
    // Use this for initialization
    // Use this for initialization
    private ActionJudge AJ;
    public Image easyPanel;
    public Image hardPanel;
    private string mode = "easy";
    public string Mode{
        set { mode = value; }
        get { return mode; }
    }

    void Awake() {
 //       KM = GameObject.Find("GameManager").GetComponent<KinectManager>();
        GameObject.Find("GameManager").GetComponent<DataMessenger>().Name = null;
        AJ = GameObject.Find("Player").GetComponent<ActionJudge>();
        GameObject.Find("Player").GetComponent<AvatarController>().mirroredMovement = true;
        AJ.GetStartSceneObject();
        
    }

    // Update is called once per frame
    void Update() {
        //      if (KM.GetPlayer1ID() == 0) return;
        if (AJ.RaiseFoot == "left") {
            mode = "hard";
            easyPanel.color = new Color(255, 255, 255);
            hardPanel.color = new Color(255, 0, 0);
        }
        else if(AJ.RaiseFoot == "right") {
            mode = "easy";
            hardPanel.color = new Color(255, 255, 255);
            easyPanel.color = new Color(255, 0, 0);
        }

        if (!AJ.StartPose) return;

        string playerName;
        GameObject.Find("name").GetComponent<NameManager>().GetName(out playerName);
        GameObject.Find("GameManager").GetComponent<DataMessenger>().Name = playerName;
        GameObject.Find("GameManager").GetComponent<DataMessenger>().Mode = mode;
        GameObject.Find("Player").GetComponent<AvatarController>().mirroredMovement = false;

        SceneManager.LoadScene("Main");//Mainシーンをロードする
    }
}
