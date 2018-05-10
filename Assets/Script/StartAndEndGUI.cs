﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;//シーンマネジメントを有効にする

public class StartAndEndGUI : MonoBehaviour {

    public Text text;
    private FieldManager FM;

    private bool firstFrag = true;
    private bool endFrag = false;
    public void SetEndFrag() { endFrag = true; }
    private float time;

	// Use this for initialization
	void Start () {
        FM = GetComponent<FieldManager>();
        time = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if ((firstFrag) && (FM.field[0, 0] != 0))
        {
            time += Time.deltaTime;
            int t = (int)(3.9 - time);
            text.text = t.ToString();
            if(t == 0)
            {
                firstFrag = false;
                time = 0;
                text.text = null;
                GetComponent<ScoreAndTimer>().Frag = true;
                FM.Player.GetComponent<ActionJudge>().Frag = true;
            }
        }

        if (endFrag)
        {
            time += Time.deltaTime;
            text.text = "Finish";
            GetComponent<ScoreAndTimer>().Frag = false;
            FM.Player.GetComponent<ActionJudge>().Frag = false;
            if (time > 5.0)
            {
                // text.text = null;
                GetComponent<ScoreAndTimer>().TransmissionData();
                SceneManager.LoadScene("Result");
            }
        }
    }

    public void ResetTimer() {
        time = 0;
    }

}
