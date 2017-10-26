using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowMoveAnimation : MonoBehaviour {

    private Transform moveTom;
    public Transform MoveTom {
        get {
            if (moveTom == null) {
                Object obj = Resources.Load<Object>(string.Format("UIFlag/{0}" , flagStr));
                moveTom = (GameObject.Instantiate(obj) as GameObject).transform;
                moveTom.rotation = Quaternion.Euler(90, 0, 0);
            }
            return moveTom;
        }
    }

    public Flowroad fr;
    public bool isLoop;
    public int startIndex;
    public string flagStr;
    public int addNum=1;

    private bool isPlaying;
    private int currentNum;
    private int frameTick = 2;
    private int currentFrame;
    private bool isInverse;

    // Use this for initialization
    void Start()
    {
        isPlaying = true;
        if (addNum == 0) addNum = 1;
        currentNum = startIndex;
    }

    void OnDestroy() {
        if (MoveTom != null)
            DestroyImmediate(MoveTom.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            if (fr == null)
            {
                Debug.LogError("fr is null");
                isPlaying = false;
                return;
            }
            currentFrame++;
            if (currentFrame < frameTick) return;
            currentFrame = 0;

            #region move
            if (!isInverse)
            {
                if (currentNum < fr.GetTotalNum()-1)
                {
                    //Debug.Log("currentNum:"+ currentNum+ " GetTotalNum:" + fr.GetTotalNum());
                    MoveTom.position = fr.GetWorldPos(currentNum);
                    currentNum += addNum;
                }
                else
                {
                    if (isLoop)
                    {
                        isInverse = true;
                        currentNum = fr.GetTotalNum() - 1;
                    }
                    else isPlaying = false;
                }
            }
            else {
                if (currentNum > 0)
                {
                    //Debug.Log("currentNum:" + currentNum );
                    MoveTom.position = fr.GetWorldPos(currentNum);
                    currentNum -= addNum;
                }
                else {
                    if (isLoop)
                    {
                        currentNum = 0;
                        isInverse = false;
                    }
                    else isPlaying = false;
                }
            }
            
            #endregion
        }
    }
}
