using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputValues : EventArgs
{
    public Vector2 PlayerDirection;
    public InputValues(Vector2 _plrDir)
    {
        PlayerDirection = _plrDir;
    }
}

public class Input : MonoBehaviour
{
    public static Input Main;
    private PlayerInputActions playerInputActions;
    private Vector2 _playerDirection;

    public event EventHandler<InputValues> onPlayerWalk;
    public event EventHandler<InputValues> onPlayerIdle;
    public event EventHandler<InputValues> onPlayerInteract;

    private void Awake()
    {
        Main = this;
    }

    private void Start()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Interact.performed += OnInteract;
    }

    private void Update()
    {
        _playerDirection = playerInputActions.Player.Walk.ReadValue<Vector2>();
        if (_playerDirection.magnitude > 0.1)
        {
            onPlayerWalk?.Invoke(this, new InputValues(_playerDirection));
        } else {
            onPlayerIdle?.Invoke(this, new InputValues(_playerDirection));
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        onPlayerInteract?.Invoke(this, new InputValues(_playerDirection));
    }



}

