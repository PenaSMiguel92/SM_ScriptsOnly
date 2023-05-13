using System;
using UnityEngine;

public enum CameraState { Loading, Following}
public class CameraController : MonoBehaviour
{
    public static CameraController Main;
    private GameControl _mainControl;
    private CameraState _state = CameraState.Loading;
    private Transform _mainTransform;
    private Player _playerToFollow;

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
        _playerToFollow = Player.Main;
        _mainTransform = gameObject.GetComponent<Transform>();
        _mainTransform.localPosition = _playerToFollow.GetPosition();
        _state = CameraState.Following;
    }

    void Update()
    {
        if (_state == CameraState.Loading) return;
        if (_playerToFollow == null) return;
        _mainTransform.localPosition = _playerToFollow.GetPosition();
    }
}
