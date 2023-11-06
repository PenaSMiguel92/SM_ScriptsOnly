using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : BasePushable, IStateChange<TrapType, PushState>
{

    void Awake() {
        type = TrapType.Hole;
    }
}
