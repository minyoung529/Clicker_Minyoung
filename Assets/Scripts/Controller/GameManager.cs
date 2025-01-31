using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField]
    private User user;
    public User CurrentUser { get { return user; } }

    private string SAVE_PATH = "";
    private readonly string SAVE_FILENAME = "/SaveFile.txt";

    public ulong cutletMoney { get; private set; } = 100;
    public int maxCutletCnt { get; private set; } = 10;
    public ulong mpsMoney { get; private set; } = 0;

    public UIManager UIManager { get; private set; }
    public QuestManager QuestManager { get; private set; }
    public TutorialManager TutorialManager { get; private set; }
    public bool isGoldCutlet { get; private set; }
    public bool isGameStart;


    [SerializeField] private Transform poolTransform;
    public Transform Pool { get { return poolTransform; } }

    public void Quit()
    {
        Application.Quit();
    }

    private void Awake()
    {

        SAVE_PATH = Application.persistentDataPath + "/Save";
        //SAVE_PATH = Application.dataPath + "/Save";
        if (!Directory.Exists(SAVE_PATH))
        {
            Directory.CreateDirectory(SAVE_PATH);
        }

        LoadFromJson();
    }

    public void Start()
    {
        UIManager = GetComponent<UIManager>();
        QuestManager = GetComponent<QuestManager>();
        TutorialManager = GetComponent<TutorialManager>();

        InvokeRepeating("EarnMoneyPerSecond", 0f, 1f);
        InvokeRepeating("SaveToJson", 1f, 60f);

        SetUser();
        SetCutletPrice();

        if (user.isTutorial)
        {
            Camera.main.transform.position = new Vector3(0f, 3f);
        }

        else
        {
            Camera.main.transform.position = new Vector3(3f, 0f);
        }
    }

    public void EarnMoneyPerSecond()
    {
        if (!isGameStart) return;

        mpsMoney = 0;

        foreach (PartTimer partTimer in user.partTimerList)
        {
            if (partTimer.GetIsSold())
                mpsMoney += (ulong)partTimer.mps * (ulong)Mathf.RoundToInt(partTimer.level);
        }

        if (isGoldCutlet)
            AddMoney(mpsMoney * 2, true);

        else
            AddMoney(mpsMoney, true);

        user.Quests[1].PlusCurValue(1);
        UIManager.UpdatePanel();
        UIManager.ActiveGoldCutlet();
    }

    public void SetCutletPrice()
    {
        cutletMoney = 100;

        foreach (Cutlet cutlet in user.cutlets)
        {
            if (cutlet.GetIsSold())
                cutletMoney += (ulong)cutlet.addMoney;
        }
    }

    private void LoadFromJson()
    {
        string json;

        if (File.Exists(SAVE_PATH + SAVE_FILENAME))
        {
            json = File.ReadAllText(SAVE_PATH + SAVE_FILENAME);
            user = JsonUtility.FromJson<User>(json);
        }

        else
        {
            user.hammerList[0].amount++;
            user.hammerList[0].SetIsSold(true);

            SaveToJson();
            LoadFromJson();
        }
    }

    private void SaveToJson()
    {
        SAVE_PATH = Application.persistentDataPath + "/Save";

        if (user == null) return;
        string json = JsonUtility.ToJson(user, true);
        File.WriteAllText(SAVE_PATH + SAVE_FILENAME, json, System.Text.Encoding.UTF8);
    }

    private void OnApplicationQuit()
    {
        SaveToJson();
    }

    public void AddMoney(ulong money, bool isAdd)
    {
        if (isAdd)
            CurrentUser.money += money;
        else
            CurrentUser.money -= money;

        UIManager.UpdatePanel();
    }

    public void AddDiamond(int diamond)
    {
        CurrentUser.diamond += diamond;
        UIManager.UpdatePanel();
    }

    private void SetUser()
    {
        float plus;

        foreach (Cutlet cutlet in user.cutlets)
        {
            if (!cutlet.GetIsSold())
            {
                plus = (cutlet.code > 0) ? 4390 * Mathf.Pow(cutlet.code, 1.2f) : 128;
                cutlet.SetPrice((ulong)Mathf.Round
                    (Mathf.Pow(cutlet.code + 3f, cutlet.code > 0 ? 0.8f * cutlet.code : 1)
                    * Mathf.Pow(cutlet.code + 3, 2f) + plus));

                cutlet.SetAddMoney(Mathf.RoundToInt(Mathf.Pow(cutlet.code + 2, 5)));

            }
        }

        foreach (PartTimer partTimer in user.partTimerList)
        {
            if (!partTimer.GetIsSold())
            {
                partTimer.SetPrice((ulong)Mathf.RoundToInt(Mathf.Pow(partTimer.code + 1, partTimer.code * 0.5f) + 9900
                    * Mathf.Pow(partTimer.code, partTimer.code * 0.5f) * partTimer.code + 9900));

                partTimer.SetMPS(Mathf.RoundToInt(partTimer.code * Mathf.Pow(partTimer.code + 1.5f, 4f) + 1));

                //=ROUND(POWER(E15+1,E15*0.2)+9900*POWER(E15+1,E15*0.3)*(E15*1.7) + 9900,0)
            }
        }
    }

    public bool RandomSelecting(float percentage)
    {
        float random = Random.Range(0, 100);
        if (percentage > random) return true;
        else return false;
    }

    public string ConvertMoneyText(ulong money)
    {
        string moneyStr = "";
        string[] unitStr = { "만", "억", "조", "경", "해", "자", "양" };
        ulong offset = 10000;

        if (money > offset)
        {
            for (int i = 4; i >= 0; i--)
            {
                if (money >= Mathf.Pow(offset, i + 1))
                {
                    moneyStr += string.Format("{0}{1} ", (int)(money / Mathf.Pow(offset, i + 1)), unitStr[i]);
                    money -= (ulong)((int)(money / Mathf.Pow(offset, i + 1)) * Mathf.Pow(offset, i + 1));
                }
            }
        }

        return string.Format("{0}{1}원", moneyStr, money);
    }

    private IEnumerator GoldCutlet()
    {
        isGoldCutlet = true;
        SoundManager.Instance.SetBGMPitch(1.25f);
        yield return new WaitForSeconds(20f);
        isGoldCutlet = false;
        SoundManager.Instance.SetBGMPitch(1f);
    }

    public void OnClickGoldCutlet()
    {
        StartCoroutine(GoldCutlet());
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}