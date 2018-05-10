using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayResultTextManager : MonoBehaviour {

    bool firstFrag = true;

    public Text nameText;
    public Text scoreChainText;
    public Text valueText;


    private void Awake()
    {
        nameText.text = "";
        scoreChainText.text = "";
        valueText.text = "";
    }

    public void DispMessage(string name,int score,int chain)
    {
        if (!firstFrag) return;
        nameText.text = name.ToString() + "さん";
        scoreChainText.text = "　スコア\n最大連鎖";
        valueText.text = score.ToString() + "\n" + chain.ToString();
        firstFrag = false;
    }
}
