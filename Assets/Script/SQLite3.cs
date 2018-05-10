using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SQLite3 : MonoBehaviour {

    public SqliteDatabase sqlDB;
    string query;

    string DATABASE = "welcomerace.sqlite3";
    string TABLE = "score";


    void Awake()
    {
        sqlDB = new SqliteDatabase(DATABASE);
    }



    public void AddDataAndGetHiScore(int s, int c, string n,out int hs,out int hc,out bool update)
    {
        query = "select * from score where name = '" + n + "';";
        var dt = sqlDB.ExecuteQuery(query);


        //データがないなら
        if (dt.Rows.Count == 0) {
            //追加してリターン
            query = "insert into score values('" + n + "'," + s + "," + c + ",1);";
            sqlDB.ExecuteNonQuery(query);
            sqlDB.TransactionCommit();
            hs = s;hc = c;update = true; return;
        }

        //データがあるなら  元データのハイスコアと比べる
        int tmpScore = (int)dt.Rows[0]["hiscore"];
        int tmpChain = (int)dt.Rows[0]["hichain"];
        int playTime = (int)dt.Rows[0]["playtime"];
        if(s < tmpScore)
        {
            //元データのほうが大きいならプレイ回数1増やしてリターン
            query = "update score set playtime = " + (playTime + 1).ToString() + " where name = '" + n + "';";
            sqlDB.ExecuteNonQuery(query);
            sqlDB.TransactionCommit();
            hs = tmpScore;hc = tmpChain;update = false; return;
        }
        //新しいほうが高いなら
        query = "update score set hiscore = " + s.ToString() 
                            + ",hichain = " + c.ToString()
                            + ",playtime = " + (playTime + 1).ToString()
                            + " where name = '" + n + "';";
        sqlDB.ExecuteNonQuery(query);
        sqlDB.TransactionCommit();
        hs = s; hc = c; update = true; return;
    }

    public void GetTop3(out string[] n, out int[] s, out int[] c)
    {
        Debug.Log("top3");
        n = new string[3]; s = new int[3]; c = new int[3];
        query = "select * from score order by hiscore desc";
        var dt = sqlDB.ExecuteQuery(query);

        for (int i = 0; i < 3; i++)
        {
            if (dt.Rows.Count <= i)
            {
                n[i] = null; s[i] = 0; c[i] = 0;
            }
            else
            {
                n[i] = (string)dt.Rows[i]["name"];
                s[i] = (int)dt.Rows[i]["hiscore"];
                c[i] = (int)dt.Rows[i]["hichain"];
            }
        }
    }

}



