using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    /*他への参照*/
    public CanvasScaler canvas;
    private ScoreAndTimer SAT;
    private ActionJudge AJ;

    /*パラメータ用*/
    public Text scoreNumText;
    public Text bonusNumText;
    public Text timeNumText;

    //RestTimeBar用
 //   public Text restTimeText;
    public RectTransform backBar;
    public RectTransform frontBar;

    /*連鎖数書くよう*/
    public Text chainNumText;
    private float timer = 0.0f;

    // Use this for initialization
    void Awake () {
        canvas.referenceResolution.Set(1080, 1920);
        SAT = GameObject.Find("FieldController").GetComponent<ScoreAndTimer>();
        AJ = GameObject.Find("Player").GetComponent<ActionJudge>();

 //       restTimeText.text = "";
        chainNumText.text = "";
    }
	
	// Update is called once per frame
	void Update () {

        //連鎖数
        timer += Time.deltaTime;
        if (timer< 1.0f) {
            chainNumText.fontSize = (int)(250.0f + 100.0f * timer);
            chainNumText.color = new Color(0.1f, 0.1f, 0.1f, 0.8f - 0.8f * timer);
        }
        if (timer > 2.0f) chainNumText.text = "";


        //RestTimeBar用
    //    restTimeText.text = (SAT.RestChainTime > 0.0f) ? SAT.RestChainTime.ToString() : "0.0";
        frontBar.transform.localScale = new Vector3(SAT.RestPercent, 1.0f, 1.0f);

        /*パラメータ用*/
        scoreNumText.text = SAT.Score.ToString();
        timeNumText.text = SAT.DispTime.ToString();
        bonusNumText.text = AJ.BonusNum.ToString();
  
    }

    public void DrawChainNum(int chainNum) {
        timer = 0.0f;
        chainNumText.text = chainNum.ToString();
        chainNumText.fontSize = 200;
        chainNumText.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    }
}
