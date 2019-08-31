using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [HideInInspector]
    public enum GeneratingStatus
    {
        Initiating = 0,         //初始化
        Selecting = 1,          //從Prefab資料夾中選出若干個關卡並依據關卡大小決定放置順序
        Place_Levels = 2,       //放置關卡
        Construct_Links = 3,    //決定關卡之間的連結
        Find_Paths = 4,         //根據所給的連結找路徑(大概率用A*)
        Build_Paths = 5,        //生成路徑
        Final_Makeup = 6        //把沒用到的接口補起來
    }
    [Header("關卡數量限制")]
    public static int minMap = 4, maxMap = 7;
    [Header("地圖邊界限制")]
    public static int mapX = 75, mapY = 75;
    [HideInInspector]
    public int[,] map = new int[mapX, mapY];
    [HideInInspector]
    public GeneratingStatus currentStatus = GeneratingStatus.Initiating;
    private GameObject[] levels, paths, lvToPut;
    private Level[] lvInform;

    void Start()
    {
        levels = Resources.LoadAll<GameObject>("Prefab(LC)/SubLevels");
        paths = Resources.LoadAll<GameObject>("Prefab(LC)/Paths");
        for (int i = 0; i < mapY; i++)
            for (int j = 0; j < mapX; j++)
                map[i,j] = 0;
        Selecting();
    }

    private void Selecting()    //步驟1
    {
        currentStatus = GeneratingStatus.Selecting;
        /*此段以上負責狀態更改*/

        int len = levels.Length;
        for (int i = 0; i < len; i++)
        {
            int rand = UnityEngine.Random.Range(0, levels.Length - 1);
            GameObject newGO = levels[i];
            levels[i] = levels[rand];
            levels[rand] = newGO;
        }
        /*此段以上負責洗牌*/

        int lvCount = UnityEngine.Random.Range(minMap, maxMap);
        int j = 0;
        lvToPut = new GameObject[lvCount];
        lvInform = new Level[lvCount];
        for (int i = 0; i < len; i++)
        {
            float rand = UnityEngine.Random.Range(0f, 1f);
            if(rand <= (float)lvCount / ((float)len - (float)i))
            {
                lvToPut[j] = levels[i];
                lvInform[j] = lvToPut[j].GetComponent<Level>();
                lvCount--;
                j++;
            }
            if (lvCount == 0)
                break;
        }
        /*此段以上負責抽關卡*/

        GameObject[] temp = lvToPut;

        /*此段以上負責排序*/
    }

    private Vector3 Index2Pos(Vector2 index, int subLvX, int subLvY)
    {
        return new Vector3((index.x - Mathf.FloorToInt((float)mapX / 2f) + 0.5f * (float)subLvX) * 4, -(index.y - Mathf.FloorToInt((float)mapY / 2f) + 0.5f * (float)subLvY) * 4, 0); 
    }
}
