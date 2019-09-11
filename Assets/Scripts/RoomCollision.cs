using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCollision : MonoBehaviour
{

    public enum Direction
    {
        north,
        south,
        east,
        west
    }

    public Direction direction;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Room"))
        {
            Debug.Log("COLLISION COLLIDE");
        }
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag.Equals("Room"))
        {
            Debug.Log("COLLISION TRIGGER");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {


    }

}
