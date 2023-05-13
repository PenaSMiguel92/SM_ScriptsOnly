using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public enum GameState {Loading, Cutscene, Menu, Pause, LevelPlay, LevelEnd}
public enum DeathType { Standard, Burn, Electricution, Acid };
public interface IGameControl
{
    public GameState State { get; }
    public Tilemap Foreground { get; }
    public Tilemap Overlap { get; }

    public Vector3Int GetGridPosition(Vector3 _position);
    public GameObject GetInstantiatedObject(Vector3Int _location);
    public void SetTile(Vector3Int _location, TileBase _tile);
    public TileBase GetTile(Vector3Int _location);
    public bool HasTile(Vector3Int _location);
    public void MoveTile(Vector3Int _currentLocation, Vector3Int _nextLocation);
    public void StoreTile(Vector3Int _currentLocation, Vector3Int _nextLocation);
    public GameObject GetInstantiatedOverlappingObject(Vector3Int _currentLocation);
    public void RestoreTile(Vector3Int _currentLocation, Vector3Int _nextLocation);
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
    [SerializeField] private GameObject _foregroundObject;
    [SerializeField] private GameObject _overlapObject;
    public static GameControl Main;
    private AudioManager _audioManager;
    private Player _currentPlayer;

    private Tilemap _foreground;
    public Tilemap Foreground {get { return _foreground; } }

    private Tilemap _overlap;
    public Tilemap Overlap {get { return _overlap; } }
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
        _foreground = _foregroundObject.GetComponent<Tilemap>();
        _overlap = _overlapObject.GetComponent<Tilemap>();
        _foregroundObject.GetComponent<TilemapRenderer>().forceRenderingOff = true;
        _overlapObject.GetComponent<TilemapRenderer>().forceRenderingOff = true;
        SetPushableObjects();
        StartCoroutine(FadeInfromLastScene(Time.realtimeSinceStartup));
    }

    void SetPushableObjects()
    {
        IPushable[] pushableObjects = GetComponents<IPushable>();
        int tracker = 1;
        foreach (IPushable push_obj in pushableObjects)
        {
            push_obj.IdUse = tracker;
            tracker += 1;
        }
    }

    public Vector3Int GetGridPosition(Vector3 _position)
    {
        return _foreground.LocalToCell(_position);
    }

    public GameObject GetInstantiatedObject(Vector3Int _location)
    {
        return _foreground.GetInstantiatedObject(_location);
    }

    public void SetTile(Vector3Int _location, TileBase _tile)
    {
        _foreground.SetTile(_location, _tile);
    }
    public TileBase GetTile(Vector3Int _location)
    {
        return _foreground.GetTile(_location);
    }
    public bool HasTile(Vector3Int _location)
    {
        return _foreground.HasTile(_location);
    }
    public void MoveTile(Vector3Int _currentLocation, Vector3Int _nextLocation)
    {
        TileBase targetTile = _foreground.GetTile(_currentLocation);
        _foreground.SetTile(_nextLocation, targetTile);
        _foreground.SetTile(_currentLocation, null);
    }
    public void StoreTile(Vector3Int _currentLocation, Vector3Int _nextLocation)
    {
        TileBase targetTile = _foreground.GetTile(_currentLocation);
        _overlap.SetTile(_nextLocation, targetTile);
        // TileBase targetTile = _foreground.GetTile(_currentLocation);
        // GameObject storageObject = _foreground.GetInstantiatedObject(_nextLocation);
        // switch(storageObject.tag)
        // {
        //     case "trap":
        //         TrapHandle tmp_traphandle = storageObject.GetComponent<TrapHandle>();
        //         tmp_traphandle.StoreTile();
        //         break;
        //     case "switch":
        //         TrapSwitchHandle tmp_trapswitchhandle = storageObject.GetComponent<TrapSwitchHandle>();
        //         tmp_trapswitchhandle.StoreTile();
        //         break;
        // }
        _foreground.SetTile(_currentLocation, null);
    }
    public GameObject GetInstantiatedOverlappingObject(Vector3Int _currentLocation)
    {
        return _overlap.GetInstantiatedObject(_currentLocation);
    }
    public void RestoreTile(Vector3Int _currentLocation, Vector3Int _nextLocation)
    {
        TileBase targetTile = _overlap.GetTile(_currentLocation);
        _foreground.SetTile(_nextLocation, targetTile);
        _overlap.SetTile(_currentLocation, null);
    }
    public void FinishLevel()
    {
        _audioManager.PlaySound(SoundType.PassLevel, _currentPlayer.GetPosition());
        StartCoroutine(FadeToNextScene(SceneManager.GetActiveScene().buildIndex + 1, Time.realtimeSinceStartup));
    }
    // int Direction2Deg(float y, float x)
    // {
    //     float degrees = Mathf.Rad2Deg * Mathf.Atan2(y,x);
    //     int result = Mathf.RoundToInt(180f-degrees);

    //     return result;
    // }

    // Vector3 updatePosition(GameObject obj,Vector3 dir)
    // {
    //     Transform obj_transform = obj.GetComponent<Transform>();
    //     obj_transform.localPosition = dir;
    //     return obj_transform.localPosition;
    // }
    // void updateSprite(GameObject obj, Sprite sprite, Vector3 rotation)
    // {
    //     obj.GetComponent<Transform>().localEulerAngles = rotation;
    //     SpriteRenderer sp_rend  = obj.GetComponent<SpriteRenderer>();
    //     sp_rend.sprite = sprite;
    //     return;
    // }
    

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

    //}
}
