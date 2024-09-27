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
            if (autoMode || gameObject.tag == "Harbor") //�Զ�ģʽ���� ���Ϊ���صİ��������ذ���Ϊ����ǰ���! ��SpawnNote����noteʱ����
            {
                if (timeDifference <= 0.1)//��Ϊ�����ǵ�һ��miss���auto���ı���autoMode֮��ᵽ������Ե�һ��auto��Difference��һ���ܴ�ĸ������ʴ���Ҫ�ж�С��-0.02 ��˵�һ��auto�İ�����ʹ�䰴��
                {
                    if(GameMenager.instance.managerNote.Count != 0 && GameMenager.instance.managerNote.GetTopElement().Key == keyChar)
                        GameMenager.instance.notePlayers[keyChar].PlayNoteSound(keyChar);
                    HitNote(true);
                }
            }
            else if (canBePressed && transform.position.y <= 0.3)
            {
                GameMenager.instance.NoteStill();
                GameMenager.instance.NoteAuto(true);//��һ��missʱ�Զ�auto                
            }
        }
    }

    public float HitNote(bool auto = false)
    {
        isPressed = true; // ���Ϊ�Ѱ���
        if (auto == false && autoMode == true) //��ʾ�������£��ǽű�����
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
