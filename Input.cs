using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InventoryDirection {Left, Right}

public class InputValues : EventArgs
{
    public Vector2 PlayerDirection;
    public InputValues(Vector2 _plrDir)
    {
        PlayerDirection = _plrDir;
    }
}

public interface IInventorySelect
{
    public InventoryDirection Direction { get; }
}

public class InventorySelect : EventArgs, IInventorySelect
{
    InventoryDirection _invDir;
    public InventoryDirection Direction {get { return _invDir; } }
    public InventorySelect(InventoryDirection _direction)
    {
        _invDir = _direction;
    }
}

public class Input : MonoBehaviour
{
    public static Input Main;
    private PlayerInputActions playerInputActions;
    private Vector2 _playerDirection;

    public event EventHandler<InputValues> onPlayerWalk;
    //public event EventHandler<InputValues> onPlayerIdle;
    public event EventHandler onPlayerInteract;
    public event EventHandler<InventorySelect> onPlayerInventorySelect;

    private void Awake()
    {
        Main = this;
    }

    private void Start()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        //playerInputActions.Player.Walk.performed += OnPlayerWalk;
        playerInputActions.Player.Interact.performed += OnInteract;
        playerInputActions.Player.InventorySelectLeft.performed += OnInventorySelectLeft;
        playerInputActions.Player.InventorySelectRight.performed += OnInventorySelectRight;
    }

    private void Update()
    {
        _playerDirection = playerInputActions.Player.Walk.ReadValue<Vector2>();
        onPlayerWalk?.Invoke(this, new InputValues(_playerDirection));
    }

    // private void OnPlayerWalk(InputAction.CallbackContext context)
    // {
    //     _playerDirection = context.action.ReadValue<Vector2>();
    //     onPlayerWalk?.Invoke(this, new InputValues(_playerDirection));
    // }

    private void OnInteract(InputAction.CallbackContext context)
    {
        onPlayerInteract?.Invoke(this, EventArgs.Empty);
    }

    private void OnInventorySelectLeft(InputAction.CallbackContext context)
    {
        onPlayerInventorySelect?.Invoke(this, new InventorySelect(InventoryDirection.Left));
    }
    private void OnInventorySelectRight(InputAction.CallbackContext context)
    {
        onPlayerInventorySelect?.Invoke(this, new InventorySelect(InventoryDirection.Right));
    }

}

