using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;


public class GameControl : MonoBehaviour
{
    //Event System
    public static GameControl current;

    //grid system variables, make sure to set in advance.
    public Grid maingrid;
    public GameObject foreground;
    public GameObject background;
    public GameObject uilayout;
    public AudioClip[] audioClips;

    //Player variables
    //private string[] plr_spritenames = {"walk","bomb","torch","boss","burn","electricution","acid"}; //first part of names of all sprites based on state.
    public Sprite[] plr_walksprites;
    public Sprite[] plr_bombsprites;
    public Sprite[] plr_torchsprites;
    public Sprite[] plr_deathsprites;

    public bool plr_death;
    public Vector2Int death_use;
    public Vector2Int[] death_types = {
        new Vector2Int(0,18),
        new Vector2Int(19,42),
        
    };
    private bool fadeSet;

    private Vector3 plr_position;
    public Vector3Int plr_gridpos;
    private GameObject player;
    public bool plr_moving; //whether or not player is gliding across grid. 
    public int plr_state; //item being held, 1 = none, 2 = bomb, 3 = torch, 4 = deathbyboss, 5 = deathbyfire, 6 = deathbyelectricity, 7 = deathbyacid
    private Vector2 plr_direction;
    public Vector3Int plr_dir;
    private int plr_facing; //direction, player is facing in angle;
    private int plr_frame; //animation frame.
    public double plr_timerRT; //timer real time since event.
    private float plr_timer1; //timing frames
    private GameObject spot_obj;
    //gameplay variables
    private int plr_score;
    private List<int> inventory = new List<int>();
    //private string[] tools; //pickup bomb and torch, select which one is active.
    private int plr_seltool; //selected tool, index above.

    //get next tile properties
    private Vector3 obj_pos; 
    private Tilemap foreground_tilemap;
    private Vector3Int obj_gridpos;
    private GameObject nextTileObj;

    //public event UnityAction  

    public void Awake()
    {
        current = this;
    }
    //event setup
    public event Action<GameObject> onPickupItem;
    public event Action<GameObject> onPushObject;
    public void invokePickupItem(GameObject obj)
    {
        onPickupItem?.Invoke(obj);
    }
    public void invokePushObject(GameObject obj)
    {
        onPushObject?.Invoke(obj);
    }


    // Start is called before the first frame update
    void Start()
    {
        //plr_sprites = Resources.LoadAll<Sprite>("Assets/Sprites/GameImages/Stickman");
        //print(plr_sprites.Length);
        
        foreground.GetComponent<TilemapRenderer>().forceRenderingOff = true;
        player = GameObject.FindGameObjectWithTag("Player");
        plr_position = player.GetComponent<Transform>().localPosition;
        plr_gridpos = foreground.GetComponent<Tilemap>().LocalToCell(plr_position);
        plr_state = 1;
        plr_score = 0;
        fadeSet = true;
        StartCoroutine(fadeInfromLastScene(Time.realtimeSinceStartup));
        onPickupItem += PickupItem;
        //onPushObject += PushObject;
        GameObject[] pushableObjects = GameObject.FindGameObjectsWithTag("push");
        int tracker = 1;
        foreach (GameObject push_obj in pushableObjects)
        {
            push_obj.GetComponent<pushable_script>().IdUse = tracker;
            tracker += 1;
        }
        //UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ChangedActiveScene;
        //Object.Destroy(foreground.GetComponent<Tilemap>().GetTile<Tile>(plr_gridpos));
        //print(plr_gridpos);
        //AudioSource. audioClips[0] //(audioClips[0], new Vector3(0.5f, 0.05f, -10));
    }

    //public void PushObject(GameObject obj)
    //{
    //    if (nextTileObj == null)
    //    {
    //        //print(spot_obj.name);
    //        obj.GetComponent<pushable_script>().dir = plr_dir;
    //        obj.GetComponent<pushable_script>().move = true;
    //        obj.GetComponent<pushable_script>().tileLoc = obj_gridpos;
    //        plr_moving = true;
    //        plr_gridpos += plr_dir;
    //        return;
    //        //foreground_tilemap.SetTile(obj_gridpos + plr_dir, foreground_tilemap.GetTile(obj_gridpos));
    //        //foreground_tilemap.SetTile(obj_gridpos, null);
    //    }
    //    else
    //    {
    //        if (nextTileObj.tag == "obstacle")
    //        {
    //            //if (!spot_obj.GetComponent<pushable_script>().crossing)
    //            //{
    //            obj.GetComponent<pushable_script>().dir = plr_dir;
    //            obj.GetComponent<pushable_script>().move = true;
    //            obj.GetComponent<pushable_script>().tileLoc = obj_gridpos;
    //            plr_moving = true;
    //            plr_gridpos += plr_dir;
    //            return;
    //            //}

    //        }

    //    }
    //    //print(spot_obj.GetComponent<pushable_script>().move);
    //}
    public void PickupItem(GameObject item)
    {
        switch (item.GetComponent<pickup_type>().TypeOfPickup)
        {
            case PickUpEnum.Coin:
                //print("coin");
                AudioSource.PlayClipAtPoint(audioClips[0], item.GetComponent<Transform>().localPosition);
                plr_score += 100;
                foreground.GetComponent<Tilemap>().SetTile(plr_gridpos, null);

                return;
            case PickUpEnum.CoinChest:
                //print("coin_chest");
                AudioSource.PlayClipAtPoint(audioClips[1], item.GetComponent<Transform>().localPosition);
                plr_score += 500;
                foreground.GetComponent<Tilemap>().SetTile(plr_gridpos, null);
                return;
            default:
                AudioSource.PlayClipAtPoint(audioClips[0], item.GetComponent<Transform>().localPosition);
                addToInventory(item);
                foreground.GetComponent<Tilemap>().SetTile(plr_gridpos, null);
                return;

        }
    }
    // Update is called once per frame
    void Update()
    {
        //Vector3Int dir = new Vector3Int(Mathf.RoundToInt(Mathf.Cos(plr_facing)), -Mathf.RoundToInt(Mathf.Sin(plr_facing)), 0);
        //print(dir);
        
        if (!plr_death)
        {
            Vector2 plr_control = Input.main.PlayerDir; //new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (!plr_moving && plr_control.magnitude > 0)
            {

                plr_direction = new Vector2(plr_control.x, plr_control.y);
                plr_facing = Direction2Deg(plr_direction.y, -plr_direction.x);
                plr_dir = new Vector3Int(Mathf.RoundToInt(Mathf.Cos(Mathf.Deg2Rad * plr_facing)), Mathf.RoundToInt(Mathf.Sin(Mathf.Deg2Rad * plr_facing)), 0);
                //GameObject target = foreground.GetComponent<Tilemap>().GetInstantiatedObject(plr_gridpos + plr_dir);
                //GameObject spot_obj = foreground.GetComponent<Tilemap>().GetInstantiatedObject(plr_gridpos + plr_dir);
                //if spot_obj.
                //print(spot_obj.name);
                if (!foreground.GetComponent<Tilemap>().HasTile(plr_gridpos + plr_dir))
                {
                    plr_moving = true;
                    plr_gridpos += plr_dir;
                }
                else
                {
                    //spot_obj = null;
                    spot_obj = null;
                    spot_obj = foreground.GetComponent<Tilemap>().GetInstantiatedObject(plr_gridpos + plr_dir);
                    if (spot_obj != null)
                    {
                        obj_pos = spot_obj.GetComponent<Transform>().localPosition;
                        foreground_tilemap = foreground.GetComponent<Tilemap>();
                        obj_gridpos = foreground_tilemap.LocalToCell(obj_pos);
                        nextTileObj = foreground_tilemap.GetInstantiatedObject(obj_gridpos + plr_dir);
                        switch (spot_obj.tag)
                        {
                            case "wall":
                                //print("wall");
                                return;
                            case "pickup":
                                //print("pickup");
                                plr_moving = true;
                                plr_gridpos += plr_dir;
                                invokePickupItem(spot_obj);
                                return;
                                
                            case "push":
                                //print("pushable object detected, make sure plr is not moving diagnally.");
                                //print(plr_dir.magnitude);
                                if (plr_dir.magnitude == 1)
                                {
                                    if (!spot_obj.GetComponent<pushable_script>().Crossing) //move to spot_obj's location if it is currently crossing and saved to a different tile.
                                    {
                                        if (nextTileObj != null)
                                        {
                                            if (nextTileObj.tag == "obstacle" && nextTileObj.GetComponent<state_chg>().type < 2)
                                            {
                                                spot_obj.GetComponent<pushable_script>().SetProperties(plr_dir, true, obj_gridpos);
                                                invokePushObject(spot_obj);
                                                plr_moving = true;
                                                plr_gridpos += plr_dir;
                                            }
                                        }
                                        else
                                        {
                                            spot_obj.GetComponent<pushable_script>().SetProperties(plr_dir, true, obj_gridpos);
                                            invokePushObject(spot_obj);
                                            plr_moving = true;
                                            plr_gridpos += plr_dir;
                                        }
                                    }
                                    else
                                    {
                                        plr_moving = true;
                                        plr_gridpos += plr_dir;
                                    }


                                }
                                //else
                                //{
                                    
                                //}
                                //if (plr_dir.magnitude == 1 && !spot_obj.GetComponent<pushable_script>().crossing)
                                //{

                                //    spot_obj.GetComponent<pushable_script>().dir = plr_dir;
                                //    spot_obj.GetComponent<pushable_script>().move = true;
                                //    plr_moving = true;
                                //    plr_gridpos += plr_dir;
                                //}
                                //else if (plr_dir.magnitude == 1 && spot_obj.GetComponent<pushable_script>().crossing)
                                //{
                                //    plr_moving = true;
                                //    plr_gridpos += plr_dir;
                                //}

                                return;
                            case "obstacle":
                                if (plr_dir.magnitude == 1)
                                {
                                    if (spot_obj.tag == "obstacle")
                                    {
                                        int type_obstacle = spot_obj.GetComponent<state_chg>().type;
                                        //print(inventory.Contains(type_obstacle+1));
                                        //if (type_obstacle < 2)
                                        //{
                                            if (inventory.Contains(type_obstacle + 1) && type_obstacle >1 && type_obstacle < 6)
                                            {
                                                AudioSource.PlayClipAtPoint(audioClips[4], spot_obj.GetComponent<Transform>().localPosition);
                                                inventory.Remove(type_obstacle + 1);
                                                StartCoroutine(playOpenDoorAnimation(spot_obj));
                                                return;
                                            }
                                            if (spot_obj.GetComponent<state_chg>().crossing)
                                            {
                                                if (nextTileObj != null)
                                                {
                                                    if (nextTileObj.tag != "obstacle")
                                                    {
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        if (nextTileObj.GetComponent<state_chg>().type >= 2 || nextTileObj.GetComponent<state_chg>().crossing)
                                                        {
                                                            return;
                                                        }
                                                    }
                                                }
                                                //print(nextTileObj);
                                                TileandObject tileAndObject = spot_obj.GetComponent<state_chg>().getTileandObjectBack();
                                                tileAndObject.pushableObject.GetComponent<pushable_script>().SetProperties(plr_dir, true, tileAndObject.pushableTileLoc);
                                                invokePushObject(tileAndObject.pushableObject);
                                                plr_moving = true;
                                                plr_gridpos += plr_dir;
                                                spot_obj.GetComponent<state_chg>().crossing = false;
                                                spot_obj.GetComponent<state_chg>().storeTileandObject(new TileandObject(new Vector3Int(), null));
                                                return;                                                
                                                
                                            }
                                            else
                                            {
                                                if (!spot_obj.GetComponent<state_chg>().crossable)
                                                {
                                                    return;
                                                }
                                            }
                                        //}
                                        //else
                                        //{
                                            
                                        //}
                                        //if (type_obstacle >= 2 && type_obstacle <= 5 && inventory.Contains(type_obstacle + 1)) //key door of any color
                                        //{
                                            
                                        //}
                                    }
                                    plr_moving = true;
                                    plr_gridpos += plr_dir;
                                }
                                

                                return;
                            case "trap":
                                if (spot_obj.GetComponent<trap_state>().trap_enabled)
                                {
                                    //death sequence based on trap type
                                    death_use = death_types[spot_obj.GetComponent<trap_state>().type];
                                    plr_frame = death_use.x;
                                    plr_death = true;
                                    AudioSource.PlayClipAtPoint(audioClips[3], player.GetComponent<Transform>().localPosition);
                                }
                                else
                                {
                                    plr_moving = true;
                                    plr_gridpos += plr_dir;
                                }
                                return;
                            case "Finish":
                                AudioSource.PlayClipAtPoint(audioClips[5], player.GetComponent<Transform>().localPosition);                                
                                plr_moving = true;
                                plr_gridpos += plr_dir;
                                StartCoroutine(fadeToNextScene(SceneManager.GetActiveScene().buildIndex + 1, Time.realtimeSinceStartup));

                                return;
                            default:
                                //print("unknown");
                                plr_moving = true;
                                plr_gridpos += plr_dir;
                                return;

                        }
                    }
                    

                    //if (!spot_obj.CompareTag("wall"))
                    //{
                    //    plr_moving = true;
                    //    plr_gridpos += plr_dir;
                    //}
                }
                //else if (target && !target.CompareTag("wall"))
                //{
                //    plr_moving = true;
                //    plr_gridpos += plr_dir;
                //}
                //handle collectibles
                //print(plr_gridpos);
            }
            else if (plr_moving)
            {
                plr_timer1 += 1;
                plr_position = updatePosition(player, plr_position + (Time.deltaTime * new Vector3(plr_dir.x, plr_dir.y, 0) * 2f));
                if (spot_obj != null)
                {
                    if (spot_obj.CompareTag("obstacle") && (plr_position - spot_obj.GetComponent<Transform>().localPosition).magnitude > 0.5 && spot_obj.GetComponent<state_chg>().type == 1)
                    {
                        spot_obj.GetComponent<state_chg>().crossing = false;
                        GameObject[] objects = GameObject.FindGameObjectsWithTag("trap");
                        foreach (GameObject obj in objects)
                        {
                            if ((obj.GetComponent<trap_state>().type == spot_obj.GetComponent<state_chg>().type) && (!spot_obj.GetComponent<state_chg>().state))
                            {
                                obj.GetComponent<trap_state>().trap_enabled = true;
                            }
                        }
                    }
                    else if (spot_obj.CompareTag("obstacle") && (plr_position - spot_obj.GetComponent<Transform>().localPosition).magnitude <= 0.5 && spot_obj.GetComponent<state_chg>().type == 1)
                    {
                        spot_obj.GetComponent<state_chg>().crossing = true;
                        GameObject[] objects = GameObject.FindGameObjectsWithTag("trap");
                        foreach (GameObject obj in objects)
                        {
                            if ((obj.GetComponent<trap_state>().type == spot_obj.GetComponent<state_chg>().type) && (!spot_obj.GetComponent<state_chg>().state))
                            {
                                obj.GetComponent<trap_state>().trap_enabled = false;
                            }
                        }
                    }
                }
                //print((plr_position - (plr_gridpos + new Vector3(0.5f, 0.5f, 0))).magnitude);
                if ((plr_position - (plr_gridpos + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
                {
                    plr_position = updatePosition(player, plr_gridpos + new Vector3(0.5f, 0.5f, 0));
                    plr_timer1 = 0;
                    plr_moving = false;
                }
                else
                {

                    if (plr_timer1 > 15)
                    {
                        plr_timer1 = 0;
                        plr_frame += 1;
                        switch (plr_state)
                        {
                            case 1:
                                if (plr_frame >= plr_walksprites.Length)
                                {
                                    plr_frame = 0;
                                }
                                updateSprite(player, plr_walksprites[plr_frame], new Vector3(0, 0, plr_facing - 90));
                                return;
                            case 2:
                                if (plr_frame >= plr_bombsprites.Length)
                                {
                                    plr_frame = 0;
                                }
                                updateSprite(player, plr_bombsprites[plr_frame], new Vector3(0, 0, plr_facing - 90));
                                return;
                            case 3:
                                if (plr_frame >= (plr_torchsprites.Length - 18))
                                {
                                    plr_frame = 0;
                                }
                                updateSprite(player, plr_torchsprites[plr_frame], new Vector3(0, 0, plr_facing - 90));
                                return;
                        }
                    }
                }
            }
            else if (!plr_moving)
            {

                switch (plr_state)
                {
                    case 1:
                        updateSprite(player, plr_walksprites[1], new Vector3(0, 0, plr_facing - 90));
                        return;
                    case 2:
                        updateSprite(player, plr_bombsprites[1], new Vector3(0, 0, plr_facing - 90));
                        return;
                    case 3:
                        updateSprite(player, plr_torchsprites[1], new Vector3(0, 0, plr_facing - 90));
                        return;
                }


            };
        }
        else
        {
            plr_timer1 += 1;
            if (plr_timer1 > 25)
            {
                plr_timer1 = 0;
                plr_frame += 1;
                if (plr_frame >= death_use.y)
                {
                    plr_frame = death_use.y;
                    if ((Time.realtimeSinceStartup - plr_timerRT) > 2 && !fadeSet)
                    {
                        fadeSet = true;
                        //plr_frame = death_use.x;
                        //idea - keep time since plr_death set, wait until a certain number of seconds.
                        //Use Time.realtimeSinceStartupAsDouble as similar to tick() from ROBLOX, difference is time that elapsed since given event
                        StartCoroutine(fadeToNextScene(SceneManager.GetActiveScene().buildIndex, Time.realtimeSinceStartup));
                        //restart?
                    }


                }
                updateSprite(player, plr_deathsprites[plr_frame], new Vector3(0, 0, plr_facing - 90));
            }
        }
        

    }

    int Direction2Deg(float y, float x)
    {
        float degrees = Mathf.Rad2Deg * Mathf.Atan2(y,x);
        int result = Mathf.RoundToInt(180f-degrees);

        return result;
    }

    Vector3 updatePosition(GameObject obj,Vector3 dir)
    {
        Transform obj_transform = obj.GetComponent<Transform>();
        obj_transform.localPosition = dir;
        return obj_transform.localPosition;
    }
    void updateSprite(GameObject obj, Sprite sprite, Vector3 rotation)
    {
        obj.GetComponent<Transform>().localEulerAngles = rotation;
        SpriteRenderer sp_rend  = obj.GetComponent<SpriteRenderer>();
        sp_rend.sprite = sprite;
        return;
    }
    void addToInventory(GameObject obj)
    {
        int typeOfPickup = (int) obj.GetComponent<pickup_type>().TypeOfPickup;
        inventory.Add(typeOfPickup);//.SetValue(typeOfPickup, inventory.Length);
        return;
    }

    IEnumerator fadeInfromLastScene(double startTime)
    {
        bool endAnim = false;
        while (!endAnim)
        {
            if ((Time.realtimeSinceStartup - startTime)>2)
            {
                endAnim = true;
                fadeSet = false;
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
        bool endAnim = false;
        //int curFrame = 1;
        //int timer1 = 0;
        while (!endAnim)
        {
            //curFrame += 1;
            if ((Time.realtimeSinceStartup - startTime)>2)
            {
                endAnim = true;
                fadeSet = false;
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
        

    IEnumerator playOpenDoorAnimation(GameObject obj)
    {
        bool endAnim = false;
        int curFrame = 1;
        int timer1 = 0;
        //updateSprite(obj, obj.GetComponent<state_chg>().sprites[curFrame], new Vector3());
        while (!endAnim && obj != null)
        {
            //print(timer1);
            //print(curFrame);
            timer1 += 1;
            if (timer1 > 20)
            {
                timer1 = 0;
                
                
                
                curFrame = curFrame + 1 >= obj.GetComponent<state_chg>().sprites.Length-1 ? obj.GetComponent<state_chg>().sprites.Length-1 : curFrame + 1;
                updateSprite(obj, obj.GetComponent<state_chg>().sprites[curFrame], new Vector3());
                if (curFrame >= obj.GetComponent<state_chg>().sprites.Length-1)
                {

                    //curFrame = 0;
                    
                    Vector3Int objGridPos = foreground.GetComponent<Tilemap>().LocalToCell(obj.GetComponent<Transform>().localPosition);
                    foreground.GetComponent<Tilemap>().SetTile(objGridPos, null);
                    endAnim = true;
                }
                
            }
            
            yield return new WaitForEndOfFrame();
        }
    }

    //bool playAnimation(GameObject obj)
    //{
    //    bool endAnim = false;
    //    int curFrame = 0;
    //    int timer1 = 0;
    //    while (!endAnim)
    //    {
    //        timer1 += 1;
    //        if (timer1>15)
    //        {
    //            timer1 = 0;
    //            curFrame += 1;
    //            if (curFrame >= obj.GetComponent<state_chg>().sprites.Length)
    //            { 
    //                curFrame = 0;
    //                endAnim = true;
    //            }
    //            updateSprite(obj, obj.GetComponent<state_chg>().sprites[curFrame], new Vector3());            
    //        }
    //    }
    //    return true;
    //    //WaitWhile.

    //}
}
