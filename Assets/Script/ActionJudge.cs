using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//シーンマネジメントを有効にする

public class ActionJudge : MonoBehaviour {

    private GameObject FC;   //フィールドコントローラ
    private P_F_Interface PFI;   //プレイヤーフィールドインターフェース
    private AvatarController AC; //アバターコントローラ

    private static bool startPos = true;
    private Transform[] bone = new Transform[22];
    private int bonusNum = 0;
    private bool startPose = false;
    public bool StartPose {
        set { startPose = value; }
        get { return startPose; }
    }
    private bool enable = false;
    public bool Frag {
        set { enable = value; }
        get { return enable; }
    }
    private string raiseFoot = "";
    public string RaiseFoot {
        get { return raiseFoot; }
    }
    public int BonusNum { get { return bonusNum; } }
   


    // Use this for initialization
    void Start() {
        DontDestroyOnLoad(GameObject.Find("parent"));
//        FC = GameObject.Find("FieldController");
//        PFI = FC.GetComponent<P_F_Interface>();
        AC = GetComponent<AvatarController>();

        //はじめは判定起こさない
        // enabled = false;
    }

    public void GetMainSceneObject() {
        bonusNum = 0;
        FC = GameObject.Find("FieldController");
        PFI = FC.GetComponent<P_F_Interface>();
        enable = false;
    }
    public void GetStartSceneObject() {
        startPose = false;
        FC = null;
        PFI = null;
    }

    private int playerX {
        get { return (int)(GetComponent<Transform>().position.x + 0.5); }
    }

    // Update is called once per frame
    void Update() {
        /*全部ACつけるんめんどいしコピっとく*/
        for (int i = 0; i < 22; i++) bone[i] = AC.Bones[i];
        if (!CheckStartPos()) return;

        /*スタートシーンだけでの処理  OlgaならStartPoseをtrue*/
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Start")) {
            if (!startPose && IsOlga()) startPose = true;
            if (IsRaiseRightFoot()) raiseFoot = "right";
            if (IsRaiseLeftFoot()) raiseFoot = "left";
        }

        /*メインシーンだけでの処理（ぷよを消すように依頼）*/
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Main")) {
            if (!Frag) return;
            //技確認の順番気を付けていけ
            if (IsOlga() && bonusNum > 0) { PFI.SetAction(playerX, 0, P_F_Interface.ActionName.Olga); bonusNum--; }
            if (IsWish() && bonusNum > 0) { PFI.SetAction(playerX, 0, P_F_Interface.ActionName.Wish); bonusNum--; }
            if (IsNeedle()) PFI.SetAction(playerX, 2);
            if (IsKick()) PFI.SetAction(playerX, 0);
            if (IsPunch()) PFI.SetAction(playerX, 1);
        }
    }


    private bool CheckStartPos() {
        if (bone[6].position.y < bone[5].position.y)
            if (bone[11].position.y < bone[10].position.y)
                if (Mathf.Abs(bone[7].position.z - bone[5].position.z) < 0.2)
                    if (Mathf.Abs(bone[12].position.z - bone[10].position.z) < 0.2)
                        if (bone[19].position.y < bone[18].position.y)
                            if (bone[15].position.y < bone[14].position.y)
                                if (Mathf.Abs(bone[14].position.z - bone[15].position.z) < 0.3)
                                    if (Mathf.Abs(bone[18].position.z - bone[19].position.z) < 0.3)
                                        startPos = true;
        return startPos;
    }



    private bool IsNeedle() {

        if (bone[6].position.y < bone[5].position.y) return false;
        if (bone[7].position.y < bone[6].position.y) return false;
        if (bone[8].position.y < bone[7].position.y) return false;
        if (bone[8].position.y < bone[3].position.y) return false;
        if (bone[11].position.y < bone[10].position.y) return false;
        if (bone[12].position.y < bone[11].position.y) return false;
        if (bone[13].position.y < bone[12].position.y) return false;
        if (bone[13].position.y < bone[3].position.y) return false;

        if (Mathf.Abs(bone[6].position.x - bone[7].position.x) > 0.3) return false;
        if (Mathf.Abs(bone[7].position.x - bone[8].position.x) > 0.3) return false;
        if (Mathf.Abs(bone[11].position.x - bone[12].position.x) > 0.3) return false;
        if (Mathf.Abs(bone[12].position.x - bone[13].position.x) > 0.3) return false;

        startPos = false;
        return true;
    }

    private bool IsKick() {

        if (Mathf.Abs(bone[14].position.z - bone[15].position.z) > 0.3) { startPos = false; return true; }
        if (Mathf.Abs(bone[18].position.z - bone[19].position.z) > 0.3) { startPos = false; return true; }
        return false;
    }

    private bool IsRaiseRightFoot() {
     //   if (Mathf.Abs(bone[14].position.z - bone[15].position.z) > 0.3) { startPos = false; return true; }
        if (Mathf.Abs(bone[18].position.z - bone[19].position.z) > 0.3) { startPos = false; return true; }
        return false;
    }

    private bool IsRaiseLeftFoot() {
        if (Mathf.Abs(bone[14].position.z - bone[15].position.z) > 0.3) { startPos = false; return true; }
    //    if (Mathf.Abs(bone[18].position.z - bone[19].position.z) > 0.3) { startPos = false; return true; }
        return false;
    }

    private bool IsPunch() {
        if (Mathf.Abs(bone[10].position.z - bone[11].position.z) > 0.17)
            if (Mathf.Abs(bone[11].position.z - bone[12].position.z) > 0.17)
                if (Mathf.Abs(bone[10].position.x - bone[11].position.x) < 0.25)
                    if (Mathf.Abs(bone[11].position.x - bone[12].position.x) < 0.25)
                        if (Mathf.Abs(bone[5].position.z - bone[6].position.z) < 0.17)
                            if (Mathf.Abs(bone[6].position.z - bone[7].position.z) < 0.17) { startPos = false; return true; }


        if (Mathf.Abs(bone[5].position.z - bone[6].position.z) > 0.17)
            if (Mathf.Abs(bone[6].position.z - bone[7].position.z) > 0.17)
                if (Mathf.Abs(bone[5].position.x - bone[6].position.x) < 0.25)
                    if (Mathf.Abs(bone[6].position.x - bone[7].position.x) < 0.25)
                        if (Mathf.Abs(bone[10].position.z - bone[11].position.z) < 0.17)
                            if (Mathf.Abs(bone[11].position.z - bone[12].position.z) < 0.17) { startPos = false; return true; }

        return false;
    }

    private bool IsOlga() {
        if (bone[6].position.y < bone[5].position.y) return false;
        if (bone[7].position.y < bone[6].position.y) return false;
        if (bone[8].position.y < bone[7].position.y) return false;
        if (bone[8].position.y < bone[3].position.y) return false;

        if (bone[11].position.y > bone[10].position.y) return false;
        if (bone[12].position.y > bone[11].position.y) return false;
        if (bone[13].position.y > bone[12].position.y) return false;
        if (bone[13].position.y > bone[3].position.y) return false;

        startPos = false;
        return true;
    }

    private bool IsWish() {
        if (bone[6].position.y > bone[5].position.y) return false;
 //       if (bone[6].position.y > bone[7].position.y) return false;
        if (bone[6].position.y > bone[8].position.y) return false;
        if (bone[11].position.y > bone[10].position.y) return false;
 //       if (bone[11].position.y > bone[12].position.y) return false;
        if (bone[11].position.y > bone[13].position.y) return false;

        bool sign = true;
        if (bone[5].position.x > bone[10].position.x) sign = false;

        if (sign) {
            //デフォが右のほうが座標が大きい
            if (bone[8].position.x < bone[13].position.x) return false;
            if (bone[7].position.x < bone[13].position.x) return false;
        }
        else {
            if (bone[8].position.x > bone[13].position.x) return false;
            if (bone[7].position.x > bone[13].position.x) return false;
        }
        if (Mathf.Abs(bone[8].position.x - bone[10].position.x) > 0.2) return false;
        if (Mathf.Abs(bone[5].position.x - bone[13].position.x) > 0.2) return false;
        if (Mathf.Abs(bone[8].position.y - bone[10].position.y) > 0.2) return false;
        if (Mathf.Abs(bone[5].position.y - bone[13].position.y) > 0.2) return false;

        startPos = false;
        return true;
    }


    public void AddBonus() {
        bonusNum++;
    }

}
