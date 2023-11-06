using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : BasePushable, IStateChange<TrapType, PushState>
{

    void Awake() {
        type = TrapType.ElectricalBox;
    }
}
