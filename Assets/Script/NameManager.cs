using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NameManager : MonoBehaviour {
    InputField inputField;

    void Start() {
        inputField = GetComponent<InputField>();
        InitInputField();
    }

    public void InputLogger() {
        //        string inputValue = inputField.text;
        //      Debug.Log(inputValue);
        //       GameObject.Find("GameManager").GetComponent<DataMessenger>().Name = inputField.text;
        //       InitInputField();
        //      GameObject.Find("DataBaseManager").GetComponent<SQLite3>().SearchName(inputValue);
        //        GetComponent<StartManager>().Frag = true;
    }

    public void GetName(out string name) {
        if (inputField.text != "") name = inputField.text;
        else name = "guest";
    }


    void InitInputField() {
        // 値をリセット
        inputField.text = "";
        // フォーカス
        inputField.ActivateInputField();
    }

}