using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour {

    /*フィールドのサイズなど*/
    private const int height = 30;
    private const int width = 6;
    private const int chainHeight = 10;
    public int CH {
        get { return chainHeight; }
    }
    public int H {
        get { return height; }
    }
    public int W {
        get { return width; }
    }

    /*他への参照*/
    public GameObject Player;
    public GameObject puyo; //ぷよぷよ

    /*公開用　変数*/
    public List<GameObject> puyoList;
    public int[,] field = new int[width, height];
    public GameObject[,] obje = new GameObject[width, height];

    private void Start() {
        Player = GameObject.Find("Player");
    }


    // Update is called once per frame
    void FixedUpdate() {

        bool judgeChainFrag = true;

        //フィールドを最新状態にセット
        //フィールドリセット
        for (int y = 0; y < H; y++) {
            for (int x = 0; x < W; x++) {
                field[x, y] = 0;
                obje[x, y] = null;
            }
        }

        //止まってるやつをフィールドにぶち込む
        for (int i = 0; i < puyoList.Count; i++) {
            int y = (int)(puyoList[i].GetComponent<PuyoController>().transform.position.y + 0.4);
            int x = (int)(puyoList[i].GetComponent<PuyoController>().transform.position.x + 0.4);
            if (puyoList[i].GetComponent<PuyoController>().IsMove()) {
                if (y < 12) judgeChainFrag = false;
                else continue;
            }
            if (y >= H) continue;

            field[x, y] = puyoList[i].GetComponent<PuyoController>().ColorNum;
            obje[x, y] = puyoList[i];
        }

        //入力いれるかも
        GetComponent<P_F_Interface>().Action();

        //連鎖    ChainCheckスクリプトに委託
        int num, colorNum;
        if (judgeChainFrag) {
            if (GetComponent<ChainCheck>().IsChain(out num, out colorNum)) {
                GetComponent<ScoreAndTimer>().AddChain(num, colorNum);
                GetComponent<StartAndEndGUI>().ResetTimer();
            }
        }

    }


    public void AddBonusPuyo() {
        int num = -1;

        while (true) {
            num = Random.Range(0, puyoList.Count);
            if (puyoList[num].GetComponent<PuyoController>().ColorNum != 0)
                if (!puyoList[num].GetComponent<PuyoController>().BonusFrag)
                    if (puyoList[num].transform.position.y < CH)
                        break;
        }
        puyoList[num].GetComponent<PuyoController>().BonusFrag = true;
    }
}
