using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICamera
{

}

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _mainCameraTransform;
    public static CameraController Main;
    private GameControl _mainControl;
    private IPlayer _playerToFollow;

    void Awake()
    {
        Main = this;
    }
    void Start()
    {
        
        _mainControl = GameControl.Main;
        _mainControl.onGameStart += OnGameStart;

    }

    void OnGameStart(object _sender, EventArgs _e)
    {
        _playerToFollow = GetComponent<IPlayer>();
    }

    void Update()
    {
        if (_playerToFollow == null) return;
        _mainCameraTransform.localPosition = _playerToFollow.LocalPosition;
    }
}
