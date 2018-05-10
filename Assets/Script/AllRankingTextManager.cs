using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllRankingTextManager : MonoBehaviour {

    bool firstFrag = true;

    public Text allRankingText;
    public Text[] scoreText;
    public Text[] chainText;
    public Text[] nameText;
    public Text[] rankText;

    //初めは表示させない
    private void Awake()
    {
        allRankingText.text = "";
        for (int i = 0; i < 3; i++)
        {
            rankText[i].text = "";
            nameText[i].text = "";
            scoreText[i].text = "";
            chainText[i].text = "";
        }
    }


    public void DispMessage(string []name, int []score, int []chain)
    {
        if (!firstFrag) return;
        allRankingText.text = "全体成績";
        for (int i = 0; i < 3; i++)
        {
            if (name[i] != null)
            {
                rankText[i].text = (i+1).ToString() + "位";
                nameText[i].text = name[i];
                scoreText[i].text = score[i].ToString();
                chainText[i].text = chain[i].ToString();
            }
        }
        firstFrag = false;
    }
}
