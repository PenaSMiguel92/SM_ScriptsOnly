using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class enemy_ai : MonoBehaviour
{
    public int enemy_type;
    private int[] chancesBehaviour;
    //public GameObject foreground;
    public Sprite[] enemy_sprites;
    public Sprite[] enemy_deathsprites;

    private bool enemy_death;
    private GameObject main_system;
    private GameObject foreground;
    private Vector3 ai_position;
    private Vector3Int ai_gridpos;
    private int ai_state;
    private bool ai_moving;
    private Vector2 ai_direction;
    private int ai_facing;
    private Vector3Int ai_dir;
    private int ai_timer1;
    private int ai_frame;
    private GameObject spot_obj;
    private IEnumerator walkAnimation;
    //private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        main_system = GameObject.FindGameObjectWithTag("GameController");
        foreground = main_system.GetComponent<GameControl>().foreground;
        //plr_sprites = Resources.LoadAll<Sprite>("Assets/Sprites/GameImages/Stickman");
        //print(plr_sprites.Length);

        //foreground.GetComponent<TilemapRenderer>().forceRenderingOff = true;
        //gameObject = GameObject.FindGameObjectWithTag("Player");
        ai_position = gameObject.GetComponent<Transform>().localPosition;
        ai_gridpos = main_system.GetComponent<GameControl>().foreground.GetComponent<Tilemap>().LocalToCell(ai_position);
        ai_state = 1;
        //plr_score = 0;
        //Object.Destroy(foreground.GetComponent<Tilemap>().GetTile<Tile>(ai_gridpos));
        //print(ai_gridpos);
        //AudioSource. audioClips[0] //(audioClips[0], new Vector3(0.5f, 0.05f, -10));
        switch(enemy_type)
        {
            case 1: //guard
                chancesBehaviour = new int[] {0,10,11,100,0,0,0,0,0,0}; //lower and upper bounds for select rnd direction and walk(min,max), keep walking(min,max), attack at current location (min, max), follow player (min,max),  
                return;
            case 2: //gatherer
                chancesBehaviour = new int[] { 0, 8,9,40, 41, 95, 96, 100 }; //lower and upper bounds for select rnd direction and walk(min,max), keep walking(min,max), attack at current location (min, max), follow player (min,max),  
                return;
            case 3: //ninja
                chancesBehaviour = new int[] { 0, 6,7,35, 36, 65, 66, 100}; //lower and upper bounds for select rnd direction and walk(min,max), keep walking(min,max), attack at current location (min, max), follow player (min,max),  
                return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3Int dir = new Vector3Int(Mathf.RoundToInt(Mathf.Cos(ai_facing)), -Mathf.RoundToInt(Mathf.Sin(ai_facing)), 0);
        //print(dir);
        if (!enemy_death)
        {
            enemyAIMakeDecision();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if ((!main_system.GetComponent<GameControl>().plr_death)&&((player.GetComponent<Transform>().localPosition - gameObject.GetComponent<Transform>().localPosition).magnitude < 0.707))
            {
                main_system.GetComponent<GameControl>().death_use = main_system.GetComponent<GameControl>().death_types[0];
                main_system.GetComponent<GameControl>().plr_death = true;
                AudioSource.PlayClipAtPoint(main_system.GetComponent<GameControl>().audioClips[3], player.GetComponent<Transform>().localPosition);
                main_system.GetComponent<GameControl>().plr_timerRT = Time.realtimeSinceStartup;
            }
        }
    }

    int Direction2Deg(float y, float x)
    {
        float degrees = Mathf.Rad2Deg * Mathf.Atan2(y, x);
        int result = Mathf.RoundToInt(180f - degrees);

        return result;
    }

    Vector3 updatePosition(GameObject obj, Vector3 dir)
    {
        Transform obj_transform = obj.GetComponent<Transform>();
        obj_transform.localPosition = dir;
        return obj_transform.localPosition;
    }
    void updateSprite(GameObject obj, Sprite sprite, Vector3 rotation)
    {
        obj.GetComponent<Transform>().localEulerAngles = rotation;
        SpriteRenderer sp_rend = obj.GetComponent<SpriteRenderer>();
        sp_rend.sprite = sprite;
        return;
    }

    void enemyAIMakeDecision()
    {
        switch (ai_state)
        {
            case 1: //standing, make choice of motion type
                int rndChoice = Mathf.RoundToInt(Random.value*100);
                if (rndChoice > chancesBehaviour[0] && rndChoice <= chancesBehaviour[1]) //80% chance of choosing random direction.
                {
                    selectRandomDirection();
                    ai_state = 2; //walk along selected direction
                }
                else if (rndChoice > chancesBehaviour[2] && rndChoice <= chancesBehaviour[3]) //20% chance of striking at current location if enemy_type other than guard and select a random direction afterwards. Chances change according to enemy type!
                {

                    ai_state = 2; //keep walking
                }
                else if (rndChoice > chancesBehaviour[4] && rndChoice <= chancesBehaviour[5])
                {
                    //strike at current location w/ function and then commence walking.
                    //selectRandomDirection();
                    //ai_state = 2;
                    
                }
                else if (rndChoice > chancesBehaviour[6] && rndChoice <= chancesBehaviour[7])
                {
                    //follow player
                }
                //anything other than guard can occasionally follow player!
                return;
            case 2: //walking
                //print("walking");
                if (!ai_moving)
                {
                    ai_moving = true;
                    ai_gridpos += ai_dir;
                    //print("Coroutine started");
                    walkAnimation = playAnimation(gameObject, new Vector2(0, 11), enemy_sprites, true);
                    //walkAnimation = StartCoroutine();
                    StartCoroutine(walkAnimation);
                }
                else
                {
                    if ((ai_position - (ai_gridpos + new Vector3(0.5f, 0.5f, 0))).magnitude <= 0.05)
                    {
                        ai_position = updatePosition(gameObject, ai_gridpos + new Vector3(0.5f, 0.5f, 0));
                        //plr_timer1 = 0;
                        ai_moving = false;
                        StopCoroutine(walkAnimation);
                        ai_state = 1;
                    }
                    else
                    {
                        spot_obj = foreground.GetComponent<Tilemap>().GetInstantiatedObject(ai_gridpos);

                        if (spot_obj != null)
                        {
                            if (spot_obj.tag == "obstacle")
                            {
                                if (spot_obj.GetComponent<state_chg>().crossable){
                                    ai_position = updatePosition(gameObject, ai_position + (Time.deltaTime * new Vector3(ai_dir.x, ai_dir.y, 0) * 2f));
                                    return;
                                }
                            }
                            else if (spot_obj.tag == "pickup" || spot_obj.tag == "enemy" || spot_obj.tag == "Player")
                            {
                                ai_position = updatePosition(gameObject, ai_position + (Time.deltaTime * new Vector3(ai_dir.x, ai_dir.y, 0) * 2f));
                                return;
                            }

                            ai_gridpos -= ai_dir;
                            ai_position = updatePosition(gameObject, ai_gridpos + new Vector3(0.5f, 0.5f, 0));
                            ai_moving = false;
                            ai_state = 1;
                            StopCoroutine(walkAnimation);

                            
                            
                        }
                        else
                        {
                            ai_position = updatePosition(gameObject, ai_position + (Time.deltaTime * new Vector3(ai_dir.x, ai_dir.y, 0) * 2f));
                        }
                        
                    }



                    
                }
                return;
            case 3: //striking
                return;
            case 4: //following
                return;
        }
                
    }

    IEnumerator pathFindTowardsPoint(GameObject obj, Vector3Int target)
    {
        yield return new WaitForEndOfFrame();
    }

    IEnumerator playAnimation(GameObject obj, Vector2 frames, Sprite[] sprites, bool loop) //frames is vect2d with x being start and y being end frames
    {
        bool endAnim = false;
        int curFrame = ai_frame;
        int timer1 = 0;
        //updateSprite(obj, obj.GetComponent<state_chg>().sprites[curFrame], new Vector3());
        while (!endAnim)
        {
            //print(timer1);
            //print(curFrame);
            timer1 += 1;
            if (timer1 > 20)
            {
                timer1 = 0;


                curFrame += 1;
                ai_frame = curFrame>Mathf.RoundToInt(frames.y)?Mathf.RoundToInt(frames.x):curFrame;
                if (curFrame > Mathf.RoundToInt(frames.y))
                {

                    curFrame = Mathf.RoundToInt(frames.x);

                    //Vector3Int objGridPos = foreground.GetComponent<Tilemap>().LocalToCell(obj.GetComponent<Transform>().localPosition);
                    //foreground.GetComponent<Tilemap>().SetTile(objGridPos, null);
                    if (!loop)
                    {
                        endAnim = true;
                    }
                    //endAnim = true;
                }

            }
            updateSprite(obj, sprites[curFrame], new Vector3(0,0,ai_facing + 90));
            yield return new WaitForEndOfFrame();
        }
    }

    void selectRandomDirection()
    {
        List<float> rndRadDirectionList = new List<float> { 0, 0.5f * Mathf.PI, Mathf.PI, 1.5f * Mathf.PI, 2f * Mathf.PI };
        float rndRadDirection;
        int indexChosen;
        bool dirObtained = false;
        while (!dirObtained)
        {
            indexChosen = Random.Range(0, rndRadDirectionList.Count);
            rndRadDirection = rndRadDirectionList[indexChosen];//(Mathf.Round(Random.Range(0, 4)) / 4f) * (2 * Mathf.PI);
            ai_direction = new Vector2(Mathf.Cos(rndRadDirection), -Mathf.Sin(rndRadDirection));
            ai_facing = Direction2Deg(-ai_direction.y, ai_direction.x);
            ai_dir = new Vector3Int(Mathf.RoundToInt(Mathf.Cos(rndRadDirection)), -Mathf.RoundToInt(Mathf.Sin(rndRadDirection)), 0);
            GameObject tmp_spot_obj = foreground.GetComponent<Tilemap>().GetInstantiatedObject(ai_gridpos + ai_dir);
            if (tmp_spot_obj!=null)
            {
                if (tmp_spot_obj.tag == "obstacle")
                {
                    if (spot_obj.GetComponent<state_chg>().crossable){
                        dirObtained = true;
                    }
                    else
                    {
                        rndRadDirectionList.RemoveAt(indexChosen);
                    }
                }
                else if (tmp_spot_obj.tag == "pickup" || tmp_spot_obj.tag == "enemy" || tmp_spot_obj.tag == "Player")
                {
                    dirObtained = true;
                }
                else
                {
                    rndRadDirectionList.RemoveAt(indexChosen);
                }

                
            }
            else
            {
                dirObtained = true;
            }
        }
        
        return;
    }

    IEnumerator followPlayer()
    {

        yield return new WaitForEndOfFrame();
    }
}
