using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueDoor : BaseDoor, IDoor
{
    public bool TestDoor(List<PickUpType> _plrInventory)
    {
        if (state == DoorState.Opening) return false;
        if (_plrInventory.Contains(PickUpType.BlueKey))
        {
            OpenDoor();
        }
        return false;
    }
}
