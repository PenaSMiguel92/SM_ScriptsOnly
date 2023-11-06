using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableDoor : BaseDoor, IDoor
{
    private int bw_currentStep = 0; //bw = breakable wall variables
    private bool bw_timerSet = false;
    private float bw_timerValue;
     void HandleBreakable()
    {
        if (bw_timerSet) return;
        bw_timerSet = true;
        bw_timerValue = Time.realtimeSinceStartup;
        bw_currentStep += 1;
        //_curRenderer.sprite = _sprites[bw_currentStep];
    }
    void UpdateBreakableTimerState()
    {
        if (!bw_timerSet) return;
        float tmp_maxTime = 0.25f;
        if (Time.realtimeSinceStartup - bw_timerValue > tmp_maxTime)
        {
            bw_timerSet = false;
        }
    }
    public bool TestDoor(List<PickUpType> _plrInventory)
    {
        if (state == DoorState.Opening) return false;
        if (bw_timerSet) return false;
        HandleBreakable();
        if (bw_currentStep > 4)
        {
            animator.CurrentFrame = bw_currentStep;
            OpenDoor();
        }
        return false;
    }
    void Update()
    {
        if (state == DoorState.Loading) return;
        if (state == DoorState.Opening) return;
        if (state == DoorState.Idle)
        {
            UpdateBreakableTimerState();
        }
    }
}
