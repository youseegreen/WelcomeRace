using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataMessenger : MonoBehaviour
{

    int score;
    int chain;
    string userName;
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

    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(this);
    }

}
