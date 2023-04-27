using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class pushable_script : MonoBehaviour
{
    public int idUse = 0; 
    public Vector3Int dir;
    public bool move;
    public int type;
    public bool crossing;
    private bool set;


    private Vector3Int gridPos;
    private Vector3 position_track;
    private GameObject grid;
    private GameObject instObj;
    //public TileBase ruleTile;
    public Vector3Int tileLoc;
    private GameObject player;


    private void Start()
    {
        position_track = gameObject.GetComponent<Transform>().localPosition;
        player = GameObject.FindGameObjectWithTag("GameController");
        grid = player.GetComponent<GameControl>().foreground;
        gridPos = grid.GetComponent<Tilemap>().LocalToCell(position_track) + dir;
        set = false;
        GameControl.current.onPushObject += PushableObject;
    }
    private void PushableObject(GameObject obj)
    {
        if (obj.GetComponent<pushable_script>().idUse == idUse && move)
        {
            StartCoroutine(MoveAsDirected());
        }
    }

    IEnumerator MoveAsDirected()
    {
        bool endLoop = false;
        while (!endLoop)
        {
            if (!set)
            {
                set = true;
                gridPos = grid.GetComponent<Tilemap>().LocalToCell(position_track) + dir;
                instObj = grid.GetComponent<Tilemap>().GetInstantiatedObject(gridPos);
                AudioSource.PlayClipAtPoint(GameControl.current.audioClips[7], position_track);
            }
            position_track = updatePosition(gameObject, position_track + (Time.deltaTime * new Vector3(dir.x, dir.y, 0) * 2f));
            if ((position_track - (gridPos + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
            {
                position_track = updatePosition(gameObject, gridPos + new Vector3(0.5f, 0.5f, 0));
                move = false;
                set = false;
                if (instObj != null)
                {
                    if (instObj.tag == "obstacle")
                    {
                        if (!instObj.GetComponent<state_chg>().state)
                        {
                            if (instObj.GetComponent<state_chg>().type == type) //gamecontrol should update the tiles.
                            {
                                instObj.GetComponent<state_chg>().state = true;
                                instObj.GetComponent<state_chg>().crossable = true;
                                instObj.GetComponent<SpriteRenderer>().sprite = instObj.GetComponent<state_chg>().sprites[instObj.GetComponent<state_chg>().sprites.Length - 1];
                                GameObject[] objects = GameObject.FindGameObjectsWithTag("trap");
                                foreach (GameObject obj in objects)
                                {
                                    if (obj.GetComponent<trap_state>().type == instObj.GetComponent<state_chg>().type)
                                    {
                                        obj.GetComponent<trap_state>().trap_enabled = false;
                                    }
                                }
                                AudioSource.PlayClipAtPoint(GameControl.current.audioClips[2], position_track);
                                grid.GetComponent<Tilemap>().SetTile(tileLoc, null);
                            }
                            else
                            {
                                crossing = true;
                                instObj.GetComponent<state_chg>().crossing = true;
                                instObj.GetComponent<state_chg>().storeTileandObject(new TileandObject(tileLoc, gameObject)); //make sure to make player set ruleTile.
                            }
                        }
                        else
                        {
                            crossing = true;
                            instObj.GetComponent<state_chg>().crossing = true;
                            instObj.GetComponent<state_chg>().storeTileandObject(new TileandObject(tileLoc, gameObject)); //make sure to make player set ruleTile.
                        }
                    }
                }
                else
                {
                    grid.GetComponent<Tilemap>().SetTile(gridPos, grid.GetComponent<Tilemap>().GetTile(tileLoc));
                    grid.GetComponent<Tilemap>().SetTile(tileLoc, null);
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
