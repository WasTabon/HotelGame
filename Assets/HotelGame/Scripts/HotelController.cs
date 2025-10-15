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
    public bool isLive;
    public bool isBuilded;
    public bool isLeft;
    public RoomController roomController;
}

public class HotelController : MonoBehaviour
{
    public static HotelController Instance;
    
    public List<Room> rooms;
    public List<GameObject> guestPrefabs;
    public Transform spawnPosition;
    public Transform waitingPosition;

    private GuestController currentGuest;
    private bool isQueueFree = true;
    private RoomBuildAnimation buildAnimation;

    private void Awake()
    {
        Instance = this;
        buildAnimation = gameObject.AddComponent<RoomBuildAnimation>();
    }

    void Start()
    {
        InitializeRooms();
        SpawnGuest();
        
        // доробити анімації прибирання номерів, добавити прокачку, добавити казино
    }

    private void InitializeRooms()
    {
        foreach (Room room in rooms)
        {
            if (room.room != null)
            {
                room.room.SetActive(true);
                UpdateRoomState(room, false);
            }
        }
    }

    private void UpdateRoomState(Room room, bool animate)
    {
        if (room.room == null) return;

        if (room.isBuilded)
        {
            if (animate)
            {
                buildAnimation.PlayBuildAnimation(room.room, room.wall);
            }
            else
            {
                foreach (Transform child in room.room.transform)
                {
                    child.gameObject.SetActive(true);
                }
                
                if (room.wall != null)
                {
                    room.wall.SetActive(false);
                }
            }
        }
        else
        {
            foreach (Transform child in room.room.transform)
            {
                child.gameObject.SetActive(false);
            }
            
            if (room.wall != null)
            {
                room.wall.SetActive(true);
            }
        }
    }

    public void BuyRoom(Room room)
    {
        if (room != null && !room.isBuilded)
        {
            room.isBuilded = true;
            UpdateRoomState(room, true);
            Debug.Log($"Room {room.room.name} has been purchased!");
        }
    }

    public void BuildRoom(Room room)
    {
        if (room != null)
        {
            room.isBuilded = true;
            UpdateRoomState(room, false);
        }
    }

    public void SpawnGuest()
    {
        if (!isQueueFree || guestPrefabs.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, guestPrefabs.Count);
        GameObject guestObj = Instantiate(guestPrefabs[randomIndex], spawnPosition.position, Quaternion.identity);
        currentGuest = guestObj.GetComponent<GuestController>();

        if (currentGuest != null)
        {
            isQueueFree = false;
            currentGuest.Initialize(this, waitingPosition.position);
        }
    }

    public void AssignGuestToRoom()
    {
        if (currentGuest == null) return;

        Room availableRoom = FindAvailableRoom();
        if (availableRoom != null)
        {
            availableRoom.isLive = true;
            
            if (availableRoom.roomController != null)
            {
                availableRoom.roomController.SetGuestLiving(true);
            }
            
            currentGuest.MoveToRoom(availableRoom.entryPos.position, availableRoom.roomPos.position);
        }
    }

    public void FreeQueue()
    {
        isQueueFree = true;
        currentGuest = null;
        SpawnGuest();
    }

    public bool CanInteract()
    {
        return currentGuest != null && currentGuest.IsAtWaitingPoint() && HasAvailableRoom();
    }

    public bool HasAvailableRoom()
    {
        return FindAvailableRoom() != null;
    }

    private Room FindAvailableRoom()
    {
        foreach (Room room in rooms)
        {
            if (room.isBuilded && !room.isLive)
            {
                return room;
            }
        }
        return null;
    }
}