using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInput {
    public Vector2 PlayerDir { get; }

}


public class Input : MonoBehaviour, IInput {
    public static Input main;
    private PlayerInputActions playerInputActions;
    private Vector2 _playerDirection;
    public Vector2 PlayerDir {
        get { return _playerDirection; }
    }

    private void Awake() 
    {
        main = this;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Interact.performed += OnInteract;

        
    }

    private void Update()
    {
        _playerDirection = playerInputActions.Player.Walk.ReadValue<Vector2>();
    }
    
    private void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log(context);
        _playerDirection = context.ReadValue<Vector2>();
    }



}

