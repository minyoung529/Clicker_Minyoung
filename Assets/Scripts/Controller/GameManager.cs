using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField]
    private User user = null;

    private string SAVE_PATH = "";
    private readonly string SAVE_FILENAME = "/SaveFile.txt";

    private void Start()
    {
        SAVE_PATH = Application.dataPath + "/Save";
        //Application.persistentDataPath (���߿� �ȵ���̵�)
        if(!Directory.Exists(SAVE_PATH))
        {
            Directory.CreateDirectory(SAVE_PATH);
        }

        LoadFromJson();
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.A))
        {
            for(int i = 0; i<user.soldierList.Count; i++)
            {
                user.soldierList[i].amount++;
            }
        }
    }

    private void LoadFromJson()
    {
        string json = "";

        if(File.Exists(SAVE_PATH + SAVE_FILENAME))
        {
            Debug.Log("Load");

            json = File.ReadAllText(SAVE_PATH + SAVE_FILENAME);
            user = JsonUtility.FromJson<User>(json);
        }

        else
        {
            Debug.Log("Save, Load");
            SaveToJson();
            LoadFromJson();
        }
    }

    private void SaveToJson()
    {
        Debug.Log("SaveToJson");

        SAVE_PATH = Application.dataPath + "/Save";

        if (user == null) return;
        string json = JsonUtility.ToJson(user, true);
        File.WriteAllText(SAVE_PATH + SAVE_FILENAME, json, System.Text.Encoding.UTF8);
    }

    private void OnApplicationPause(bool pause)
    {
        //SaveToJson();
    }

    private void OnApplicationQuit()
    {
        SaveToJson();
    }

    //�ٸ� �͵�(GameObject, Transform)�� �����Ϸ��� Json �÷�����
    //���ϴ� �� ����
}
