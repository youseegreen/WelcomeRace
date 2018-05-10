using System.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainCheck : MonoBehaviour {

    private FieldManager FM;

    //FM.CHはFMに持たせる?
    private bool[,] checkFrag;
    private int[,] connectNum;
    private bool[] colorFrag = new bool[5];
    private int vanishNum;

    void Start() {
        FM = GetComponent<FieldManager>();
        checkFrag = new bool[FM.W, FM.CH];
        connectNum = new int[FM.W, FM.CH];
    }

    public bool IsChain(out int num, out int colorNum) {
        bool chainFrag = false;
        //FM.W×FM.CHマスリセット
        Variable0Fill();

        for (int y = 0; y < FM.CH; y++) {
            for (int x = 0; x < FM.W; x++) {
                //注目対象が色ぷよ かつ 未捜査
                if ((FM.field[x, y] != 0) && (checkFrag[x, y] == false)) {
                    ConnectCheck(x, y, FM.field[x, y]);
                }
            }
        }

        for (int y = 0; y < FM.CH; y++) {
            for (int x = 0; x < FM.W; x++) {
                if ((connectNum[x, y] > 3) && (FM.field[x, y] != 0)) {
                    chainFrag = true;
                    colorFrag[FM.field[x, y] - 1] = true;
                    ChainVanish(x, y, FM.field[x, y]);
                }
            }
        }

        //FMに消えた数
        num = vanishNum;
        colorNum = 0;
        for (int i = 0; i < 5; i++) if (colorFrag[i]) colorNum++;

        return chainFrag;
    }


    void Variable0Fill() {
        for (int y = 0; y < FM.CH; y++) {
            for (int x = 0; x < FM.W; x++) {
                checkFrag[x, y] = false;
                connectNum[x, y] = 0;
            }
        }
        for (int i = 0; i < 5; i++) colorFrag[i] = false;
        vanishNum = 0;
    }

    int ConnectCheck(int u, int v, int col) {
        connectNum[u, v] = 1;
        checkFrag[u, v] = true;
        if ((v + 1) < FM.CH) {
            if ((FM.field[u, v + 1] == col) && (checkFrag[u, v + 1] == false)) {
                connectNum[u, v] += ConnectCheck(u, v + 1, col);
            }
        }
        if ((v - 1) > -1) {
            if ((FM.field[u, v - 1] == col) && (checkFrag[u, v - 1] == false)) {
                connectNum[u, v] += ConnectCheck(u, v - 1, col);
            }
        }
        if ((u + 1) < FM.W) {
            if ((FM.field[u + 1, v] == col) && (checkFrag[u + 1, v] == false)) {
                connectNum[u, v] += ConnectCheck(u + 1, v, col);
            }
        }
        if ((u - 1) > -1) {
            if ((FM.field[u - 1, v] == col) && (checkFrag[u - 1, v] == false)) {
                connectNum[u, v] += ConnectCheck(u - 1, v, col);
            }
        }
        return connectNum[u, v];
    }

    void ChainVanish(int u, int v, int col) {
        vanishNum++;
        FM.field[u, v] = 0;
        FM.obje[u, v].GetComponent<PuyoController>().PuyoDestroy();
        FM.puyoList.Remove(FM.obje[u, v]);

        if ((v + 1) < FM.CH) {
            if (FM.field[u, v + 1] == col) {
                ChainVanish(u, v + 1, col);
            }
        }
        if ((v - 1) >= 0) {
            if (FM.field[u, v - 1] == col) {
                ChainVanish(u, v - 1, col);
            }
        }
        if ((u + 1) < FM.W) {
            if (FM.field[u + 1, v] == col) {
                ChainVanish(u + 1, v, col);
            }
        }
        if ((u - 1) > -1) {
            if (FM.field[u - 1, v] == col) {
                ChainVanish(u - 1, v, col);
            }
        }
    }
}


