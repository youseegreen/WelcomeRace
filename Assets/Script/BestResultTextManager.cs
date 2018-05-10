using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestResultTextManager : MonoBehaviour {

    bool firstFrag = true;

    public Text bestText;
    public Text scoreChainText;
    public Text valueText;


    private void Awake()
    {
        bestText.text = "";
        scoreChainText.text = "";
        valueText.text = "";
    }


    public void DispMessage(int score, int chain, bool updateFrag)
    {
        if (!firstFrag) return;
        if (updateFrag) bestText.text = "自己ベスト更新！";
        else bestText.text = "自己ベスト";
        scoreChainText.text = "　スコア\n最大連鎖";
        valueText.text = score.ToString() + "\n" + chain.ToString();
        firstFrag = false;
    }
}
