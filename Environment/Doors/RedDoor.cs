using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedDoor : BaseDoor, IDoor
{
    public bool TestDoor(List<PickUpType> _plrInventory)
    {
        if (state == DoorState.Opening) return false;
        if (_plrInventory.Contains(PickUpType.RedKey))
        {
            OpenDoor();
        }
        return false;
    }
}
