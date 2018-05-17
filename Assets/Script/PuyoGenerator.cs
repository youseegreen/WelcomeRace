using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuyoGenerator : MonoBehaviour
{
    private FieldManager FM;    //フィールドマネージャへの参照
    private int puyoColorNum;   //モードによって変更する

    
    //連鎖チェック用
    private bool[,] checkFrag;
    private int[,] connectNum;

    // Use this for initialization
    void Start()
    {
        //        if (GameObject.Find("GameManager").GetComponent<DataMessenger>().Mode == "easy") puyoColorNum = 4;
        //       else puyoColorNum = 5;
        puyoColorNum = 5;

        FM = GetComponent<FieldManager>();
        checkFrag = new bool[FM.W, FM.H];
        connectNum = new int[FM.W, FM.H];
        //初期適当に配置する
        for (int y = 0; y < FM.H; y++)
        {
            for (int x = 0; x < FM.W; x++)
            {
                FM.field[x, y] = Random.Range(1, puyoColorNum + 1);
            }
        }

        //連鎖しないようにフィールドいじる
        while (IsChain()) { }

        //それをもとにインスタンス化  ぷよにはColor Tagをつける
        for (int y = 0; y < FM.H; y++)
        {
            for (int x = 0; x < FM.W; x++)
            {
                GameObject puyo = Instantiate(FM.puyo, new Vector3(x, FM.H + y, 0), new Quaternion(0, 0, 0, 0));
                int val = FM.field[x, y];
                if (val == 1) puyo.tag = "Red";
                else if (val == 2) puyo.tag = "Green";
                else if (val == 3) puyo.tag = "Blue";
                else if (val == 4) puyo.tag = "Yellow";
                else if (val == 5) puyo.tag = "Purple";
                else puyo.tag = "No";

            //    if (y >= 30)
            //    {
            //        puyo.tag = "No";
               //   
               //     }
                FM.puyoList.Add(puyo);
            }
        }
    }

   

    bool IsChain()
    {
        bool chainFrag = false;
        Variable0Fill();
        for (int y = 0; y < FM.H; y++)
        {
            for (int x = 0; x < FM.W; x++)
            {
                //注目対象が色ぷよ かつ 未捜査
                if (checkFrag[x, y] == false)
                {
                    ConnectCheck(x, y, FM.field[x, y]);
                }
            }
        }

        for (int y = 0; y < FM.H; y++)
        {
            for (int x = 0; x < FM.W; x++)
            {
                if ((connectNum[x, y] > 3) && (FM.field[x, y] != 0))
                {
                    chainFrag = true;
                    ChangeColor(x, y, FM.field[x, y]);
                }
            }
        }
        return chainFrag;
    }
    int ConnectCheck(int u, int v, int col)
    {
        connectNum[u, v] = 1;
        checkFrag[u, v] = true;
        if ((v + 1) < FM.H)
        {
            if ((FM.field[u, v + 1] == col) && (checkFrag[u, v + 1] == false))
            {
                connectNum[u, v] += ConnectCheck(u, v + 1, col);
            }
        }
        if ((v - 1) >= 0)
        {
            if ((FM.field[u, v - 1] == col) && (checkFrag[u, v - 1] == false))
            {
                connectNum[u, v] += ConnectCheck(u, v - 1, col);
            }
        }
        if ((u + 1) < FM.W)
        {
            if ((FM.field[u + 1, v] == col) && (checkFrag[u + 1, v] == false))
            {
                connectNum[u, v] += ConnectCheck(u + 1, v, col);
            }
        }
        if ((u - 1) > -1)
        {
            if ((FM.field[u - 1, v] == col) && (checkFrag[u - 1, v] == false))
            {
                connectNum[u, v] += ConnectCheck(u - 1, v, col);
            }
        }
        return connectNum[u, v];
    }
    void ChangeColor(int u, int v, int col)
    {
        FM.field[u, v] = Random.Range(1, puyoColorNum + 1);
        if ((v + 1) < FM.H)
        {
            if (FM.field[u, v + 1] == col)
            {
                ChangeColor(u, v + 1, col);
            }
        }
        if ((v - 1) >= 0)
        {
            if (FM.field[u, v - 1] == col)
            {
                ChangeColor(u, v - 1, col);
            }
        }
        if ((u + 1) < FM.W)
        {
            if (FM.field[u + 1, v] == col)
            {
                ChangeColor(u + 1, v, col);
            }
        }
        if ((u - 1) > -1)
        {
            if (FM.field[u - 1, v] == col)
            {
                ChangeColor(u - 1, v, col);
            }
        }
    }

    void Variable0Fill()
    {
        for (int y = 0; y < FM.H; y++)
        {
            for (int x = 0; x < FM.W; x++)
            {
                checkFrag[x, y] = false;
                connectNum[x, y] = 0;
            }
        }
    }

    //色なしぷよを生成する  色なしじゃなくする
    public void GenerateNoPuyo(Transform tf)
    {
        int x = (int)(tf.position.x + 0.2);
        float y = tf.position.y;// / 100;
        GameObject puyo = Instantiate(FM.puyo, new Vector3(x, FM.H + y, 0), new Quaternion(0, 0, 0, 0));
        int val = Random.Range(1, puyoColorNum + 1);
        if (val == 1) puyo.tag = "Red";
        else if (val == 2) puyo.tag = "Green";
        else if (val == 3) puyo.tag = "Blue";
        else if (val == 4) puyo.tag = "Yellow";
        else if (val == 5) puyo.tag = "Purple";
        else puyo.tag = "No";
        FM.puyoList.Add(puyo);
    }

    //ワンちゃん　お邪魔降らす機能もいるかも
}

