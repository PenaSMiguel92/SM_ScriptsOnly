using UnityEngine;

public class BaseTile : MonoBehaviour {
    protected Vector3 gridOffset = new Vector3(0.5f, 0.5f, 0);
    protected Vector3Int tileLocation;
    protected Vector3Int gridPos;
    protected Vector3 positionTrack;
    protected GameObject grid;
    protected GameObject instObj;
    protected Transform currentTransform;    

    protected GameControl mainControl;
    protected AudioManager audioManager;

    public Vector3Int TileLocation { get { return tileLocation; } }

    public Vector3 GetPosition()
    {
        return currentTransform.localPosition;
    }

    protected void Start()
    {
        mainControl = GameControl.Main;
        audioManager = AudioManager.Main;
        currentTransform = gameObject.GetComponent<Transform>();
    }
}
