using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenDoor : BaseDoor, IDoor
{
    public bool TestDoor(List<PickUpType> _plrInventory)
    {
        if (state == DoorState.Opening) return false;
        if (_plrInventory.Contains(PickUpType.GreenKey))
        {
            OpenDoor();
        }
        return false;
    }
}
