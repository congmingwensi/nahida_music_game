using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CharGameObjectQueue : IEnumerable<KeyValuePair<char, NoteObject>>
{
    private Queue<KeyValuePair<char, NoteObject>> note_queue = new Queue<KeyValuePair<char, NoteObject>>();
    private static Dictionary<char, NoteObject> dictionary = new Dictionary<char, NoteObject>();

    // 入队操作
    public void Enqueue(char key, NoteObject obj)
    {
        // 入队 KeyValuePair
        note_queue.Enqueue(new KeyValuePair<char, NoteObject>(key, obj));
    }

    // 出队操作
    public KeyValuePair<char, NoteObject>? Dequeue()
    {
        //Debug.Log("出队！");
        if (note_queue.Count == 0)
        {
            return null; // 或者根据需要抛出异常
        }

        var item = note_queue.Dequeue();
        item.Value.gameObject.SetActive(false);
        return item;
    }
    public KeyValuePair<char, NoteObject> GetTopElement()
    {
        if (note_queue.Count > 0)
            return note_queue.Peek();
        else
            return new KeyValuePair<char, NoteObject>();
    }

    // 获取当前队列中的元素数量
    public int Count
    {
        get { return note_queue.Count; }
    }

    // 允许外部访问字典（只读）
    public static IReadOnlyDictionary<char, NoteObject> Dictionary
    {
        get { return dictionary; }
    }
    public void NoteAuto(bool autofactor)//全局auto状态调整
    {
        foreach (var track in note_queue)
        {
            NoteObject temp_node = track.Value.GetComponent<NoteObject>();
            temp_node.autoMode = autofactor;
        }
    }
    public void NoteSports(bool sports = true)//全局运动状态调整
    {
        //if (sports)
        //    Debug.Log("运动");
        //else
        //    Debug.Log("静止");
        foreach (var track in note_queue)
        {
            NoteObject temp_node = track.Value.GetComponent<NoteObject>();
            temp_node.nodeSports = sports;
        }
    }
    public IEnumerator<KeyValuePair<char, NoteObject>> GetEnumerator()
    {
        return note_queue.GetEnumerator();
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    public void ClearAll()
    {
        note_queue.Clear();
        dictionary.Clear();
    }

}

public class KeyPressInfo
{
    public KeyCode Key;
    public float TimePressed;

    public KeyPressInfo(KeyCode key, float timePressed)
    {
        Key = key;
        TimePressed = timePressed;
    }
    public override bool Equals(object obj)
    {
        return obj is KeyPressInfo info &&
               Key == info.Key &&
               TimePressed == info.TimePressed;
    }

    // 重写GetHashCode方法
    public override int GetHashCode()
    {
        return HashCode.Combine(Key, TimePressed);
    }
}
public static class GlobalKeyPresses
{
    public static HashSet<KeyPressInfo> KeyPresses = new HashSet<KeyPressInfo>();
    public static StringBuilder PrintAllKeysAsOneString()
    {
        StringBuilder allKeys = new StringBuilder();

        foreach (KeyPressInfo keyPress in KeyPresses)
        {
            allKeys.Append(keyPress.Key.ToString());
            allKeys.Append(", "); // 为了美观，每个KeyCode之间加入逗号和空格
        }

        // 移除最后一个逗号和空格（如果存在）
        if (allKeys.Length > 0)
        {
            allKeys.Remove(allKeys.Length - 2, 2);
        }

        return allKeys;
    }
}

