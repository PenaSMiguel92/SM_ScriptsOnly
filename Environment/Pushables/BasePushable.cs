using System;
using System.Collections.Generic;
using UnityEngine;

public class BasePushable : BaseTile, IPushable
{
    protected bool crossingExit = false;
    protected bool crossingEnter = false;
    protected bool crossingMoving = false;
    protected float moveSpeed = 4.25f;
    protected float moveT = 0f; //for Vector3.Lerp
    protected TrapType type;
    protected PushState state = PushState.Loading;
    protected Vector3Int dir;
    public bool CrossingExit {get { return crossingExit; } set { crossingExit = value; } }
    public bool CrossingEnter {get { return crossingEnter; } set { crossingEnter = value; } }
    public bool CrossingMoving {get { return crossingMoving; } set { crossingMoving = value; } }
    public PushState State {get { return state; } set { state = value; } }
    public TrapType Type {get { return type; }}
    public Vector3Int MoveDirection { get { return dir; } }
    
    void OnGameStart(object _sender, EventArgs _e)
    {
        positionTrack = currentTransform.localPosition;
        gridPos = mainControl.GetGridPosition(positionTrack);
        state = PushState.Idle;
    }

    public Vector3 UpdatePosition(Vector3 _newPosition)
    {
        currentTransform.localPosition = _newPosition;
        return currentTransform.localPosition;
    }

    public bool TestPush(Vector3Int _plrMoveDirection)
    {
        if (state == PushState.Moving) return true;
        bool _proceed = true;
        List<TilemapUse> tmp_tilemaps = new List<TilemapUse> { TilemapUse.Foreground, TilemapUse.Moveables};
        foreach (TilemapUse tmp_tilemap in tmp_tilemaps)
        {
            if (!mainControl.HasTile(gridPos + _plrMoveDirection, tmp_tilemap)) continue;
            GameObject tmp_spot_obj = mainControl.GetInstantiatedObject(gridPos + _plrMoveDirection, tmp_tilemap);
            if (tmp_spot_obj == null) continue;
            _proceed = tmp_spot_obj.tag switch
            {
                "switch" => tmp_spot_obj.GetComponent<TrapSwitchHandle>().TestTrapSwitch(gameObject, _plrMoveDirection),
                "trap" => tmp_spot_obj.GetComponent<TrapHandle>().TestPushable(gameObject),
                "pickup" => true,
                "mover" => true,
                _ => false,
            };
        }

        if (_proceed)
        {
            audioManager.PlaySound(SoundType.Push, positionTrack);
            moveT = 0;
            gridPos += _plrMoveDirection;
            dir = _plrMoveDirection;
            moveSpeed = 4.25f;
            state = PushState.Moving;
        }
        return _proceed;
    }
    
    public bool ForcePush(Vector3Int _moveDirection, float _speed) {
        if (state == PushState.Moving) return true;
        bool _proceed = true;
        List<TilemapUse> tmp_tilemaps = new List<TilemapUse> { TilemapUse.Foreground, TilemapUse.Moveables};
        foreach (TilemapUse tmp_tilemap in tmp_tilemaps)
        {
            if (!mainControl.HasTile(gridPos + _moveDirection, tmp_tilemap)) continue;
            GameObject tmp_spot_obj = mainControl.GetInstantiatedObject(gridPos + _moveDirection, tmp_tilemap);
            if (tmp_spot_obj == null) continue;
            _proceed = tmp_spot_obj.tag switch
            {
                "switch" => tmp_spot_obj.GetComponent<TrapSwitchHandle>().TestTrapSwitch(gameObject, _moveDirection),
                "trap" => tmp_spot_obj.GetComponent<TrapHandle>().TestPushable(gameObject),
                "pickup" => true,
                "mover" => true,
                _ => false,
            };
        }

        if (_proceed)
        {
            //_audioManager.PlaySound(SoundType.Push, _positionTrack);
            moveT = 0;
            gridPos += _moveDirection;
            dir = _moveDirection;
            moveSpeed = _speed;
            state = PushState.Moving;

        }
        return _proceed;
    }

    protected new void Start()
    {
        base.Start();
        mainControl.onGameStart += OnGameStart;
        if (mainControl.State == GameState.LevelPlay)
        {
            OnGameStart(this, EventArgs.Empty);
        }
    }

    protected void Update()
    {
        if (state == PushState.Loading || state == PushState.Idle) return;
        switch(state)
        {
            case PushState.Set:
                mainControl.SetTile(gridPos - dir, null, TilemapUse.Moveables);
                break;
            case PushState.Moving:
                moveT = Mathf.Min(1, moveT + (Time.deltaTime * moveSpeed));
                UpdatePosition(Vector3.Lerp(positionTrack, gridPos + gridOffset, moveT));//_positionTrack + (Time.deltaTime * new Vector3(_dir.x, _dir.y, 0) * _moveSpeed));
                if (moveT == 1)//((_positionTrack - (_gridPos + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
                {
                    positionTrack = UpdatePosition(gridPos + gridOffset);
                    state = PushState.Idle;
                    mainControl.MoveTile(gridPos - dir, gridPos, TilemapUse.Moveables);
                }
                break;

        }
    }
}
