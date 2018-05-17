using UnityEngine;
using UnityEngine.SceneManagement;


// Awake前にManagerSceneを自動でロードするクラス

public class StartUpManager :MonoBehaviour{
    private void Start() {
        SceneManager.LoadScene("Start");//Mainシーンをロードする
    }
}