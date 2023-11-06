using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinDoor : BaseDoor, IDoor
{
    void DetermineCoinDoorState()
    {
        int coin_sum = 0;
        GameObject[] coins = GameObject.FindGameObjectsWithTag("pickup");
        foreach (GameObject coin in coins)
        {
            if ( coin.GetComponent<IPickup>().TypeOfPickup == PickUpType.Coin)
            {
                coin_sum += 1;
            }
        }
        if (coin_sum <= 0)
        {
            OpenDoor();
        }
    }
    public bool TestDoor(List<PickUpType> _plrInventory)
    {
        if (state == DoorState.Opening) return false;

        return false;
    }
    
    void Update()
    {
        if (state == DoorState.Loading) return;
        if (state == DoorState.Opening) return;
        if (state == DoorState.Idle)
        {
            DetermineCoinDoorState();
        }
        
    }
}
