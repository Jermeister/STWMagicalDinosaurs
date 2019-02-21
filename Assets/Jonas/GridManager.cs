﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GridManager : MonoBehaviour
{
	[Header("Player Settings")]
	public int playerId = 0;

	[Header("Grid Settings")]
	public int gridSize = 10;
	public GameObject tile;
	public float ySpawnValue = 0.3f;
	public GameObject selectionItem;
	public GameObject standItem; // Item that spawns when you select something

	[Header("Game Settings")]
	public List<GameObject> dinosaurPrefabs;
	public List<GameObject> obstaclePrefabs;
	public bool inGame = false;
	public bool inSetup = false;
	[Range(2,14)]
	public int maxObstaclesCount = 2;
	public float obstaclesSpawnY = 0.55f;
	[Header("Misc")]
	public bool inTiles = false;
	public int obstacleId = 10;
    public int monsterId = 1;

    //Dovydo
    public int selectedIndex;
    public Dinosaur selectedDino;

    [HideInInspector]
    public UIController uiController;


    //Private stuff
    private GameObject[,] Tiles;
	private TileScript[,] tileScripts;
	private int[,] TilePlayerMap;
	private int[,] TileTypeMap;
	private Transform parent;
	private GameObject selectionInstance;
	private GameObject standInstance;
	private List<GameObject> SpawnedObjects;
	private List<GameObject> SpawnedObstacles;

	// Start is called before the first frame update
	public void GirdManagerSetUp()
    {
		#region Instantiating objects
		// Instantiating objects
		Tiles = new GameObject[gridSize,gridSize];
		tileScripts = new TileScript[gridSize, gridSize];
        TileTypeMap = new int[gridSize,gridSize];
		TilePlayerMap = new int[gridSize, gridSize];
		SpawnedObjects = new List<GameObject>();
		SpawnedObstacles = new List<GameObject>();

		parent = this.transform;
		#endregion

		#region Instantiating tiles
		for (int x = 0;x < gridSize; x++)
		{
			for(int z = 0;z<gridSize; z++)
			{
				Tiles[x, z] = Instantiate(tile, new Vector3(x,ySpawnValue,z),Quaternion.identity,parent);

				TileTypeMap[x, z] = 0;

				tileScripts[x, z] = Tiles[x, z].GetComponent<TileScript>();
				if (inSetup)
				{
					playerId = GameObject.Find("Client(Clone)").GetComponent<Client>().isHost ? 1 : 2;

					if (playerId == 1 && x < 3)
					{
						TilePlayerMap[x, z] = playerId;
						tileScripts[x, z].playerType = playerId;
					}
					else if (playerId == 2 &&  x > 6)
					{
						TilePlayerMap[x, z] = playerId;
						tileScripts[x, z].playerType = playerId;
					}
					else
					{
						TilePlayerMap[x, z] = 0;
						tileScripts[x, z].playerType = 0;
					}
					tileScripts[x, z].updateTile();
				}
			}
		}
        #endregion

        //Dovydo
        uiController = GameObject.FindObjectOfType<UIController>();

		//if (inSetup)
			//SpawnRandomObstaclesOnGrid();
	}
    string t = "45|645|7!";

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.A))
        {
            pos[] posses = new pos[12];
            posses[0] = new pos(5, 5);
            posses[1] = new pos(5, 6);
            posses[2] = new pos(6, 5);
            posses[3] = new pos(4, 5);
            posses[4] = new pos(5, 4);
            posses[5] = new pos(6, 6);
            posses[6] = new pos(4, 4);
            posses[7] = new pos(6, 4);
            posses[8] = new pos(4, 6);
            posses[9] = new pos(3, 5);
            posses[10] = new pos(5, 3);
            posses[11] = new pos(7, 5);

           //ShowPossibleMoves(posses);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {

            t = BuildDinosString();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {

            DecodeDinosString(t);
        }




		UpdatePressObject();
		UpdateObjectSpawn();
		UpdateSelectionSquare();
    }

	public void StartGame()
	{
		inSetup = false;
		inGame = true;
	}

	public void StartSetup()
	{
		inGame = false;
		inSetup = true;
		SpawnRandomObstaclesOnGrid();
	}

	public void SpawnDinoButton(int id)
	{
		monsterId = id;
	}

    public row[] canAttackObjects, canMoveObjects;
    public List<pos> nowTargetable;
    public void ShowPossibleMoves(pos[] positions, pos origin)
    {
        nowTargetable.Clear();
        for (int i = 0; i < 10; i++) {
            for (int j = 0; j < 10; j++)
            {
                canMoveObjects[i].column[j].SetActive(false);
            }
        }
        for (int i = 0; i < positions.Length; i++)
        {
            if (positions[i].x + origin.x >= 0 && positions[i].x + origin.x <= 9 && positions[i].y + origin.y >= 0 && positions[i].y + origin.y <= 9 && TileTypeMap[positions[i].x + origin.x, positions[i].y + origin.y] == 0) {
               // if(positions[i].x == 1 && positions[i].y == 1 && TileTypeMap[positions[i].x + origin.x, positions[i].y + origin.y] == 0))
                canMoveObjects[positions[i].x + origin.x].column[positions[i].y + origin.y].SetActive(true);
                nowTargetable.Add(new pos(positions[i].x + origin.x, positions[i].y + origin.y));
            }
        }
    }

    public void ShowPossibleAttacks(pos[] positions, pos origin)
    {
        nowTargetable.Clear();
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                canAttackObjects[i].column[j].SetActive(false);
            }
        }
        for (int i = 0; i < positions.Length; i++)
        {
            if (positions[i].x + origin.x >= 0 && positions[i].x + origin.x <= 9 && positions[i].y + origin.y >= 0 && positions[i].y + origin.y <= 9)
            {
                canAttackObjects[positions[i].x + origin.x].column[positions[i].y + origin.y].SetActive(true);
                nowTargetable.Add(new pos(positions[i].x + origin.x, positions[i].y + origin.y));
            }
        }
    }

    void HidePossibleActions()
    {
        nowTargetable.Clear();
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                canAttackObjects[i].column[j].SetActive(false);
                canMoveObjects[i].column[j].SetActive(false);
            }
        }
        
    }

    /// <summary>
    /// Placing dinosaur in a grid
    /// </summary>
    void PlaceObjectNear(Vector3 clickPoint)
	{
		var finalPosition = GetNearestPointOnGrid(clickPoint);

		int xCount = Mathf.RoundToInt(finalPosition.x);
		int zCount = Mathf.RoundToInt(finalPosition.z);

		Quaternion rot;
		if (playerId == 1)
			rot = Quaternion.Euler(0, 90, 0);
		else
			rot = Quaternion.Euler(0, -90, 0);

        if (inTiles && TileTypeMap[xCount, zCount] == 0 && TilePlayerMap[xCount, zCount] == playerId && uiController.dinosAre[monsterId - 1] < uiController.maxAmount_Dino[monsterId - 1] && uiController.dinoButtons[monsterId - 1].cost <= uiController.money)
		{
			SpawnedObjects.Add(Instantiate(dinosaurPrefabs[monsterId - 1], new Vector3(xCount, 0.75f, zCount), rot));
            SpawnedObjects[SpawnedObjects.Count - 1].GetComponent<Dinosaur>().tileX = xCount;
            SpawnedObjects[SpawnedObjects.Count - 1].GetComponent<Dinosaur>().tileZ = zCount;
            SpawnedObjects[SpawnedObjects.Count - 1].GetComponent<Dinosaur>().id = monsterId;
            uiController.Bought(monsterId - 1);
            uiController.dinosAre[monsterId - 1]++;
			Destroy(selectionInstance);
			TileTypeMap[xCount, zCount] = monsterId;
			TilePlayerMap[xCount, zCount] = playerId;
		}
	}

    public void MultiplayerDinoMove(int index, pos originPos, pos targetPos)
    {
        SpawnedObjects[index].transform.position = canMoveObjects[targetPos.x].column[targetPos.y].transform.position + new Vector3(0f, 0.17f, 0f);
        SpawnedObjects[index].GetComponent<Dinosaur>().tileX = targetPos.x;
        SpawnedObjects[index].GetComponent<Dinosaur>().tileZ = targetPos.y;

        TileTypeMap[originPos.x, originPos.y] = 0;
        TileTypeMap[targetPos.x, targetPos.y] = SpawnedObjects[index].GetComponent<Dinosaur>().id;

        TilePlayerMap[targetPos.x, targetPos.y] = TilePlayerMap[originPos.x, originPos.y];
        TilePlayerMap[originPos.x, originPos.y] = 0;
        
    }


    public string BuildObstaclesString()
    {
        List<Obstacle> toBeSpawned = new List<Obstacle>();

        int count = Random.Range(2, maxObstaclesCount);
		int value = (count / 2) * 2;
		for (int i = 0;i<=value;i+=2)
		{
			int x = Random.Range(0, gridSize - 1);
			int z = Random.Range(0, gridSize - 1);
			int x2 = gridSize - 1 - x;
			int z2 = gridSize - 1 - z;
			if (TileTypeMap[x,z] == 0 && TileTypeMap[x2,z2] == 0)
			{
				int rndObstacle = Random.Range(0, obstaclePrefabs.Count);
				int rndRotation = Random.Range(0, 360);
				int rndRotation2 = Random.Range(0, 360);
				TileTypeMap[x, z] = obstacleId+rndObstacle;
				TileTypeMap[x2, z2] = obstacleId+rndObstacle;
                toBeSpawned.Add(new Obstacle(rndObstacle, x, z, new Vector3(x, obstaclesSpawnY, z), Quaternion.Euler(new Vector3(0, rndRotation, 0))));
                toBeSpawned.Add(new Obstacle(rndObstacle, x2, z2, new Vector3(x2, obstaclesSpawnY, z2), Quaternion.Euler(new Vector3(0, rndRotation2, 0))));
			}
		}

        string result = "";
        Debug.Log("SpawnedObstacles" + " " + toBeSpawned.Count);
        for (int i = 0; i < toBeSpawned.Count; i++)
        {
            result += toBeSpawned[i].tileX + "*" + toBeSpawned[i].tileZ + "*" + toBeSpawned[i].id + "!";

        }
        Debug.Log("result OBSTACLES" + " " + result);
        return result;
    }
	
    public void DecodeObstaclesString(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            int tileX = 0, tileZ = 0, identification = 0;
            int index = i;
            string temp = "";
            while (text[index] != '*')
            {
                temp += text[index];
                index++;
            }
            Debug.Log("x: " + temp);
            for (int a = 0; a < temp.Length; a++)
            {
                tileX += (int)temp[a] - 48;
            }
            temp = "";
            index++;

            while (text[index] != '*')
            {
                temp += text[index];
                index++;
            }
            Debug.Log("y: " + temp);
            for (int a = 0; a < temp.Length; a++)
            {
                tileZ += (int)temp[a] - 48;
            }
            temp = "";
            index++;

            while (text[index] != '!')
            {
                temp += text[index];
                index++;
            }
            Debug.Log("id: " + temp);
            i = index;
            for (int a = 0; a < temp.Length; a++)
            {
                identification += (int)temp[a] - 48;
            }

            Quaternion rot;
            if (playerId == 1)
                rot = Quaternion.Euler(0, 90, 0);
            else
                rot = Quaternion.Euler(0, -90, 0);

            SpawnedObstacles.Add(Instantiate(obstaclePrefabs[identification], new Vector3(tileX, 0.75f, tileZ), rot));
        }
    }

    public string BuildDinosString()
    {
        string result = "";
        Debug.Log("SpawnedObjects" + " " + SpawnedObjects.Count);
        for (int i = 0; i < SpawnedObjects.Count; i++)
        {
            result += SpawnedObjects[i].GetComponent<Dinosaur>().tileX + "*" + SpawnedObjects[i].GetComponent<Dinosaur>().tileZ + "*" + SpawnedObjects[i].GetComponent<Dinosaur>().id + "!";
            
        }
        Debug.Log("result" + " " + result);
        return result;
    }

    public void DecodeDinosString(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            int tileX = 0, tileZ = 0, identification = 0;
            int index = i;
            string temp = "";
            while (text[index] != '*')
            {
                temp += text[index];
                index++;
            }
            Debug.Log("x: " + temp);
            for (int a = 0; a < temp.Length; a++)
            {
                tileX += (int)temp[a] - 48;
            }
            temp = "";
            index++;

            while (text[index] != '*')
            {
                temp += text[index];
                index++;
            }
            Debug.Log("y: " + temp);
            for (int a = 0; a < temp.Length; a++)
            {
                tileZ += (int)temp[a] - 48;
            }
            temp = "";
            index++;

            while (text[index] != '!')
            {
                temp += text[index];
                index++;
            }
            Debug.Log("id: " + temp);
            i = index;
            for (int a = 0; a < temp.Length; a++)
            {
                identification += (int)temp[a] - 48;
            }

            Quaternion rot;
            if (playerId == 1)
                rot = Quaternion.Euler(0, 90, 0);
            else
                rot = Quaternion.Euler(0, -90, 0);

            SpawnedObjects.Add(Instantiate(dinosaurPrefabs[identification - 1], new Vector3(tileX, 0.75f, tileZ), rot));
        }
    }

    void ClickOnPossibleAction(Vector3 clickPoint)
    {
        var finalPosition = GetNearestPointOnGrid(clickPoint);

        int xCount = Mathf.RoundToInt(finalPosition.x);
        int zCount = Mathf.RoundToInt(finalPosition.z);

        Quaternion rot;
        if (playerId == 1)
            rot = Quaternion.Euler(0, 90, 0);
        else
            rot = Quaternion.Euler(0, -90, 0);

        bool inThose = false;
        for (int i = 0; i < nowTargetable.Count; i++)
        {
            if(nowTargetable[i].x == xCount && nowTargetable[i].y == zCount)
            {
                inThose = true;
            }
        }

        if (inTiles && inThose)
        {
            for (int a = 0; a < SpawnedObjects.Count; a++)
            {
                if(SpawnedObjects[a].GetComponent<Dinosaur>() == selectedDino)
                {
                    HidePossibleActions();
                    MultiplayerDinoMove(a, new pos(SpawnedObjects[a].GetComponent<Dinosaur>().tileX, SpawnedObjects[a].GetComponent<Dinosaur>().tileZ), new pos(xCount, zCount));
                    break;
                }
            }
            TileTypeMap[xCount, zCount] = monsterId;
            TilePlayerMap[xCount, zCount] = playerId;
        }
    }

    

	/// <summary>
	/// Deleting dinosaur from a grid
	/// </summary>
	void HandlePressOnObjectNear(Vector3 clickPoint)
	{
		var finalPosition = GetNearestPointOnGrid(clickPoint);

		int xCount = Mathf.RoundToInt(finalPosition.x);
		int zCount = Mathf.RoundToInt(finalPosition.z);

		if (inSetup && TileTypeMap[xCount, zCount] > 0 && TileTypeMap[xCount, zCount] < 10)
		{
			for(int i = 0;i<SpawnedObjects.Count;i++)
			{
				if ((int)SpawnedObjects[i].transform.position.x == xCount && (int)SpawnedObjects[i].transform.position.z == zCount)
				{
					Destroy(SpawnedObjects[i]);
                    uiController.dinosAre[SpawnedObjects[i].GetComponent<Dinosaur>().id - 1]--;
                    uiController.Sold(SpawnedObjects[i].GetComponent<Dinosaur>().id - 1);
                    SpawnedObjects.RemoveAt(i);
					TileTypeMap[xCount, zCount] = 0;
					TilePlayerMap[xCount, zCount] = playerId;
				}
			}
		}

		// Jeigu nereiks obstacles selecto, dadeti && TileTypeMap[xCount, zCount] < 10
		if (inGame && TileTypeMap[xCount, zCount] > 0)
		{
			if (!standInstance)
			{
				standInstance = Instantiate(standItem, new Vector3(finalPosition.x, 0.60f, finalPosition.z), Quaternion.identity);
                monsterId = TileTypeMap[xCount, zCount];
                for (int i = 0; i < SpawnedObjects.Count; i++)
                {
                    if(SpawnedObjects[i].GetComponent<Dinosaur>() != null && SpawnedObjects[i].GetComponent<Dinosaur>().tileX == xCount && SpawnedObjects[i].GetComponent<Dinosaur>().tileZ == zCount)
                    {
                        selectedIndex = i;
                        selectedDino = SpawnedObjects[i].GetComponent<Dinosaur>();
                    }
                }
                uiController.unitIsSelected = true;
                return;
			}

            if (standInstance)
            {
                standInstance.transform.position = new Vector3(finalPosition.x, 0.60f, finalPosition.z);
                for (int i = 0; i < SpawnedObjects.Count; i++)
                {
                    if (SpawnedObjects[i].GetComponent<Dinosaur>() != null && SpawnedObjects[i].GetComponent<Dinosaur>().tileX == xCount && SpawnedObjects[i].GetComponent<Dinosaur>().tileZ == zCount)
                    {
                        selectedIndex = i;
                        selectedDino = SpawnedObjects[i].GetComponent<Dinosaur>();

                    }
                }
                monsterId = TileTypeMap[xCount, zCount];
            }
		}
		else
		{
			Destroy(standInstance);
		}
	}
	/// <summary>
	/// Spawning selection item so that player sees where he can spawn a dinosaur
	/// </summary>
	/// <param name="mousePoint"></param>
	void PlaceSelectionNear(Vector3 mousePoint)
	{
		var finalPosition = GetNearestPointOnGrid(mousePoint);
		int xCount = Mathf.RoundToInt(finalPosition.x);
		int zCount = Mathf.RoundToInt(finalPosition.z);

		// Checking if we are holding mouse on the same tile, so selection does not despawn and we don't have to spawn it again
		if (selectionInstance && (int)selectionInstance.transform.position.x == xCount && (int)selectionInstance.transform.position.z == zCount)
			return;

		if ((inGame || inSetup) && finalPosition.x < 0 || finalPosition.x > gridSize-1 || finalPosition.z < 0 || finalPosition.z > gridSize-1)
		{
			Destroy(selectionInstance);
			inTiles = false;
			return;
		}



        if (inSetup && (TileTypeMap[xCount, zCount] != 0 || TilePlayerMap[xCount, zCount] != playerId))
		{
			Destroy(selectionInstance);
			inTiles = false;
			return;
		}

		inTiles = true;
		if (!selectionInstance)
		{
			selectionInstance = Instantiate(selectionItem, new Vector3(finalPosition.x, 0.60f, finalPosition.z), Quaternion.identity);
			return;
		}

		if (selectionInstance)
			selectionInstance.transform.position = new Vector3(finalPosition.x, 0.60f, finalPosition.z);

		
	}

	Vector3 GetNearestPointOnGrid(Vector3 position)
	{
		position -= transform.position;

		int xCount = Mathf.RoundToInt(position.x);
		int yCount = Mathf.RoundToInt(position.y);
		int zCount = Mathf.RoundToInt(position.z);

		Vector3 result = new Vector3(
			(float)xCount,
			(float)yCount,
			(float)zCount);

		result += transform.position;

		return result;
	}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (float x = 0; x < gridSize; x += 1)
        {
            for (float z = 0; z < gridSize; z += 1)
            {
                var point = GetNearestPointOnGrid(new Vector3(x, 0.75f, z));
                Gizmos.DrawSphere(point, 0.05f);
            }

        }

    }

    void UpdateObjectSpawn()
	{
		if (Input.GetMouseButtonDown(0) && inSetup)
		{
			RaycastHit hitInfo;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hitInfo))
			{
				PlaceObjectNear(hitInfo.point);
                
			}
		}

        if (Input.GetMouseButtonDown(0) && inGame)
        {
            RaycastHit hitInfo;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hitInfo))
            {
                ClickOnPossibleAction(hitInfo.point);

            }
        }
        
    }

	void UpdateSelectionSquare()
	{
        if (inGame || inSetup)
        { 
            RaycastHit hitInfo;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hitInfo))
            {
                PlaceSelectionNear(hitInfo.point);
            }
        }
	}

	void UpdatePressObject()
	{
		if (Input.GetMouseButtonDown(0) && (inSetup || inGame))
		{
			RaycastHit hitInfo;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hitInfo))
			{
				HandlePressOnObjectNear(hitInfo.point);

			}
		}
	}

	/// <summary>
	/// TileType map'e, langelio ID su obsticles prasideda nuo 10. Dabar yra 9 obstacles, tai jų IDs bus 10-18
	/// Jei spawninsi multiplayeryje, imk objektą iš listo pagal tiletype id atimti 10 (18-10 = 8) - spawninti 9 objektą iš listo
	/// </summary>
	void SpawnRandomObstaclesOnGrid()
	{
        //string t = BuildObstaclesString();
        //DecodeObstaclesString(t);
        
		
	}

}
