using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomStats : MonoBehaviour
{
   public enum Type
    {
        corridor,
        cap,
        fourExit,
        threeExit,
        twoExit
    }  
    public Type type;

    public bool isConnNorth;
    public bool isConnEast;
    public bool isConnSouth;
    public bool isConnWest;
    public bool hasConnection;

    public float connectionOffset;
    public int roomWidth;
    public int roomHeight;
    
}


