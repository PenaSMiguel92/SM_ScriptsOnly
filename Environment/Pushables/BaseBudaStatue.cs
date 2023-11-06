using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBudaStatue : BasePushable, IStateChange<TrapType, PushState>
{
    
    protected ShootDirection shootDir;
    [SerializeField] Sprite[] sprites;
    void Awake() {
        type = TrapType.BudaStatue;
    }
    protected new void Start() {
        base.Start();
    }
    protected new void Update() {
        base.Update();
    }
}
