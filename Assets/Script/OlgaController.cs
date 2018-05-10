using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class OlgaController : MonoBehaviour
{

    private void Start()
    {
        GetComponent<Image>().enabled = false;
    }

    private float time = 0.0f;
    private const float dispTime = 1.0f;


    // スライドイン（Pauseボタンが押されたときに、これを呼ぶ）
    public void SlideIn()
    {
        StartCoroutine(DispOlga());
    }

    private IEnumerator DispOlga()
    {
        GetComponent<Image>().enabled = true;
   //     transform.localPosition = new Vector3(640, 1453, 0);
        yield return new WaitForSeconds(1.5f);  //1秒待つ
    //    transform.position = new Vector3(0, -2800, 0);
        GetComponent<Image>().enabled = false;
    }

}