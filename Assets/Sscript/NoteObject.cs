using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using static UnityEngine.GraphicsBuffer;
using YamlDotNet.Core.Tokens;

public class NoteObject : MonoBehaviour
{
    public float bearTempo;
    //private float downSumTtime;

    public bool canBePressed;
    private bool isPressed;
    public char keyChar;
    public KeyCode keyToPress;
    public float initialTime;

    public bool nodeSports;
    public bool autoMode = false;


    // Start is called before the first frame update
    void Start()
    {
        nodeSports = true;
        canBePressed = false;
        isPressed = false;
        //downSumTtime = 11.5f / bearTempo;
    }

    void Update()
    {
        //float timeDifference = downSumTtime - (Time.time - initialTime);
        float timeDifference = transform.position.y - 0.95f;
        //UnityEngine.Debug.Log($"this.gameObject.name:{this.gameObject.name},nodeSports:{nodeSports}");
        if (nodeSports)
        {
            transform.position -= new Vector3(0f, bearTempo * Time.deltaTime, 0f);
            if (autoMode || gameObject.tag == "Harbor") //自动模式，或 标记为隐藏的按键。隐藏按键为按键前面带! 由SpawnNote生成note时设置
            {
                if (timeDifference <= 0.1)//因为下面是第一次miss后的auto，改变了autoMode之后会到这里。所以第一次auto的Difference是一个很大的负数。故此需要判断小于-0.02 兼顾第一次auto的按键，使其按下
                {
                    if(GameMenager.instance.managerNote.Count != 0 && GameMenager.instance.managerNote.GetTopElement().Key == keyChar)
                        GameMenager.instance.notePlayers[keyChar].PlayNoteSound(keyChar);
                    HitNote(true);
                }
            }
            else if (canBePressed && transform.position.y <= 0.3)
            {
                GameMenager.instance.NoteStill();
                GameMenager.instance.NoteAuto(true);//第一次miss时自动auto                
            }
        }
    }

    public float HitNote(bool auto = false)
    {
        isPressed = true; // 标记为已按下
        if (auto == false && autoMode == true) //表示正常按下，非脚本控制
        {
            GameMenager.instance.NoteAuto(false);
        }
        GameMenager.instance.RemoveNoteFromTrack(keyChar);
        //Debug.Log($"HitNote Disappeared:{this.GetHashCode()}");
        return transform.position.y - 0.90f;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameObject.tag != "Harbor" && other.tag == "Activator")
        {
            canBePressed = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Activator" && !isPressed)
        {
            UnityEngine.Debug.Log($"{this.gameObject.name} note out of range");
            canBePressed = false;
            GameMenager.instance.NoteMissed();
            GameMenager.instance.RemoveNoteFromTrack(keyChar);
        }
    }
    
}
