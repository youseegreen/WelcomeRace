using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;//シーンマネジメントを有効にする

public class ResultManager : MonoBehaviour {

    private float time = 0;

    private string[] rName = new string[3];
    private int[] rScore = new int[3];
    private int[] rChain = new int[3];

    private string pName;
    private int score;
    private int chain;

    private int hiScore;
    private int hiChain;

    private bool updateFrag = false;

    public Text thankText;
    DataMessenger messenger;
    SQLite3 SQL;


    // Use this for initialization
    void Start()
    {
        thankText.text = "";
        GameObject.Find("Player").GetComponent<AvatarController>().mirroredMovement = true;
        messenger = GameObject.Find("GameManager").GetComponent<DataMessenger>();
        messenger.PlayTimes += messenger.PlayTimes + 1;
        pName = messenger.Name;
        score = messenger.Score;
        chain = messenger.Chain;


        SQL = GetComponent<SQLite3>();

        if(pName != "guest")SQL.AddDataAndGetHiScore(score, chain, pName, out hiScore, out hiChain,out updateFrag);
        //トップ3をゲットする
        SQL.GetTop3(out rName,out rScore, out rChain);
    }


    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        if (time > 3.0)
        {
            GameObject.Find("PlayResultTextManager").
                GetComponent<PlayResultTextManager>().DispMessage(pName,score,chain);
        }

        if (time > 5.0)
        {
            GameObject.Find("BestResultTextManager").
                GetComponent<BestResultTextManager>().DispMessage(pName, hiScore, hiChain, updateFrag);
        }


        if (time > 7.0)
        {
            GameObject.Find("AllRankingTextManager").
                GetComponent<AllRankingTextManager>().DispMessage(rName, rScore, rChain);
        }

        if(time > 9.0)
        {
            thankText.text = "Thank you for playing!!";
        }


        //人が消えたらスタートに戻す
        if ((GameObject.Find("GameManager").GetComponent<KinectManager>().GetPlayer1ID() == 0)&&(time > 12.0))
        {
        //    GetComponent<SQLite3>().SearchName();
            SceneManager.LoadScene("Start");//Mainシーンをロードする
        }
    }
}
