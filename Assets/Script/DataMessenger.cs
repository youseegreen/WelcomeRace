using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataMessenger : MonoBehaviour
{

    int score;
    int chain;
    string userName;
    string mode;

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


    // Use this for initialization
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

}
