using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public enum GameState {Cutscene, Menu, Pause, LevelPlay, LevelEnd}
public enum DeathType { Standard, Burn, Electricution, Acid };
public interface IGameControl
{
    public GameState State { get; }
    public Tilemap Foreground { get; }

    public Vector3Int GetGridPosition(Vector3 _position);
    public GameObject GetInstantiatedObject(Vector3Int _location);
    public void SetTile(Vector3Int _location, TileBase _tile);
    public TileBase GetTile(Vector3Int _location);
    public bool HasTile(Vector3Int _location);
}

public class GameControl : MonoBehaviour, IGameControl
{
    [SerializeField] private GameObject _foregroundObject;
    
    public static GameControl Main;
    
    private Tilemap _foreground;
    public Tilemap Foreground {get { return _foreground; } }

    private GameState _state;
    public GameState State {get { return _state; } }

    public event EventHandler onGameStart;
    public event EventHandler onGameFadeToNextLevel;

    //public event UnityAction  

    void Awake()
    {
        Main = this;  
    }
    // Start is called before the first frame update
    void Start()
    {
        _foreground = _foregroundObject.GetComponent<Tilemap>();
        _foregroundObject.GetComponent<TilemapRenderer>().forceRenderingOff = true;
        SetPushableObjects();
        StartCoroutine(fadeInfromLastScene(Time.realtimeSinceStartup));
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
    

    IEnumerator fadeInfromLastScene(double startTime)
    {
        bool endAnim = false;
        while (!endAnim)
        {
            if ((Time.realtimeSinceStartup - startTime)>2)
            {
                endAnim = true;
                onGameStart?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                GameObject fadeScreen = GameObject.FindGameObjectWithTag("fade");
                fadeScreen.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 1f - (float)((Time.realtimeSinceStartup - startTime) / 2));
            }
            yield return new WaitForEndOfFrame();
        }
        
    }
    IEnumerator fadeToNextScene(int index, double startTime)
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
        

   

    //}
}
