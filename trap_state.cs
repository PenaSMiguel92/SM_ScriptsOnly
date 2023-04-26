using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trap_state : MonoBehaviour
{
    public bool trap_enabled;
    public int type;
    public Sprite[] sprites;

    //private GameObjects
    private int trap_enabled_int;
    // Start is called before the first frame update
    //void Start()
    //{
        
    //}

    // Update is called once per frame
    void Update()
    {
        int trap_enabled_int = !trap_enabled ? 1 : 0;
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[trap_enabled_int];

        //if GameObject.FindGameObjectsWithTag("obstacle")
    }
}
