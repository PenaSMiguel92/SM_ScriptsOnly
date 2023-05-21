using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public enum GameState {Loading, Cutscene, Menu, Pause, LevelPlay, LevelEnd}
public enum DeathType { Standard, Burn, Electricution, Acid }
public enum TilemapUse { Foreground, Moveables, Enemies }
public interface IGameControl
{
    public GameState State { get; }

    public Vector3Int GetGridPosition(Vector3 _position);
    public GameObject GetInstantiatedObject(Vector3Int _location, TilemapUse _tilemapUse);
    public void SetTile(Vector3Int _location, TileBase _tile, TilemapUse _tilemapUse);
    public TileBase GetTile(Vector3Int _location, TilemapUse _tilemapUse);
    public bool HasTile(Vector3Int _location, TilemapUse _tilemapUse);
    public void MoveTile(Vector3Int _currentLocation, Vector3Int _nextLocation, TilemapUse _tilemapUse);
    public void FinishLevel();
    public void InvokeTrapStateChange(bool _state);
}

public class ITrapStateChange : EventArgs
{
    public bool State;
    public ITrapStateChange(bool _state)
    {
        State = _state;
    }
}

public class GameControl : MonoBehaviour, IGameControl
{
    [SerializeField] private Tilemap _foregroundTilemap;
    [SerializeField] private Tilemap _moveablesTilemap;
    [SerializeField] private Tilemap _enemiesTilemap;
    [SerializeField] private Tilemap _playersTilemap;
    public static GameControl Main;
    private AudioManager _audioManager;
    private Player _currentPlayer;
    private GameState _state = GameState.Loading;
    public GameState State {get { return _state; } }

    public event EventHandler onGameStart;
    public event EventHandler onGameFadeToNextLevel;
    public event EventHandler<ITrapStateChange> onTrapStateChange;

    //public event UnityAction  

    void Awake()
    {
        Main = this;  
    }
    // Start is called before the first frame update
    void Start()
    {
        _audioManager = AudioManager.Main;
        _currentPlayer = Player.Main;
        _currentPlayer.onPlayerDeath += RestartLevel;
        _foregroundTilemap.GetComponent<TilemapRenderer>().forceRenderingOff = true;
        _moveablesTilemap.GetComponent<TilemapRenderer>().forceRenderingOff = true;
        _enemiesTilemap.GetComponent<TilemapRenderer>().forceRenderingOff = true;
        _playersTilemap.GetComponent<TilemapRenderer>().forceRenderingOff = true;
        StartCoroutine(FadeInfromLastScene(Time.realtimeSinceStartup));
    }

    public Vector3Int GetGridPosition(Vector3 _position)
    {
        return _foregroundTilemap.LocalToCell(_position);
    }

    public GameObject GetInstantiatedObject(Vector3Int _location, TilemapUse _tilemapUse)
    {
        GameObject tmp_instantiatedObject = null;
        switch(_tilemapUse)
        {
            case TilemapUse.Foreground:
                tmp_instantiatedObject = _foregroundTilemap.GetInstantiatedObject(_location);
                break;
            case TilemapUse.Moveables:
                tmp_instantiatedObject = _moveablesTilemap.GetInstantiatedObject(_location);
                break;
            case TilemapUse.Enemies:
                tmp_instantiatedObject = _enemiesTilemap.GetInstantiatedObject(_location);
                break;

        }
        return tmp_instantiatedObject;
    }

    public void SetTile(Vector3Int _location, TileBase _tile, TilemapUse _tilemapUse)
    {
        switch(_tilemapUse)
        {
            case TilemapUse.Foreground:
                _foregroundTilemap.SetTile(_location, _tile);
                break;
            case TilemapUse.Moveables:
                _moveablesTilemap.SetTile(_location, _tile);
                break;
            case TilemapUse.Enemies:
                _enemiesTilemap.SetTile(_location, _tile);
                break;
        }
        
    }
    public TileBase GetTile(Vector3Int _location, TilemapUse _tilemapUse)
    {
        TileBase tmp_tile = null;
        switch(_tilemapUse)
        {
            case TilemapUse.Foreground:
                tmp_tile = _foregroundTilemap.GetTile(_location);
                break;
            case TilemapUse.Moveables:
                tmp_tile = _moveablesTilemap.GetTile(_location);
                break;
            case TilemapUse.Enemies:
                tmp_tile = _enemiesTilemap.GetTile(_location);
                break;
        }
        return tmp_tile;
    }
    public bool HasTile(Vector3Int _location, TilemapUse _tilemapUse)
    {
        bool _hasTile = false;
        switch(_tilemapUse)
        {
            case TilemapUse.Foreground:
                _hasTile = _foregroundTilemap.HasTile(_location);
                break;
            case TilemapUse.Moveables:
                _hasTile = _moveablesTilemap.HasTile(_location);
                break;
            case TilemapUse.Enemies:
                _hasTile = _enemiesTilemap.HasTile(_location);
                break;

        }
        
        return _hasTile;
    }
    public void MoveTile(Vector3Int _currentLocation, Vector3Int _nextLocation, TilemapUse _tilemapUse)
    {
        TileBase tmp_targetTile;
        switch(_tilemapUse)
        {
            case TilemapUse.Foreground:
                tmp_targetTile = _foregroundTilemap.GetTile(_currentLocation);
                _foregroundTilemap.SetTile(_nextLocation, tmp_targetTile);
                _foregroundTilemap.SetTile(_currentLocation, null);
                break;
            case TilemapUse.Moveables:
                tmp_targetTile = _moveablesTilemap.GetTile(_currentLocation);
                _moveablesTilemap.SetTile(_nextLocation, tmp_targetTile);
                _moveablesTilemap.SetTile(_currentLocation, null);
                break;
            case TilemapUse.Enemies:
                tmp_targetTile = _enemiesTilemap.GetTile(_currentLocation);
                _enemiesTilemap.SetTile(_nextLocation, tmp_targetTile);
                _enemiesTilemap.SetTile(_currentLocation, null);
                break;
        }
       
    }
    public void FinishLevel()
    {
        _audioManager.PlaySound(SoundType.PassLevel, _currentPlayer.GetPosition());
        StartCoroutine(FadeToNextScene(SceneManager.GetActiveScene().buildIndex + 1, Time.realtimeSinceStartup));
    }
    IEnumerator FadeInfromLastScene(double startTime)
    {
        bool endAnim = false;
        while (!endAnim)
        {
            if ((Time.realtimeSinceStartup - startTime)>2)
            {
                endAnim = true;
                onGameStart?.Invoke(this, EventArgs.Empty);
                _state = GameState.LevelPlay;
            }
            else
            {
                GameObject fadeScreen = GameObject.FindGameObjectWithTag("fade");
                fadeScreen.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 1f - (float)((Time.realtimeSinceStartup - startTime) / 2));
            }
            yield return new WaitForEndOfFrame();
        }
        
    }
    IEnumerator FadeToNextScene(int index, double startTime)
    {
        onGameFadeToNextLevel?.Invoke(this, EventArgs.Empty);
        bool endAnim = false;
        while (!endAnim)
        {
            //curFrame += 1;
            if ((Time.realtimeSinceStartup - startTime)>2)
            {
                endAnim = true;
                SceneManager.LoadScene(index);
            }
            else
            { 
                GameObject fadeScreen = GameObject.FindGameObjectWithTag("fade");
                fadeScreen.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, (float)((Time.realtimeSinceStartup - startTime) / 2));
            }
            yield return new WaitForEndOfFrame();
        }
        
    }
    public void RestartLevel(object _sender, EventDeath _e)
    {
        StartCoroutine(FadeToNextScene(SceneManager.GetActiveScene().buildIndex, Time.realtimeSinceStartup));
    }
    public void InvokeTrapStateChange(bool _state)
    {
        onTrapStateChange?.Invoke(this, new ITrapStateChange(_state));
    }
}
