using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataMessenger : MonoBehaviour
{

    int score;
    int chain;
    string userName;
    string mode;
    int playTimes;

    public int Score
    {
        get { return score; }
        set { score = value; }
    }
    public int Chain
    {
        get { return chain; }
        set { chain = value; }
    }
    public string Name
    {
        get { return userName; }
        set { userName = value; }
    }
    public string Mode {
        get { return mode; }
        set { mode = value; }
    }
    public int PlayTimes {
        get { return playTimes; }
        set { playTimes = value; }
    }

    // Use this for initialization
    void Awake()
    {
        playTimes = 0;
        DontDestroyOnLoad(this);
    }

}
