using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LevelGenerator : MonoBehaviour
{
    [HideInInspector]
    public enum GeneratingStatus
    {
        Initiating = 0,         //初始化
        Selecting = 1,          //從Prefab資料夾中選出若干個關卡並依據關卡大小決定放置順序
        Place_Levels = 2,       //放置關卡
        Construct_Links = 3,    //決定關卡之間的連結
        Find_Paths = 4,         //根據所給的連結找路徑(用A*)
        Build_Paths = 5,        //生成路徑
        Final_Makeup = 6        //把沒用到的接口補起來
    }
    [Header("關卡數量限制"), Range(7,15)]
    public int minMap = 7, maxMap = 7;
    [Header("地圖邊界限制")]
    public static int mapX = 75, mapY = 75;
    [Header("關卡離散程度"), Range(1, 10)]
    public int divergence = 4;
    [Header("關卡離散程度"), Range(0, 9)]
    public int convergence = 0;
    [HideInInspector]
    public int[,] map = new int[mapX, mapY];
    [HideInInspector]
    public GeneratingStatus currentStatus = GeneratingStatus.Initiating;
    public GameObject StartingPlace;
    public GameObject[] paths, makeUp;
    private GameObject[] levels, lvToPut;
    private Level[] lvInform;      //儲存lvToPut內的Level class資料
    private Vector2Int[,] link;    //link存關卡間的連結，在linkConstructor Enque的時候查找是否有重複的連結，儲存的方式是用bit operation
    private Queue<Vector4> linkConstructor = new Queue<Vector4>();  //(a,b,c,d) => "從第a號關卡的第b號接口做一條路到第c號關卡的第d號接口"
    private Queue<Vector2Int> PathCoordinate = new Queue<Vector2Int>(); //負責儲存那些被標記成道路的座標
    private Stack<Vector4> checkDir = new Stack<Vector4>();     //(a,b,c,d)，a代表第幾個被生成的物件，b和c分別代表該物件的x,y座標，d代表要檢查的生成方向

    void Start()
    {
        levels = Resources.LoadAll<GameObject>("Prefab(LC)/SubLevels");
        for (int i = 0; i < mapY; i++)
            for (int j = 0; j < mapX; j++)
                map[i,j] = 0;
        Selecting();
    }

    private void Selecting() //步驟1
    {
        currentStatus = GeneratingStatus.Selecting;
        /*此段以上負責狀態更改*/

        int len = levels.Length;
        for (int i = 0; i < len; i++)
        {
            int rand = UnityEngine.Random.Range(0, len - 1);
            GameObject newGO = levels[i];
            levels[i] = levels[rand];
            levels[rand] = newGO;
        }
        /*此段以上負責洗牌*/

        int lvCount = UnityEngine.Random.Range(minMap, maxMap);
        lvToPut = new GameObject[lvCount + 1];
        lvInform = new Level[lvCount + 1];
        link = new Vector2Int[lvCount + 1, lvCount + 1];
        for (int i = 0; i < lvCount + 1; i++)
            for (int j = 0; j < lvCount + 1; j++)
                link[i, j] = Vector2Int.zero;
        lvToPut[0] = StartingPlace;
        lvInform[0] = StartingPlace.GetComponent<Level>();
        for (int i = 1; i < lvCount + 1; i++)
        {
            lvToPut[i] = levels[i];
            lvInform[i] = lvToPut[i].GetComponent<Level>();
        }
        /*此段以上負責抽關卡*/

        for (int i = 1; i < lvCount; i++)
        {
            for (int j = i; j >= 1; j--)
            {
                if (Mathf.Max(lvInform[j].X, lvInform[j].Y) > Mathf.Max(lvInform[j - 1].X, lvInform[j - 1].Y))
                {
                    GameObject tempGO = lvToPut[j];
                    lvToPut[j] = lvToPut[j - 1];
                    lvToPut[j - 1] = tempGO;
                    Level tempLV = lvInform[j];
                    lvInform[j] = lvInform[j - 1];
                    lvInform[j - 1] = tempLV;
                }
                else
                    break;
            }
        }
        /*此段以上負責排序(用selection sort，因為基數不大(< 100)，而且演算法in place且stable)*/

        PlaceLevels();
    }

    private void PlaceLevels() //步驟2
    {
        currentStatus = GeneratingStatus.Place_Levels;
        /*此段以上負責狀態更改*/

        int len = lvToPut.Length;
        Vector2Int startPos = new Vector2Int(UnityEngine.Random.Range(mapX / 2 - 5, mapX / 2 + 6), UnityEngine.Random.Range(mapY / 2 - 5, mapY / 2 + 6));
        PlaceLevelSetup(0, startPos);
        /*第一個被隨機生成的關卡*/
        
        for (int i = 1; i < len; i ++)
        {
            bool findPlace = false;
            while(findPlace == false)
            {
                Vector4 newDir = checkDir.Pop();
                for (int j = 0; j < 5; j++)
                {
                    Vector2Int newPos = PickPosition(new Vector2(newDir.y, newDir.z), lvInform[(int)newDir.x].X, lvInform[(int)newDir.x].Y, lvInform[i].X, lvInform[i].Y, (int)newDir.w);
                    if (map[newPos.x, newPos.y] == 0 && map[newPos.x + lvInform[i].X - 1, newPos.y] == 0 && map[newPos.x, newPos.y + lvInform[i].Y - 1] == 0 && map[newPos.x + lvInform[i].X - 1, newPos.y + lvInform[i].Y - 1] == 0)
                    {
                        PlaceLevelSetup(i, newPos);
                        int rand = UnityEngine.Random.Range(0, i);
                        while((lvInform[i].PosInArray - lvInform[rand].PosInArray).sqrMagnitude > 400)
                            rand = UnityEngine.Random.Range(0, i);
                        BuildLink(rand, i, false);
                        findPlace = true;
                        break;
                    }
                }
            }
        }
        /*此段以上負責生成關卡*/

        ConstructLinks();
    }

    private void ConstructLinks() //步驟3
    {
        currentStatus = GeneratingStatus.Construct_Links;
        /*此段以上負責狀態更改*/

        int len = lvToPut.Length;
        Vector2Int fromTo = new Vector2Int(-1, -1);
        for (int i = 0; i < len; i ++)
        {
            if (lvInform[i].connection == 1 && fromTo.x < 0)
                fromTo.x = i;
            else if (lvInform[i].connection == 1 && fromTo.y < 0)
                fromTo.y = i;
            if(fromTo.x >= 0 && fromTo.y >= 0)
            {
                BuildLink(fromTo.x, fromTo.y, true);
                fromTo = new Vector2Int(-1, -1);
            }
        }


        FindPaths();
    }

    private void FindPaths() //步驟4
    {
        currentStatus = GeneratingStatus.Find_Paths;
        Queue<Vector2Int> temp = new Queue<Vector2Int>();
        while(linkConstructor.Count != 0)
        {
            temp.Clear();
            Vector4 newLink = linkConstructor.Dequeue();
            Vector2Int from = BuildLinkInit(temp, ConnectionPos((int)newLink.x, (int)newLink.y), (int)newLink.x, (int)newLink.y);
            Vector2Int to = BuildLinkInit(temp, ConnectionPos((int)newLink.z, (int)newLink.w), (int)newLink.z, (int)newLink.w);
            int stepcount = 0;
            while (stepcount < 100 && from != to)
            {
                stepcount++;
                if (map[from.x, from.y] != 1)
                    PathCoordinate.Enqueue(from);
                temp.Enqueue(from);
                map[from.x, from.y] = 3;
                int upCost = int.MaxValue, downCost = int.MaxValue, leftCost = int.MaxValue, rightCost = int.MaxValue;
                if (from.y > 0 && map[from.x,from.y - 1] < 2 && DeadEnd(from.x,from.y - 1) == false)
                    upCost = (to - (from + new Vector2Int(0, -1))).sqrMagnitude;
                if (from.y < mapY - 1 && map[from.x, from.y + 1] < 2 && DeadEnd(from.x, from.y + 1) == false)
                    downCost = (to - (from + new Vector2Int(0, 1))).sqrMagnitude;
                if (from.x > 0 && map[from.x - 1, from.y] < 2 && DeadEnd(from.x - 1, from.y) == false)
                    leftCost = (to - (from + new Vector2Int(-1, 0))).sqrMagnitude;
                if (from.x < mapX - 1 && map[from.x + 1, from.y] < 2 && DeadEnd(from.x + 1, from.y) == false)
                    rightCost = (to - (from + new Vector2Int(1, 0))).sqrMagnitude;
                int bestCost = (int)Mathf.Min(upCost, downCost, leftCost, rightCost);
                if (upCost == bestCost)
                    from += new Vector2Int(0, -1);
                else if (downCost == bestCost)
                    from += new Vector2Int(0, 1);
                else if (leftCost == bestCost)
                    from += new Vector2Int(-1, 0);
                else if (rightCost == bestCost)
                    from += new Vector2Int(1, 0);
            }
            if(map[from.x,from.y] != 1)
                PathCoordinate.Enqueue(from);
            map[from.x, from.y] = 3;
            temp.Enqueue(from);
            foreach (Vector2Int vector in temp)
                map[vector.x, vector.y] = 1;
        }
        //linkConstructor(a,b,c,d) => "從第a號關卡的第b號接口做一條路到第c號關卡的第d號接口"
        string path = "Assets/Resources/Test.txt";
        StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Truncate));
        for (int i = 0; i < mapY; i++)
        {
            string str = "";
            for (int j = 0; j < mapX; j++)
            {
                if (map[j, i] >= 0)
                    str += map[j, i].ToString();
                else
                    str += "+";
            }
            str += "\n";
            writer.Write(str);
        }
        writer.Close();

        BuildPaths();
    }

    private void BuildPaths() //步驟5
    {
        currentStatus = GeneratingStatus.Build_Paths;
        while(PathCoordinate.Count != 0)
        {
            int spawnIndex = 0;
            Vector2Int Pos = PathCoordinate.Dequeue();
            if (Pos.y > 0 && map[Pos.x, Pos.y - 1] != 2 && map[Pos.x, Pos.y - 1] > 0)
                spawnIndex += 1;
            if (Pos.y < mapY - 1 && map[Pos.x, Pos.y + 1] != 2 && map[Pos.x, Pos.y + 1] > 0)
                spawnIndex += 2;
            if (Pos.x > 0 && map[Pos.x - 1, Pos.y] != 2 && map[Pos.x - 1, Pos.y] > 0)
                spawnIndex += 4;
            if (Pos.x < mapX - 1 && map[Pos.x + 1, Pos.y] != 2 && map[Pos.x + 1, Pos.y] > 0)
                spawnIndex += 8;
            Instantiate(paths[spawnIndex], Index2Pos(Pos, 1, 1), Quaternion.identity);
        }

        FinalMakeup();
    }

    private void FinalMakeup() //步驟6
    {
        currentStatus = GeneratingStatus.Final_Makeup;
        for (int i = 0; i < lvInform.Length; i++)
        {
            for (int j = 0; j < lvInform[i].directions.Length; j++)
            {
                int k = (int)lvInform[i].directions[j].direction;
                if (lvInform[i].directions[j].connectedAmount == 0)
                    Instantiate(makeUp[k], Index2Pos(ConnectionPos(i, j), 1, 1), Quaternion.identity);
            }
        }
    }

    private void PlaceLevelSetup(int index, Vector2Int PlacePos) //步驟2會用到
    {
        GameObject newGO = Instantiate(lvToPut[index], Index2Pos(PlacePos, lvInform[index].X, lvInform[index].Y), Quaternion.identity);
        lvToPut[index] = newGO;
        lvInform[index] = lvToPut[index].GetComponent<Level>();
        lvInform[index].PosInArray = PlacePos;
        for (int k = (int)Mathf.Max(0, PlacePos.x - 3); k < (int)Mathf.Min(mapX, PlacePos.x + lvInform[index].X - 1 + 4); k++)
        {
            for (int l = (int)Mathf.Max(0, PlacePos.y - 3); l < (int)Mathf.Min(mapY, PlacePos.y + lvInform[index].Y - 1 + 4); l++)
            {
                if(map[k, l] == 0)
                    map[k, l] = -1;
                if (k >= PlacePos.x - 1 && k <= PlacePos.x + lvInform[index].X && l >= PlacePos.y - 1 && l <= PlacePos.y + lvInform[index].Y)
                    map[k, l] = 2;
                if (k >= PlacePos.x && k <= PlacePos.x + lvInform[index].X - 1 && l >= PlacePos.y && l <= PlacePos.y + lvInform[index].Y - 1)
                    map[k, l] = 3;
            }
        }
        int[] Octo = { 0, 1, 2, 3, 4, 5, 6, 7 };
        for (int i = 0; i < 8; i++)
        {
            int rand = UnityEngine.Random.Range(0, 8);
            int temp = Octo[i];
            Octo[i] = Octo[rand];
            Octo[rand] = temp;
        }
        for (int i = 0; i < 8; i++)
            checkDir.Push(new Vector4(index, PlacePos.x, PlacePos.y, Octo[i]));
        /*Octo表八方位，關於數字和方向的對應去看底下的OctoDir enum*/
        /*Stack儲存Vector4(a,b,c,d)，a代表第幾個被生成的物件，b和c分別代表該物件的x,y座標，d代表要檢查的生成方向*/
    }

    private void BuildLink(int from, int to, bool pickSmallest) //步驟2，3會用到
    {
        int connectionA = UnityEngine.Random.Range(0, lvInform[from].directions.Length);
        int connectionB = UnityEngine.Random.Range(0, lvInform[to].directions.Length);
        if(pickSmallest == true)
        {
            int smallA = int.MaxValue;
            for (int i = 0; i < lvInform[from].directions.Length; i++)
                if (smallA > lvInform[from].directions[i].connectedAmount)
                    smallA = lvInform[from].directions[i].connectedAmount;
            int smallB = int.MaxValue;
            for (int i = 0; i < lvInform[to].directions.Length; i++)
                if (smallB > lvInform[to].directions[i].connectedAmount)
                    smallB = lvInform[to].directions[i].connectedAmount;
            while(lvInform[from].directions[connectionA].connectedAmount != smallA)
                connectionA = UnityEngine.Random.Range(0, lvInform[from].directions.Length);
            while (lvInform[to].directions[connectionB].connectedAmount != smallB)
                connectionB = UnityEngine.Random.Range(0, lvInform[to].directions.Length);
        }
        if (lvInform[from].directions[connectionA].connectedAmount == 0)
            lvInform[from].connection++;
        lvInform[from].directions[connectionA].connectedAmount++;
        if (lvInform[to].directions[connectionB].connectedAmount == 0)
            lvInform[to].connection++;
        lvInform[to].directions[connectionB].connectedAmount++;
        link[from, to] = new Vector2Int(link[from, to].x | (1 << connectionA), link[from, to].y | (1 << connectionB));
        link[to, from] = new Vector2Int(link[to, from].x | (1 << connectionB), link[to, from].y | (1 << connectionA));
        linkConstructor.Enqueue(new Vector4(from, connectionA, to, connectionB));
    }

    private bool DeadEnd(int x, int y) //步驟4會用到
    {
        bool isDead = true;
        if (x > 0 && map[x - 1, y] < 2)
            isDead = false;
        if (x < mapX - 1 && map[x + 1, y] < 2)
            isDead = false;
        if (y > 0 && map[x, y - 1] < 2)
            isDead = false;
        if (y < mapY - 1 && map[x, y + 1] < 2)
            isDead = false;
        return isDead;
    } 

    private Vector2Int BuildLinkInit(Queue<Vector2Int> temp, Vector2Int pos, int lvIndex, int dirIndex) //步驟4會用到
    {
        Vector2Int limit = new Vector2Int(0, 0);
        switch (lvInform[lvIndex].directions[dirIndex].direction)
        {
            case Direction.DIR.Up:
                limit = new Vector2Int(0, -1);
                break;
            case Direction.DIR.Down:
                limit = new Vector2Int(0, 1);
                break;
            case Direction.DIR.Left:
                limit = new Vector2Int(-1, 0);
                break;
            case Direction.DIR.Right:
                limit = new Vector2Int(1, 0);
                break;
        }
        if (map[pos.x, pos.y] != 1) 
            PathCoordinate.Enqueue(pos);
        map[pos.x, pos.y] = 3;
        temp.Enqueue(pos);
        return pos + limit;
    }

    private Vector2Int ConnectionPos(int levelIndex, int connectionIndex) //步驟4，6會用到
    {
        Vector2Int pos = new Vector2Int(-1, -1);
        Level currentLv = lvInform[levelIndex];
        Direction currentDir = currentLv.directions[connectionIndex];
        switch (currentDir.direction)
        {
            case Direction.DIR.Up:
                pos = new Vector2Int(currentLv.PosInArray.x + currentDir.position, currentLv.PosInArray.y - 1);
                break;
            case Direction.DIR.Down:
                pos = new Vector2Int(currentLv.PosInArray.x + currentDir.position, currentLv.PosInArray.y + currentLv.Y);
                break;
            case Direction.DIR.Left:
                pos = new Vector2Int(currentLv.PosInArray.x - 1, currentLv.PosInArray.y + currentDir.position);
                break;
            case Direction.DIR.Right:
                pos = new Vector2Int(currentLv.PosInArray.x + currentLv.X, currentLv.PosInArray.y + currentDir.position);
                break;
        }
        return pos;
    }

    enum OctoDir
    {
        U = 0,
        UL = 1,
        L = 2,
        DL = 3,
        D = 4,
        DR = 5,
        R = 6,
        UR = 7
    }

    private Vector2Int PickPosition(Vector2 index, int subLvX, int subLvY, int toPutX, int toPutY, int operation) //步驟2會用到
    {
        OctoDir dir = (OctoDir)operation;
        Vector2Int pos = new Vector2Int(-1,-1);
        int randX = UnityEngine.Random.Range(convergence, divergence),randY = UnityEngine.Random.Range(convergence, divergence);
        int randLvX = UnityEngine.Random.Range(-3, subLvX + 3), randLvY = UnityEngine.Random.Range(-3, subLvY + 3);
        switch (dir)
        {
            case OctoDir.U:
                pos = new Vector2Int((int)index.x + randLvX, (int)index.y - 4 - randY - toPutY + 1);
                break;
            case OctoDir.UL:
                pos = new Vector2Int((int)index.x - 4 - randX - toPutX + 1, (int)index.y - 4 - randY - toPutY + 1);
                break;
            case OctoDir.L:
                pos = new Vector2Int((int)index.x - 4 - randX - toPutX + 1, (int)index.y + randLvY);
                break;
            case OctoDir.DL:
                pos = new Vector2Int((int)index.x - 4 - randX - toPutX + 1, (int)index.y + subLvY - 1 + 4 + randY);
                break;
            case OctoDir.D:
                pos = new Vector2Int((int)index.x + randLvX, (int)index.y + subLvY - 1 + 4 + randY);
                break;
            case OctoDir.DR:
                pos = new Vector2Int((int)index.x + subLvX - 1 + 4 + randX, (int)index.y + subLvY - 1 + 4 + randY);
                break;
            case OctoDir.R:
                pos = new Vector2Int((int)index.x + subLvX - 1 + 4 + randX, (int)index.y + randLvY);
                break;
            case OctoDir.UR:
                pos = new Vector2Int((int)index.x + subLvX - 1 + 4 + randX, (int)index.y - 4 - randY);
                break;
        }
        pos = new Vector2Int((int)Mathf.Clamp(pos.x, 2, mapX - 3 - toPutX + 1), (int)Mathf.Clamp(pos.y, 2, mapY - 3 - toPutY + 1));
        return pos;
    }

    private Vector3 Index2Pos(Vector2 index, int subLvX, int subLvY) //步驟2，6會用到
    {
        return new Vector3((index.x - Mathf.FloorToInt((float)mapX / 2f) + 0.5f * (float)subLvX) * 4, -(index.y - Mathf.FloorToInt((float)mapY / 2f) + 0.5f * (float)subLvY) * 4, 0);
    }  
}
