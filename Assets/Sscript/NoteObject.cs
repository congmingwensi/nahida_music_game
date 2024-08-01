using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using static UnityEngine.GraphicsBuffer;
using YamlDotNet.Core.Tokens;

public class NoteObject : MonoBehaviour, IPointerClickHandler
{
    public float bearTempo;
    //private float downSumTtime;

    public bool canBePressed;
    private bool isPressed;
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
                    AutoPlay();
                    HitNote(true);
                }
            }
            else if (canBePressed && transform.position.y <= 0.3)
            {
                GameMenager.insrance.NoteStill();
                GameMenager.insrance.NoteAuto(true);//第一次miss时自动auto                
            }
        }
    }

    public float HitNote(bool auto = false)
    {
        isPressed = true; // 标记为已按下
        if (auto == false && autoMode == true) //表示正常按下，非脚本控制
        {
            GameMenager.insrance.NoteAuto(false);
        }
        GameMenager.insrance.RemoveNoteFromTrack(keyToPress);
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
            GameMenager.insrance.NoteMissed();
            GameMenager.insrance.RemoveNoteFromTrack(keyToPress);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isPressed)
            isPressed = true;
        GameMenager.insrance.CheckForKeyPress(keyToPress, true);
        GameMenager.insrance.PlayNote(keyToPress);
    }
    public void AutoPlay()//auto演奏用到。直接调用OnPointerClick的话，参数很复杂没法构造，再定义一个不需要构造参数的
    {
        GameMenager.insrance.PlayNote(keyToPress);
    }
}
