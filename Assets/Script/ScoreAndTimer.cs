using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ScoreAndTimer : MonoBehaviour {

    private int chainNum = 0;
    private float chainTime = 0;
    private const float chainThresholdTime = 5.5f;
    private float gameTime = 0;
    private const float gameEndTime = 100.0f;
    private int score = 0;
    private int maxChainNum = 0;
    private bool updateFrag = true;

    public bool TimeUpdate {
        set { updateFrag = value; }
    }


    public Text scoreText;
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
        scoreText.text = "C " + chainNum.ToString()
                    + "  B " + AJ.BonusNum.ToString()
                    + " T " + dispTime.ToString()
                    + " S " + score.ToString();

        if (Frag) {
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
        chainTime = 0;
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
