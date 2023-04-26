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

public class state_chg : MonoBehaviour
{
    public bool state; //whether or not disabled with block, false - 
    public bool crossable;
    public bool crossing;
    private bool endAnim;
    public Vector3Int pushableTileLoc;
    public GameObject pushableObject;
    public int type; // 0 - hole, 1 - electric switch
    public Sprite[] sprites; //sprites to use.
    private AudioClip soundUse;
    private Tilemap mainTileMap;
    private Vector3Int TileLoc;
    //private string nameofobject;
    
    //private 

    //private 
    // Start is called before the first frame update
    void Start()
    {
        state = false;
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[0];
        soundUse = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>().audioClips[4];
        mainTileMap = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>().foreground.GetComponent<Tilemap>();
        TileLoc = mainTileMap.LocalToCell(gameObject.GetComponent<Transform>().localPosition);
    }

    // Update is called once per frame
    void Update()
    {
        if (!state && type < 2 && gameObject != null)
        {
            int crossingTst = crossing ? 1 : 0;
            gameObject.GetComponent<SpriteRenderer>().sprite = sprites[crossingTst];
        }
        if (!state && type == 6 && gameObject != null)
        {
            int coin_sum = 0;
            GameObject[] coins = GameObject.FindGameObjectsWithTag("pickup");
            foreach (GameObject coin in coins)
            {
                if (coin.GetComponent<pickup_type>().typeofPickup < 3)
                {
                    coin_sum += 1;
                }
            }
            if (coin_sum < 1)
            {
                state = true;
                AudioSource.PlayClipAtPoint(soundUse, gameObject.GetComponent<Transform>().localPosition);
                StartCoroutine(playAnimation(gameObject, new Vector2(0, 20), sprites, false));
            }
        }
        if (state && type == 6 && endAnim)
        {
            mainTileMap.SetTile(TileLoc, null);
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
        endAnim = false;
        int curFrame = 0;
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
                curFrame = curFrame > Mathf.RoundToInt(frames.y)+1 ? Mathf.RoundToInt(frames.x) : curFrame;
                if (curFrame >= Mathf.RoundToInt(frames.y))
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
            updateSprite(obj, sprites[curFrame], new Vector3(0, 0, 0)); //static class, so no rotation necessary.
            yield return new WaitForEndOfFrame();
        }
        
        //Destroy(gameObject); //destroy once animation ends.
    }
    public TileandObject getTileandObjectBack()
    {
        return new TileandObject(pushableTileLoc, pushableObject);
    }

    public void storeTileandObject(TileandObject _tileandobject)
    {
        pushableTileLoc = _tileandobject.pushableTileLoc;
        pushableObject = _tileandobject.pushableObject;
    }

    //    if (state)
    //    {
    //        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[sprites.Length - 1];
    //    }
    //    else
    //    {
    //        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[0];
    //    }
    //    //else if (!state)
    //    //{
    //    //    if (crossing)
    //    //            int crossingTst = crossing ? 1 : 0;
    //    //            gameObject.GetComponent<SpriteRenderer>().sprite = sprites[crossingTst];
    //    //            return;
    //    //    }
    //    //}
    //}
}
