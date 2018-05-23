using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    //------------------------------------------------------------- 
    //! @name	画面の位置関係. 
    //------------------------------------------------------------- 
    //@{ 
    public enum ALIGN {
        CENTER = 0, //!< 中央寄せ. 
        UP,         //!< 上寄せ. 
        DOWN,       //!< 下寄せ. 
        LEFT,       //!< 左寄せ. 
        RIGTH,      //!< 右寄せ. 
    };
    //@} 

    //------------------------------------------------------------- 
    //! @name	メンバー変数. 
    //------------------------------------------------------------- 
    //@{ 
    public float baseHeight = 1920.0f;           //!< 基準となるスクリーン高さ. 
    public float baseWidth = 1080.0f;            //!< 基準となるスクリーン幅. 
    public ALIGN screenAligh = ALIGN.CENTER;    //!< 画面寄せ 
                                                //@} 

    //------------------------------------------------------------- 
    //! 開始. 
    //------------------------------------------------------------- 
    void Start() {
        Camera camera = gameObject.GetComponent<Camera>();
        float baseAspect = baseHeight / baseWidth;
        float nowAspect = (float)Screen.height / (float)Screen.width;
        float changeAspect;

        if (baseAspect > nowAspect) {
            // 横基準 
            changeAspect = nowAspect / baseAspect;

            switch (screenAligh) {
                case ALIGN.CENTER:
                    camera.rect = new Rect((1 - changeAspect) * 0.5f, 0, changeAspect, 1);
                    break;
                case ALIGN.LEFT:
                    camera.rect = new Rect((1 - changeAspect) * 0.0f, 0, changeAspect, 1);
                    break;
                case ALIGN.RIGTH:
                    camera.rect = new Rect((1 - changeAspect) * 1.0f, 0, changeAspect, 1);
                    break;
            }
        }
        else {
            // 縦基準 
            changeAspect = baseAspect / nowAspect;

            switch (screenAligh) {
                case ALIGN.CENTER:
                    camera.rect = new Rect(0, (1 - changeAspect) * 0.5f, 1, changeAspect);
                    break;
                case ALIGN.UP:
                    camera.rect = new Rect(0, (1 - changeAspect) * 1.0f, 1, changeAspect);
                    break;
                case ALIGN.DOWN:
                    camera.rect = new Rect(0, (1 - changeAspect) * 0.0f, 1, changeAspect);
                    break;
            }
        }
    }
}