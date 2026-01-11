using System.Collections;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public float bearTempo;

    public bool canBePressed;
    private bool isPressed;
    public char keyChar;
    public KeyCode keyToPress;
    public float initialTime;
    public bool autoMode = false;
    
    // 新增：关联的 NoteInfo 引用（用于新的判定系统）
    [HideInInspector]
    public NoteInfo noteInfo;

    void Start()
    {
        canBePressed = false;
        isPressed = false;
    }

    void Update()
    {
        float timeDifference = transform.position.y - 0.95f;
        
        // 每帧检查全局状态，而不是依赖实例变量
        // 这样新生成的音符也能立即响应暂停
        bool shouldMove = !GameMenager.instance.nodeStill;
        
        // 检查全局 auto 状态
        bool isAutoMode = autoMode || GameMenager.instance.nodeAuto || gameObject.tag == "Harbor";
        
        if (shouldMove)
        {
            transform.position -= new Vector3(0f, bearTempo * Time.deltaTime, 0f);
            
            // 自动模式或隐藏按键
            if (isAutoMode)
            {
                if (timeDifference <= 0.1 && !isPressed)
                {
                    // 检查是否已被标记删除
                    if (noteInfo == null || !noteInfo.IsDeleted)
                    {
                        GameMenager.instance.notePlayers[keyChar].PlayNoteSound(keyChar);
                        HitNote(true);
                    }
                }
            }
            else if (canBePressed && transform.position.y <= 0.5 && !isPressed && (noteInfo == null || !noteInfo.IsDeleted))
            {
                UnityEngine.Debug.Log($"[NoteObject] 音符 '{keyChar}' (ID={noteInfo?.NoteId}) 触发暂停, y={transform.position.y:F3}");
                GameMenager.instance.TriggerStill();
            }
        }
    }

    public float HitNote(bool auto = false)
    {
        if (isPressed) return 0f; // 防止重复处理
        
        isPressed = true;
        
        if (auto == false && autoMode == true)
        {
            GameMenager.instance.NoteAuto(false);
        }
        
        // 标记 RhythmCore 中的音符为已删除（如果还没标记的话）
        if (noteInfo != null && !noteInfo.IsDeleted)
        {
            noteInfo.IsDeleted = true;
        }
        
        // 延迟删除音符（隐藏 GameObject）
        StartCoroutine(DelayedHide());
        
        return transform.position.y - 0.90f;
    }
    
    private IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
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
            // 音符离开判定区但未被按下，保持可按状态等待暂停机制处理
            canBePressed = true;
        }
    }
}
