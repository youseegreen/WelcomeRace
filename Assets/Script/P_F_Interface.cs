using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class P_F_Interface : MonoBehaviour {

    private FieldManager FM;
    private ScoreAndTimer SAT;
    private GameObject OlgaObje;    //音楽もここにいれる
    public AudioClip audioClip;     //   private GameObject WishObje;
    private AudioSource AS;

    public enum ActionName {
        Def, Wish, Olga
    }
    private class InputInfo {
        public int x; public int y; public ActionName name;
        public InputInfo(int a, int b, ActionName c) { x = a; y = b; name = c; }
    }
    Queue<InputInfo> queue = new Queue<InputInfo>();

    // Use this for initialization
    void Start() {
        AS = GetComponent<AudioSource>();
        AS.clip = audioClip;
        FM = GetComponent<FieldManager>();
        SAT = GetComponent<ScoreAndTimer>();
        OlgaObje = GameObject.Find("OlgaImage");
        OlgaObje.GetComponent<Image>().enabled = false;
        //        WishObje = GameObject.Find("WishImage");
        //       WishObje.GetComponent<Image>().enabled = false;
        queue.Clear();
    }

    public void SetAction(int x, int y, ActionName type = ActionName.Def) {
        if ((x < 0) || (x >= FM.W)) return;
        queue.Enqueue(new InputInfo(x, y, type));
    }

    //スタックつくってそこから攻撃取り出して消していく感じかな
    public void Action() {
        while (queue.Count > 0) {
            InputInfo input = queue.Dequeue();

            if (input.name == ActionName.Def) {
                if (FM.field[input.x, input.y] != 0) {
                    FM.field[input.x, input.y] = 0;
                    FM.obje[input.x, input.y].GetComponent<PuyoController>().PuyoDestroy();
                    FM.puyoList.Remove(FM.obje[input.x, input.y]);
                    SAT.AddScore();
                }
            }

            if (input.name == ActionName.Wish)   //ウィッシュ　ある色全部消す
            {
                //                StartCoroutine(DispWish());

                int[] counter = new int[5];
                for (int i = 0; i < 5; i++) counter[i] = 0;
                for (int y = 0; y < FM.CH; y++) {
                    for (int x = 0; x < FM.W; x++) {
                        if (FM.field[x, y] == 0) continue;
                        counter[FM.field[x, y] - 1]++;
                    }
                }
                int max = counter[0];
                int maxNum = 0;
                for (int i = 1; i < 5; i++) {
                    if (max < counter[i]) {
                        max = counter[i];
                        maxNum = i;
                    }
                }

                int vanishColor = maxNum + 1;
                int vanishNum = 0;

                for (int y = 0; y < FM.CH; y++) {
                    for (int x = 0; x < FM.W; x++) {
                        if (FM.field[x, y] == vanishColor) {
                            vanishNum++;
                            FM.field[x, y] = 0;
                            FM.obje[x, y].GetComponent<PuyoController>().PuyoDestroy();
                            FM.puyoList.Remove(FM.obje[x, y]);
                        }
                    }
                }
                if (vanishNum != 0) GetComponent<ScoreAndTimer>().AddChain(vanishNum, 1);
                //一つも消せなかったらボーナス戻しておく
                else GameObject.Find("Player").GetComponent<ActionJudge>().AddBonus();
            }

            if (input.name == ActionName.Olga)   //オルガ　時をとめる
            {
                StartCoroutine(DispOlga());
            }
        }
    }

    private IEnumerator DispOlga() {
        OlgaObje.GetComponent<Image>().enabled = true;
        SAT.TimeUpdate = false;
        SAT.AddChain(0, 1);     //連鎖数だけ増やす
        AS.Play();
        yield return new WaitForSeconds(5.0f);  //10秒待つ

        OlgaObje.GetComponent<Image>().enabled = false;
        SAT.TimeUpdate = true;
    }


    private IEnumerator DispWish() {
        //       WishObje.GetComponent<Image>().enabled = true;
        yield return new WaitForSeconds(1.0f);  //10秒待つ
                                                //        WishObje.GetComponent<Image>().enabled = false;
    }
}