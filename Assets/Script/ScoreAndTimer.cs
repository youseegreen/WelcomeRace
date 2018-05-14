using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ScoreAndTimer : MonoBehaviour {

    private int chainNum = 0;
    private float chainTime = 0;
    private float chainTime2 = 0;
    private const float chainThresholdTime = 5.5f;
    private float gameTime = 0;
    private const float gameEndTime = 99.9f;
    private int score = 0;
    private int maxChainNum = 0;
    private bool updateFrag = true;

    public bool TimeUpdate {
        set { updateFrag = value; }
    }

    public Text chainText;
    public Text scoreText;
    public Text scoreNumText;
    private ActionJudge AJ;
    private FieldManager FM;


    private bool enable;
    public bool Frag {
        set { enable = value; }
        get { return enable; }
    }

    // Use this for initialization
    void Start() {
        Frag = false; //はじめはカウントしない
        FM = GetComponent<FieldManager>();
        AJ = FM.Player.GetComponent<ActionJudge>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        int dispTime = (int)(gameEndTime - gameTime + 1);
        scoreText.text = "★：" + AJ.BonusNum.ToString()
                    + "　 score：    　 time：" + dispTime.ToString();
        scoreNumText.text = score.ToString();

        if (Frag) {
            //止まってる時でも止まらないタイマー
            chainTime2 += Time.deltaTime;
            if (chainTime2 < 1.0f) {
                chainText.fontSize = (int)(250 + 100 * chainTime2);
                chainText.color = new Color(0.1f, 0.1f, 0.1f, 0.8f - 0.8f * chainTime2);
            }
            if (chainTime2 > 2.0f) chainText.text = "";


            if (updateFrag) gameTime += Time.deltaTime;
            if (updateFrag) chainTime += Time.deltaTime;
            if (chainTime > chainThresholdTime) chainNum = 0;
            if (gameTime > gameEndTime) 
                GetComponent<StartAndEndGUI>().SetEndFrag();
        }
    }


    public void AddChain(int num, int colorNum) {
        chainNum++;
        if (maxChainNum < chainNum) maxChainNum = chainNum;
        score += chainNum * num;
        score += colorNum - 1;
        if ((chainNum % 5 == 0) && (chainNum > 0)) { FM.AddBonusPuyo(); }
        chainText.text = chainNum.ToString();       //連鎖数の表示
        chainText.fontSize = 200;
        chainText.color = new Color(0.1f, 0.1f, 0.1f,0.8f);
        FieldManager.audio.CallChain(chainNum - 1);    //音鳴らす：連鎖数-1
        chainTime = 0;
        chainTime2 = 0;
    }

    public void AddScore(int num = 1) {
        if (num < 0) return;
        score += num;
    }

    public void TransmissionData() {
        GameObject.Find("GameManager").GetComponent<DataMessenger>().Score = score;
        GameObject.Find("GameManager").GetComponent<DataMessenger>().Chain = maxChainNum;
    }



}
