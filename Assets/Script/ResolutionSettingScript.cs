//このコードをゲーム開始時に最初に読み込むスクリプトファイル（プレイヤー系やタイトル系など）に追記する。画面サイズはスクリプトをアタッチされたオブジェクトのインスペクター上で指定

using UnityEngine;
using System.Collections;
public class ResolutionSettingScript : MonoBehaviour { //xxxxにはスクリプト自体のファイル名が入る


    private int dispWidth;
    private int dispHeight;

    void Awake() {
        DontDestroyOnLoad(this);

        if (Screen.fullScreen) Screen.fullScreen = true;
        else {
            dispWidth = Screen.currentResolution.width;
            dispHeight = Screen.currentResolution.height;

            int height = dispHeight - 35;
            int width = height * 9 / 16;

            Screen.SetResolution(width, height, false);
        }
    }

    private void Update() {

        if (!Screen.fullScreen) {

            if (dispHeight != Screen.currentResolution.height) {
                dispWidth = Screen.currentResolution.width;
                dispHeight = Screen.currentResolution.height;

                int height = dispHeight - 35;
                int width = height * 9 / 16;

                Screen.SetResolution(width, height, false);
            }

        }
    }

}