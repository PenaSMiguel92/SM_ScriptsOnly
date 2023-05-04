using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TileandObject
{
    public Vector3Int pushableTileLoc;
    public GameObject pushableObject;

    public TileandObject(Vector3Int _pushableTileLoc,GameObject _pushableObject)
    {
        pushableTileLoc = _pushableTileLoc;
        pushableObject = _pushableObject;
    }
}
public interface IStateChange 
{
    public bool State { get; set; }
    public bool Crossable { get; set; }
    public bool Crossing { get; set; }
    public TrapType Type { get; }
    public TileandObject getTileandObjectBack();
    public void storeTileandObject(TileandObject _tileandobject);

}


public class state_chg : MonoBehaviour, IStateChange
{
    [SerializeField] private TrapType _type; // 0 - hole, 1 - electric switch
    public TrapType Type {get { return _type; } }
    [SerializeField] private Sprite[] _sprites; //sprites to use.
    [SerializeField] private AudioClip _soundUse;

    private bool _state; //whether or not disabled with block, false - 
    public bool State {get { return _state; } set { _state = value; } }
    private bool _crossable;
    public bool Crossable {get { return _crossable; } set { _crossable = value; } }
    private bool _crossing;
    public bool Crossing {get { return _crossing; } set { _crossing = value; } }
    
    
    
    private bool _endAnim;
    private Vector3Int _pushableTileLoc;
    private GameObject _pushableObject;
    private Vector3Int _tileLoc;
    private GameControl _mainControl;
    private AudioManager _audioManager;

    
    void Start()
    {
        _mainControl = GameControl.Main;
        _audioManager = AudioManager.Main;
        _mainControl.onGameStart += OnGameStart;
    }
    void OnGameStart(object _sender, EventArgs _e)
    {
        _state = false;
        gameObject.GetComponent<SpriteRenderer>().sprite = _sprites[0];
        _tileLoc = _mainControl.GetGridPosition(gameObject.GetComponent<Transform>().localPosition);
    }
    void Update()
    {
        if (!_state && (int) _type < 2)
        {
            int crossingTst = _crossing ? 1 : 0;
            gameObject.GetComponent<SpriteRenderer>().sprite = _sprites[crossingTst];
        }
        if (!_state && (int) _type == 6)
        {
            int coin_sum = 0;
            GameObject[] coins = GameObject.FindGameObjectsWithTag("pickup");
            foreach (GameObject coin in coins)
            {
                if ((int) coin.GetComponent<pickup_type>().TypeOfPickup < 3)
                {
                    coin_sum += 1;
                }
            }
            if (coin_sum < 1)
            {
                _state = true;
                _audioManager.PlaySound(SoundType.Explosion, gameObject.GetComponent<Transform>().localPosition);
                //StartCoroutine(playAnimation(gameObject, new Vector2(0, 20), _sprites, false));
            }
        }
        if (_state && _crossable)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = _sprites[_sprites.Length - 1];
        }
        if (_state && (int) _type == 6 && _endAnim)
        {
            _mainControl.SetTile(_tileLoc, null);
            Destroy(gameObject);
        }
        
    }

    void updateSprite(GameObject obj, Sprite sprite, Vector3 rotation)
    {
        obj.GetComponent<Transform>().localEulerAngles = rotation;
        SpriteRenderer sp_rend = obj.GetComponent<SpriteRenderer>();
        sp_rend.sprite = sprite;
        return;
    }
    IEnumerator playAnimation(GameObject obj, Vector2 frames, Sprite[] sprites, bool loop) //frames is vect2d with x being start and y being end frames
    {
        _endAnim = false;
        int curFrame = 0;
        int timer1 = 0;
        while (!_endAnim)
        {
            timer1 += 1;
            if (timer1 > 20)
            {
                timer1 = 0;


                curFrame += 1;
                curFrame = curFrame > Mathf.RoundToInt(frames.y)+1 ? Mathf.RoundToInt(frames.x) : curFrame;
                if (curFrame >= Mathf.RoundToInt(frames.y))
                {
                    curFrame = Mathf.RoundToInt(frames.x);
                    if (!loop)
                    {
                        _endAnim = true;
                    }
                }

            }
            updateSprite(obj, sprites[curFrame], new Vector3(0, 0, 0)); //static class, so no rotation necessary.
            yield return new WaitForEndOfFrame();
        }
    }
    public TileandObject getTileandObjectBack()
    {
        return new TileandObject(_pushableTileLoc, _pushableObject);
    }

    public void storeTileandObject(TileandObject _tileandobject)
    {
        _pushableTileLoc = _tileandobject.pushableTileLoc;
        _pushableObject = _tileandobject.pushableObject;
    }

     // IEnumerator playOpenDoorAnimation(GameObject obj)
    // {
    //     bool endAnim = false;
    //     int curFrame = 1;
    //     int timer1 = 0;
    //     //updateSprite(obj, obj.GetComponent<state_chg>().sprites[curFrame], new Vector3());
    //     while (!endAnim && obj != null)
    //     {
    //         //print(timer1);
    //         //print(curFrame);
    //         timer1 += 1;
    //         if (timer1 > 20)
    //         {
    //             timer1 = 0;
                
                
                
    //             curFrame = curFrame + 1 >= obj.GetComponent<state_chg>().sprites.Length-1 ? obj.GetComponent<state_chg>().sprites.Length-1 : curFrame + 1;
    //             updateSprite(obj, obj.GetComponent<state_chg>().sprites[curFrame], new Vector3());
    //             if (curFrame >= obj.GetComponent<state_chg>().sprites.Length-1)
    //             {

    //                 //curFrame = 0;
                    
    //                 Vector3Int objGridPos = foreground.GetComponent<Tilemap>().LocalToCell(obj.GetComponent<Transform>().localPosition);
    //                 foreground.GetComponent<Tilemap>().SetTile(objGridPos, null);
    //                 endAnim = true;
    //             }
                
    //         }
            
    //         yield return new WaitForEndOfFrame();
    //     }
    // }
}
