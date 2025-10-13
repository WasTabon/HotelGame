using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Room
{
    public GameObject room;
    public GameObject wall;
    public Transform entryPos;
    public Transform roomPos;
}

public class HotelController : MonoBehaviour
{
    public List<Room> rooms;
}
