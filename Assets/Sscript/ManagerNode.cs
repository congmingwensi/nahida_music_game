using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CharGameObjectQueue
{
    private Queue<KeyValuePair<KeyCode, NoteObject>> note_queue = new Queue<KeyValuePair<KeyCode, NoteObject>>();
    private static Dictionary<KeyCode, NoteObject> dictionary = new Dictionary<KeyCode, NoteObject>();

    // ��Ӳ���
    public void Enqueue(KeyCode key, NoteObject obj)
    {
        // ��� KeyValuePair
        note_queue.Enqueue(new KeyValuePair<KeyCode, NoteObject>(key, obj));
    }

    // ���Ӳ���
    public KeyValuePair<KeyCode, NoteObject>? Dequeue()
    {
        //Debug.Log("���ӣ�");
        if (note_queue.Count == 0)
        {
            return null; // ���߸�����Ҫ�׳��쳣
        }

        var item = note_queue.Dequeue();
        item.Value.gameObject.SetActive(false);
        return item;
    }
    public KeyValuePair<KeyCode, NoteObject> GetTopElement()
    {
        if (note_queue.Count > 0)
            return note_queue.Peek();
        else
            return new KeyValuePair<KeyCode, NoteObject>();
    }

    // ��ȡ��ǰ�����е�Ԫ������
    public int Count
    {
        get { return note_queue.Count; }
    }

    // �����ⲿ�����ֵ䣨ֻ����
    public static IReadOnlyDictionary<KeyCode, NoteObject> Dictionary
    {
        get { return dictionary; }
    }
    public void NoteAuto(bool autofactor)//ȫ��auto״̬����
    {
        foreach (var track in note_queue)
        {
            NoteObject temp_node = track.Value.GetComponent<NoteObject>();
            temp_node.autoMode = autofactor;
        }
    }
    public void NoteSports(bool sports = true)//ȫ���˶�״̬����
    {
        if (sports)
            Debug.Log("�˶�");
        else
            Debug.Log("��ֹ");
        foreach (var track in note_queue)
        {
            NoteObject temp_node = track.Value.GetComponent<NoteObject>();
            temp_node.nodeSports = sports;
        }
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

    // ��дGetHashCode����
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
            allKeys.Append(", "); // Ϊ�����ۣ�ÿ��KeyCode֮����붺�źͿո�
        }

        // �Ƴ����һ�����źͿո�������ڣ�
        if (allKeys.Length > 0)
        {
            allKeys.Remove(allKeys.Length - 2, 2);
        }

        return allKeys;
    }
}
