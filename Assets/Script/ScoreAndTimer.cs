using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ScoreAndTimer : MonoBehaviour {

    /*時間、連鎖関係*/
    private int chainNum = 0;
    private float chainTime = 0;
    private float chainThresholdTime;
    private float gameTime = 0;
    private const float gameEndTime = 99.9f;
    private bool chainingFrag = false;
    private int score = 0;
    private int maxChainNum = 0;

    /*公開メンバ*/
    public int ChainNum { get { return chainNum; } }
    public int DispTime { get { return (int)(gameEndTime - gameTime + 1); } }
    public float RestPercent { get { return(chainingFrag) ? (1-chainTime/chainThresholdTime) : 0.0f; } }
    public int Score { get { return score; } }

    /*他への参照*/
    private FieldManager FM;
    
    /*公開OnOffFrag*/
    private bool updateFrag = true;
    private bool enable;
    public bool Frag { set { enable = value; } get { return enable; } }
    public bool TimeUpdate { set { updateFrag = value; } }
    

    // Use this for initialization
    void Start() {
        Frag = false; //はじめはカウントしない
        FM = GetComponent<FieldManager>();
        if (GameObject.Find("GameManager").GetComponent<DataMessenger>().Mode == "easy") chainThresholdTime = 10.0f;
        else chainThresholdTime = 5.5f;
    }

    // Update is called once per frame
    void FixedUpdate() { 

        if (Frag) {

            if (updateFrag) gameTime += Time.deltaTime;
            if (updateFrag&&chainingFrag) chainTime += Time.deltaTime;
            if (chainTime > chainThresholdTime) { chainNum = 0;chainingFrag = false; }
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
        GetComponent<UIManager>().DrawChainNum(chainNum);
        FieldManager.audio.CallChain(chainNum - 1);    //音鳴らす：連鎖数-1
        chainingFrag = true;
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
