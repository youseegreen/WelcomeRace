using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuyoController : MonoBehaviour {

    private PuyoGenerator PG;
    private GameObject player;

    public const float thresholdTime = 0.5f;
    private bool destroyFrag = false;
    private bool materialChangeFrag = false;
    private float time = 0.0f;
    private int colorNum = 0;
   
    private bool bonusFrag = false;
    public bool BonusFrag
    {
        get { return bonusFrag; }
        set { bonusFrag = value; }
    }
    

    public int ColorNum{
        get { return colorNum; }
    }

    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        PG = GameObject.Find("FieldController").GetComponent<PuyoGenerator>();
        if (gameObject.CompareTag("Red")) { GetComponent<Renderer>().material = GameObject.Find("RedSample").GetComponent<Renderer>().material; colorNum = 1; }
        if (gameObject.CompareTag("Green")) { GetComponent<Renderer>().material = GameObject.Find("GreenSample").GetComponent<Renderer>().material; colorNum = 2; }
        if (gameObject.CompareTag("Blue")) { GetComponent<Renderer>().material = GameObject.Find("BlueSample").GetComponent<Renderer>().material; colorNum = 3; }
        if (gameObject.CompareTag("Yellow")) { GetComponent<Renderer>().material = GameObject.Find("YellowSample").GetComponent<Renderer>().material; colorNum = 4; }
        if (gameObject.CompareTag("Purple")) { GetComponent<Renderer>().material = GameObject.Find("PurpleSample").GetComponent<Renderer>().material; colorNum = 5; }
        if (gameObject.CompareTag("No")) { Destroy(GetComponent<Renderer>()); colorNum = 0; }
    }
	
	// Update is called once per frame
	void Update () {
        if (bonusFrag)
        {
            if (gameObject.CompareTag("Red"))  GetComponent<Renderer>().material = GameObject.Find("RedBonus").GetComponent<Renderer>().material; 
            if (gameObject.CompareTag("Green"))  GetComponent<Renderer>().material = GameObject.Find("GreenBonus").GetComponent<Renderer>().material; 
            if (gameObject.CompareTag("Blue"))  GetComponent<Renderer>().material = GameObject.Find("BlueBonus").GetComponent<Renderer>().material; 
            if (gameObject.CompareTag("Yellow"))  GetComponent<Renderer>().material = GameObject.Find("YellowBonus").GetComponent<Renderer>().material; 
            if (gameObject.CompareTag("Purple"))  GetComponent<Renderer>().material = GameObject.Find("PurpleBonus").GetComponent<Renderer>().material; 
        }

        if (destroyFrag)
        {
            if (!materialChangeFrag) {
                GetComponent<Rigidbody>().isKinematic = true;   //ついでに物理演算も切っておく
                materialChangeFrag = true;
                if (gameObject.CompareTag("Red")) GetComponent<Renderer>().material = GameObject.Find("RedSample2").GetComponent<Renderer>().material;
                if (gameObject.CompareTag("Green")) GetComponent<Renderer>().material = GameObject.Find("GreenSample2").GetComponent<Renderer>().material;
                if (gameObject.CompareTag("Blue")) GetComponent<Renderer>().material = GameObject.Find("BlueSample2").GetComponent<Renderer>().material;
                if (gameObject.CompareTag("Yellow")) GetComponent<Renderer>().material = GameObject.Find("YellowSample2").GetComponent<Renderer>().material;
                if (gameObject.CompareTag("Purple")) GetComponent<Renderer>().material = GameObject.Find("PurpleSample2").GetComponent<Renderer>().material;
            }
            time += Time.deltaTime;
            if(time > thresholdTime)
            {
                if (BonusFrag) player.GetComponent<ActionJudge>().AddBonus();
                Destroy(gameObject);
            }
        }
	}

    //デストロイ申請されたらその上に色なしぷよをだす
    public void PuyoDestroy() {
        PG.GenerateNoPuyo(transform);
        destroyFrag = true;
    }

    public bool IsMove()
    {
        if (destroyFrag) return true;   //多分呼ばれることはないけど一応
        if (Mathf.Abs(GetComponent<Rigidbody>().velocity.y) < 0.05)
        {
            //     int y = (int)(transform.position.y + 0.1);
            //    if (Mathf.Abs(y - transform.position.y) < 0.1) return false;
            return false;
        }
        return true;
    }
}
