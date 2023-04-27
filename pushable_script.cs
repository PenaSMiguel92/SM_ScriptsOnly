using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PushableType {Crate, Boulder, LimitedCrate, Snowball}

public interface IPushable 
{
    public int IdUse { get; set; }
    public PushableType Type { get; }
    public bool Move { get; }
    public bool Set { get; }
    public bool Crossing { get; }
    public Vector3Int MoveDirection { get; }
    public Vector3Int TileLocation { get; }
    

    public void SetProperties(Vector3Int dir, bool move, Vector3Int tileLoc);
}

public class pushable_script : MonoBehaviour, IPushable
{
    [SerializeField] PushableType _type;
    int _idUse = 0; //Note that it is set by another class.
    bool _move;
    bool _crossing;
    bool _set;
    Vector3Int _dir;
    Vector3Int _tileLocation;

    Vector3Int _gridPos;
    Vector3 _positionTrack;
    GameObject _grid;
    GameObject _instObj;

    GameControl _mainControl;

    public PushableType Type
    {
        get { return _type; }
    }

    public int IdUse 
    {
        get { return _idUse; }
        set { _idUse = value; }
    }

    public bool Move
    {
        get { return _move; }
    }

    public bool Crossing
    {
        get { return _crossing; }
    }

    public bool Set
    {
        get { return _set; }
    }

    public Vector3Int MoveDirection
    {
        get { return _dir; }
    }

    public Vector3Int TileLocation
    {
        get { return _tileLocation; }
    }

    void Awake()
    {
        _mainControl = GameControl.current;
    }

    void Start()
    {
        _positionTrack = gameObject.GetComponent<Transform>().localPosition;
        _grid = _mainControl.foreground;
        _gridPos = _grid.GetComponent<Tilemap>().LocalToCell(_positionTrack) + _dir;
        _set = false;
        _mainControl.onPushObject += PushableObject;
    }
    public void SetProperties(Vector3Int dir, bool move, Vector3Int tileLoc)
    {
        _dir = dir;
        _move = move;
        _tileLocation = tileLoc;
    }
    void PushableObject(GameObject obj)
    {
        if (obj.GetComponent<pushable_script>().IdUse == _idUse && _move)
        {
            StartCoroutine(MoveAsDirected());
        }
    }

    IEnumerator MoveAsDirected()
    {
        bool endLoop = false;
        while (!endLoop)
        {
            if (!_set)
            {
                _set = true;
                _gridPos = _grid.GetComponent<Tilemap>().LocalToCell(_positionTrack) + _dir;
                _instObj = _grid.GetComponent<Tilemap>().GetInstantiatedObject(_gridPos);
                AudioSource.PlayClipAtPoint(GameControl.current.audioClips[7], _positionTrack);
            }
            _positionTrack = updatePosition(gameObject, _positionTrack + (Time.deltaTime * new Vector3(_dir.x, _dir.y, 0) * 2f));
            if ((_positionTrack - (_gridPos + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
            {
                _positionTrack = updatePosition(gameObject, _gridPos + new Vector3(0.5f, 0.5f, 0));
                _move = false;
                _set = false;
                if (_instObj != null)
                {
                    if (_instObj.tag == "obstacle")
                    {
                        if (!_instObj.GetComponent<state_chg>().state)
                        {
                            if (_instObj.GetComponent<state_chg>().type == (int) _type) //gamecontrol should update the tiles.
                            {
                                _instObj.GetComponent<state_chg>().state = true;
                                _instObj.GetComponent<state_chg>().crossable = true;
                                _instObj.GetComponent<SpriteRenderer>().sprite = _instObj.GetComponent<state_chg>().sprites[_instObj.GetComponent<state_chg>().sprites.Length - 1];
                                GameObject[] objects = GameObject.FindGameObjectsWithTag("trap");
                                foreach (GameObject obj in objects)
                                {
                                    if (obj.GetComponent<trap_state>().type == _instObj.GetComponent<state_chg>().type)
                                    {
                                        obj.GetComponent<trap_state>().trap_enabled = false;
                                    }
                                }
                                AudioSource.PlayClipAtPoint(GameControl.current.audioClips[2], _positionTrack);
                                _grid.GetComponent<Tilemap>().SetTile(_tileLocation, null);
                            }
                            else
                            {
                                _crossing = true;
                                _instObj.GetComponent<state_chg>().crossing = true;
                                _instObj.GetComponent<state_chg>().storeTileandObject(new TileandObject(_tileLocation, gameObject)); //make sure to make player set ruleTile.
                            }
                        }
                        else
                        {
                            _crossing = true;
                            _instObj.GetComponent<state_chg>().crossing = true;
                            _instObj.GetComponent<state_chg>().storeTileandObject(new TileandObject(_tileLocation, gameObject)); //make sure to make player set ruleTile.
                        }
                    }
                }
                else
                {
                    _grid.GetComponent<Tilemap>().SetTile(_gridPos, _grid.GetComponent<Tilemap>().GetTile(_tileLocation));
                    _grid.GetComponent<Tilemap>().SetTile(_tileLocation, null);
                }
                endLoop = true;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    Vector3 updatePosition(GameObject obj, Vector3 dir)
    {
        Transform obj_transform = obj.GetComponent<Transform>();
        obj_transform.localPosition = dir;
        return obj_transform.localPosition;
    }
}
