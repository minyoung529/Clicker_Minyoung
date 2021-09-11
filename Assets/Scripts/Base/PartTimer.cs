using UnityEngine;

[System.Serializable]
public class PartTimer
{
    public string name;
    public string englishName;
    public int level;
    public ulong price;
    public int mps;
    public bool isSold;

    public void LevelUp()
    {
        level++;
        price += (ulong)Mathf.RoundToInt(Mathf.Pow(level - 1, 2) * Mathf.Pow(level, 1.15f) - 1 * level + 22);
        isSold = true;
    }

    public bool GetIsSold()
    {
        return isSold;
    }
}
