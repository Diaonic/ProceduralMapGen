using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapTile
{
    public enum RoomType
    {
        empty,
        room,
        cap,
        corridor
    }
    public RoomType roomType;
}

public class LevelGenerator : MonoBehaviour
{
    //public int numRoomTries;
    public static MapTile[,] map;
    public static int map_size;
    public static int map_size_x = 40;
    public static int map_size_y = 40;
    public int maximumRoomCount = 10;
    public float spawnDelay;
    public GameObject[] rooms;
    public GameObject[] corridors;
    public GameObject[] endCaps;
    public GameObject wallPrefab;
    public Collider2D[] colliders;
    public List<Vector3> connectionKeys;
    public Dictionary<Vector3, GameObject> connectionPoints = new Dictionary<Vector3, GameObject>();

    public LayerMask prefabMask;


    private void Start()
    {
        GenerateDungeon();
    }

    public void GenerateDungeon()
    {
        SpawnStartingRoom();
        AddConnectionPoints();
        StartCoroutine(Example());
    }


    IEnumerator Example()
    {
        for (int i = 0; i < maximumRoomCount; i++)
        {
            SpawnPrefab(rooms);
            AddConnectionPoints();
            yield return new WaitForSeconds(spawnDelay);
            if (i % 3 == 0)
            {
                SpawnPrefab(corridors);
                AddConnectionPoints();
            }
        }
        CapRoomEnds(endCaps);
        yield return new WaitForSeconds(.5f);
        FillWallGaps();
    }

    /// <summary>
    /// Checks if an object with collider is present at the location, 
    /// it uses a box collider size to evaluate if placement is available.
    /// </summary>
    /// <param name="pos">Position the box is being placed</param>
    /// <param name="go">Gameobject that wants to be placed</param>
    /// <returns></returns>
    private bool CheckOverlap(Vector3 pos, GameObject go)
    {
        Vector2 size = go.GetComponent<BoxCollider2D>().size;
        Collider2D[] hits = Physics2D.OverlapBoxAll(pos, size, 0);
        foreach (var hit in hits)
        {
            if (hit.gameObject.layer == 8)
            {
                return true;
            }
        }

        return false;
    }

    // Generate 100x100 map of MapTiles which are empty
    // We're barely using this grid, its really just here for future development
    private void GenerateGrid()
    {
        map = new MapTile[map_size_x, map_size_y];
        for (int x = 0; x < map_size_x; x++)
        {
            for (int y = 0; y < map_size_y; y++)
            {
                map[x, y] = new MapTile
                {
                    roomType = 0
                };
            }
        }
    }

    //Gives us an initial starting spot for our map
    private void SpawnStartingRoom()
    {
        Vector3 randomPos = RandomMapPos();
        GameObject roomPrefab = GameObject.Instantiate(rooms[Random.Range(0, rooms.Length)], randomPos, Quaternion.identity);
        AddConnectionPoints();
    }

    private bool HallwaySafetyNet()
    {
        GameObject[] hallways = GameObject.FindGameObjectsWithTag("Hall");
        GameObject[] rooms = GameObject.FindGameObjectsWithTag("Room");

        if (hallways.Length >= rooms.Length)
            return true;
        return false;
    }

    /// <summary>
    /// Evaluates available connections on game objects and flags them as connected
    /// </summary>
    /// <param name="roomPrefab"></param>
    /// <param name="tarDir"></param>
    /// <param name="roomToConnectTo"></param>
    private void FlagRoomConnections(RoomStats roomPrefab, int tarDir, RoomStats roomToConnectTo)
    {
        switch (tarDir)
        {
            case 0: //north
                if (roomPrefab.type == RoomStats.Type.corridor)
                {
                    roomPrefab.isConnEast = true;
                    roomToConnectTo.isConnEast = true;
                    roomPrefab.isConnWest = true;
                    roomToConnectTo.isConnWest = true;
                }
                roomPrefab.isConnSouth = true;
                roomToConnectTo.isConnNorth = true;
                break;
            case 1: //east
                if (roomPrefab.type == RoomStats.Type.corridor)
                {
                    roomPrefab.isConnNorth = true;
                    roomToConnectTo.isConnNorth = true;
                    roomPrefab.isConnSouth = true;
                    roomToConnectTo.isConnSouth = true;
                }
                roomPrefab.isConnWest = true;
                roomToConnectTo.isConnEast = true;
                break;
            case 2: //south
                if (roomPrefab.type == RoomStats.Type.corridor)
                {
                    roomPrefab.isConnEast = true;
                    roomToConnectTo.isConnEast = true;
                    roomPrefab.isConnWest = true;
                    roomToConnectTo.isConnWest = true;
                }
                roomPrefab.isConnNorth = true;
                roomToConnectTo.isConnSouth = true;
                break;
            case 3: //west
                if (roomPrefab.type == RoomStats.Type.corridor)
                {
                    roomPrefab.isConnNorth = true;
                    roomToConnectTo.isConnNorth = true;
                    roomPrefab.isConnSouth = true;
                    roomToConnectTo.isConnSouth = true;
                }
                roomPrefab.isConnEast = true;
                roomToConnectTo.isConnWest = true;
                break;
        }
    }

    /// <summary>
    /// Main creation function, this takes a prefab list and spawns the object
    /// It does so by grabbing an available connection location from the dictionary
    /// Then evaluating a position that qualifies for placement
    /// </summary>
    /// <param name="prefabList"></param>
    private void SpawnPrefab(GameObject[] prefabList)
    {
        Vector3 spawnVector = connectionKeys[Random.Range(0, connectionKeys.Count - 1)];
        GameObject roomToConnectTo = connectionPoints[spawnVector];
        RoomStats roomToConStats = roomToConnectTo.GetComponent<RoomStats>();
        GameObject roomPrefab = GameObject.Instantiate(prefabList[Random.Range(0, prefabList.Length)], new Vector3(-100, -100, 0), Quaternion.identity);
        RoomStats prefabStats = roomPrefab.GetComponent<RoomStats>();
        Vector3 roomToConNORTH = new Vector3(roomToConnectTo.transform.position.x, roomToConnectTo.transform.position.y + (roomToConStats.connectionOffset * 2), roomToConnectTo.transform.position.z);
        Vector3 roomToConEAST = new Vector3(roomToConnectTo.transform.position.x + (roomToConStats.connectionOffset * 2), roomToConnectTo.transform.position.y, roomToConnectTo.transform.position.z);
        Vector3 roomToConSOUTH = new Vector3(roomToConnectTo.transform.position.x, roomToConnectTo.transform.position.y + (-roomToConStats.connectionOffset * 2), roomToConnectTo.transform.position.z);
        Vector3 roomToConWEST = new Vector3(roomToConnectTo.transform.position.x + (-roomToConStats.connectionOffset * 2), roomToConnectTo.transform.position.y, roomToConnectTo.transform.position.z);

        //Evaluating placement
        if (!CheckOverlap(spawnVector, roomPrefab))
        {
            if (!roomToConStats.isConnNorth && spawnVector == roomToConNORTH)
            {
                roomToConNORTH = new Vector3(roomToConnectTo.transform.position.x, roomToConnectTo.transform.position.y + (roomToConStats.connectionOffset + prefabStats.connectionOffset), 0);
                PlaceRoom(roomPrefab, roomToConNORTH, 90f);
                FlagRoomConnections(prefabStats, 0, roomToConStats);
                return;
            }


            if (!roomToConStats.isConnEast && spawnVector == roomToConEAST)
            {
                roomToConEAST = new Vector3(roomToConnectTo.transform.position.x + (roomToConStats.connectionOffset + prefabStats.connectionOffset), roomToConnectTo.transform.position.y, 0);
                PlaceRoom(roomPrefab, roomToConEAST, 0f);
                FlagRoomConnections(prefabStats, 1, roomToConStats);
                return;
            }

            if (!roomToConStats.isConnSouth && spawnVector == roomToConSOUTH)
            {

                roomToConSOUTH = new Vector3(roomToConnectTo.transform.position.x, roomToConnectTo.transform.position.y + -(roomToConStats.connectionOffset + prefabStats.connectionOffset), 0);
                PlaceRoom(roomPrefab, roomToConSOUTH, 90f);
                FlagRoomConnections(prefabStats, 2, roomToConStats);
                return;
            }

            if (!roomToConStats.isConnWest && spawnVector == roomToConWEST)
            {
                roomToConWEST = new Vector3(roomToConnectTo.transform.position.x + -(roomToConStats.connectionOffset + prefabStats.connectionOffset), roomToConnectTo.transform.position.y, 0);
                PlaceRoom(roomPrefab, roomToConWEST, 0f);
                FlagRoomConnections(prefabStats, 3, roomToConStats);
                return;
            }

        }
        //Add new connection points
        AddConnectionPoints();
    }

    /// <summary>
    /// TODO: Refactor into single method
    /// This caps room ends after the base map has been created. I think this can get refactored into the method above
    /// </summary>
    /// <param name="prefabList"></param>
    private void CapRoomEnds(GameObject[] prefabList)
    {
        Vector3 spawnVector;
        GameObject roomToConnectTo;

        for (int i = 0; i < connectionKeys.Count; i++)
        {
            spawnVector = connectionKeys[i];
            roomToConnectTo = connectionPoints[spawnVector];
            RoomStats roomToConStats = roomToConnectTo.GetComponent<RoomStats>();

            Vector3 roomToConNORTH = new Vector3(roomToConnectTo.transform.position.x, roomToConnectTo.transform.position.y + (roomToConStats.connectionOffset + 2), roomToConnectTo.transform.position.z);
            Vector3 roomToConEAST = new Vector3(roomToConnectTo.transform.position.x + (roomToConStats.connectionOffset * 2), roomToConnectTo.transform.position.y, roomToConnectTo.transform.position.z);
            Vector3 roomToConSOUTH = new Vector3(roomToConnectTo.transform.position.x, roomToConnectTo.transform.position.y + (-roomToConStats.connectionOffset * 2), roomToConnectTo.transform.position.z);
            Vector3 roomToConWEST = new Vector3(roomToConnectTo.transform.position.x + (-roomToConStats.connectionOffset * 2), roomToConnectTo.transform.position.y, roomToConnectTo.transform.position.z);



            foreach (var prefab in prefabList)
            {
                GameObject roomPrefab = GameObject.Instantiate(prefab, new Vector3(-100, -100, 0), Quaternion.identity);
                RoomStats prefabStats = roomPrefab.GetComponent<RoomStats>();
                if (!CheckOverlap(spawnVector, roomPrefab))
                {
                    if (!roomToConStats.isConnNorth && spawnVector == roomToConNORTH)
                    {
                        spawnVector = new Vector3(roomToConnectTo.transform.position.x, roomToConnectTo.transform.position.y + (roomToConStats.connectionOffset + prefabStats.connectionOffset), 0);
                        PlaceRoom(roomPrefab, spawnVector, -90f);
                        FlagRoomConnections(prefabStats, 0, roomToConStats);

                    }

                    if (!roomToConStats.isConnEast && spawnVector == roomToConEAST)
                    {
                        spawnVector = new Vector3(roomToConnectTo.transform.position.x + (roomToConStats.connectionOffset + prefabStats.connectionOffset), roomToConnectTo.transform.position.y, 0);
                        PlaceRoom(roomPrefab, spawnVector, 180f);
                        FlagRoomConnections(prefabStats, 1, roomToConStats);

                    }

                    if (!roomToConStats.isConnSouth && spawnVector == roomToConSOUTH)
                    {
                        spawnVector = new Vector3(roomToConnectTo.transform.position.x, roomToConnectTo.transform.position.y + -(roomToConStats.connectionOffset + prefabStats.connectionOffset), 0);
                        PlaceRoom(roomPrefab, spawnVector, 90f);
                        FlagRoomConnections(prefabStats, 2, roomToConStats);
                    }

                    if (!roomToConStats.isConnWest && spawnVector == roomToConWEST)
                    {
                        spawnVector = new Vector3(roomToConnectTo.transform.position.x + -(roomToConStats.connectionOffset + prefabStats.connectionOffset), roomToConnectTo.transform.position.y, 0);
                        PlaceRoom(roomPrefab, spawnVector, 0f);
                        FlagRoomConnections(prefabStats, 3, roomToConStats);
                    }

                }
                if (roomPrefab.transform.position == new Vector3(-100, -100, 0))
                {
                    Destroy(roomPrefab);
                }
            }
        }

    }

    //This places and rotates objects
    private void PlaceRoom(GameObject prefab, Vector3 spawnLocation, float rotation)
    {
        prefab.transform.position = spawnLocation;
        prefab.transform.Rotate(0, 0, rotation);
    }

    //This evaluates each floor tile and fills in the map where an edge might not have a wall
    private void FillWallGaps()
    {
        GameObject[] floorTiles = GameObject.FindGameObjectsWithTag("Floor");
        Debug.Log("FLOOR TILES: " + floorTiles.Length);
        foreach (var floor in floorTiles)
        {
            //check four directions
            Vector3 north = new Vector3(floor.transform.position.x, floor.transform.position.y + 1f, 0);
            Vector3 south = new Vector3(floor.transform.position.x, floor.transform.position.y - 1f, 0);
            Vector3 east = new Vector3(floor.transform.position.x + 1f, floor.transform.position.y, 0);
            Vector3 west = new Vector3(floor.transform.position.x - 1f, floor.transform.position.y, 0);
            GameObject wallPrefabObj = GameObject.Instantiate(wallPrefab, new Vector3(-100, -100, 0), Quaternion.identity);


            if (!CheckOverlap(north, wallPrefabObj))
            {
                PlaceRoom(wallPrefabObj, north, 0f);
            }

            if (!CheckOverlap(east, wallPrefabObj))
            {
                PlaceRoom(wallPrefabObj, east, 0f);
            }

            if (!CheckOverlap(west, wallPrefabObj))
            {
                PlaceRoom(wallPrefabObj, west, 0f);
            }

            if (!CheckOverlap(south, wallPrefabObj))
            {
                PlaceRoom(wallPrefabObj, south, 0f);
            }

            if (wallPrefabObj.transform.position == new Vector3(-100, -100, 0))
            {
                Destroy(wallPrefabObj);
            }

        }
    }

    //Adds connection points to our connection array and dictionary
    private void AddConnectionPoints()
    {

        GameObject[] roomObjs = GameObject.FindGameObjectsWithTag("Room");
        GameObject[] hallObjs = GameObject.FindGameObjectsWithTag("Hall");
        var allObjects = roomObjs.Union(hallObjs).ToArray();
        Dictionary<Vector3, GameObject> newPoints = new Dictionary<Vector3, GameObject>();
        List<Vector3> newKeys = new List<Vector3>();

        foreach (var room in allObjects)
        {
            if (room.transform.position == new Vector3(-100, -100, 0))
            {
                Destroy(room);
                continue;
            }
            RoomStats roomInfo = room.GetComponent<RoomStats>();

            if (roomInfo.isConnNorth == false)
            {
                Vector3 cp = new Vector3(room.transform.position.x, room.transform.position.y + (roomInfo.connectionOffset * 2), room.transform.position.z);

                if (!newPoints.ContainsKey(cp))
                {
                    newKeys.Add(cp);
                    newPoints.Add(cp, room);
                }

            }
            if (roomInfo.isConnEast == false)
            {
                Vector3 cp = new Vector3(room.transform.position.x + (roomInfo.connectionOffset * 2), room.transform.position.y, room.transform.position.z);
                if (!newPoints.ContainsKey(cp))
                {
                    newPoints.Add(cp, room);
                    newKeys.Add(cp);
                }

            }
            if (roomInfo.isConnSouth == false)
            {
                Vector3 cp = new Vector3(room.transform.position.x, room.transform.position.y + (-roomInfo.connectionOffset * 2), room.transform.position.z);
                if (!newPoints.ContainsKey(cp))
                {
                    newPoints.Add(cp, room);
                    newKeys.Add(cp);
                }

            }
            if (roomInfo.isConnWest == false)
            {
                Vector3 cp = new Vector3(room.transform.position.x + (-roomInfo.connectionOffset * 2), room.transform.position.y, room.transform.position.z);
                if (!newPoints.ContainsKey(cp))
                {
                    newPoints.Add(cp, room);
                    newKeys.Add(cp);
                }

            }
        }

        connectionKeys = newKeys;
        connectionPoints = newPoints;
    }

    //Gives us a random map position in grid
    private Vector3 RandomMapPos()
    {
        int x = Random.Range(0, map_size_x);
        int y = Random.Range(0, map_size_y);

        Vector3 newPos = new Vector3(x, y, 0);
        return newPos;
    }
}