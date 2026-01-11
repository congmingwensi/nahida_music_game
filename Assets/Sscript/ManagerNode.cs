using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 音符信息类 - 存储单个音符的所有信息
/// </summary>
public class NoteInfo
{
    public char KeyChar;           // 音符字符（如 'Z', 'A', 'Q' 等）
    public NoteObject NoteObj;     // 音符对象
    public int NoteId;             // 音符唯一ID
    public bool IsDeleted;         // 是否已被删除（标记删除，延迟真正删除）
    public float SpawnTime;        // 生成时间

    public NoteInfo(char keyChar, NoteObject noteObj, int noteId)
    {
        KeyChar = keyChar;
        NoteObj = noteObj;
        NoteId = noteId;
        IsDeleted = false;
        SpawnTime = Time.time;
    }

    /// <summary>
    /// 获取当前 Y 位置（相对于判定线的距离）
    /// </summary>
    public float GetYPos()
    {
        if (NoteObj == null || NoteObj.gameObject == null) return float.MaxValue;
        // 返回相对于判定线 (0.95f) 的距离
        return NoteObj.transform.position.y - 0.95f;
    }
}

/// <summary>
/// 音游核心 - 使用 Dictionary 结构管理音符
/// 参考 Python 的 RhythmGameCore 实现
/// 支持：
/// 1. 快速按键查找（O(1) 访问轨道）
/// 2. 逆序按下（可以先按后面的键再按前面的键）
/// 3. 判定区内选择最接近判定线的音符
/// 4. 白键/黑键分离（同一个 KeyCode 对应不同的 char 轨道）
/// </summary>
public class RhythmGameCore
{
    // 核心数据结构：每个轨道（char 音符字符）对应一个音符列表
    // 使用 char 而不是 KeyCode，因为白键和黑键共用 KeyCode 但字符不同
    // 例如：'Q'(白键) 和 'q'(黑键) 都对应 KeyCode.Q
    private Dictionary<char, List<NoteInfo>> tracks = new Dictionary<char, List<NoteInfo>>();
    
    // 所有音符的列表（用于遍历操作）
    private List<NoteInfo> allNotes = new List<NoteInfo>();
    
    // 音符ID计数器
    private int noteIdCounter = 0;
    
    // 判定范围配置
    // 注意：这个系统没有严格的 MISS，只有"暂停等待 + auto"模式
    public float HitRangeTop = 1.5f;      // 判定区上限（相对于判定线）
    public float PerfectRange = 0.3f;     // Perfect 判定范围
    public float GoodRangeTop = 0.5f;     // Good 判定上限（按早了）
    public float GoodRangeBottom = -0.4f; // Good 判定下限（按晚了）
    
    // 调试开关
    public bool DebugMode = true;

    // 锁对象（线程安全，虽然 Unity 主要是单线程）
    private readonly object lockObj = new object();

    /// <summary>
    /// 添加音符到指定轨道（使用 char 作为轨道 key）
    /// </summary>
    public void AddNote(char keyChar, NoteObject noteObj)
    {
        lock (lockObj)
        {
            if (!tracks.ContainsKey(keyChar))
            {
                tracks[keyChar] = new List<NoteInfo>();
            }

            NoteInfo noteInfo = new NoteInfo(keyChar, noteObj, noteIdCounter++);
            tracks[keyChar].Add(noteInfo);
            allNotes.Add(noteInfo);
            
            // 将 NoteInfo 引用存到 NoteObject 中，方便后续访问
            noteObj.noteInfo = noteInfo;
            
            if (DebugMode)
            {
                Debug.Log($"[RhythmCore] 添加音符: char='{keyChar}', ID={noteInfo.NoteId}, 轨道总数={tracks[keyChar].Count}");
            }
        }
    }
    
    // 保留旧方法签名以兼容
    public void AddNote(KeyCode key, char keyChar, NoteObject noteObj)
    {
        AddNote(keyChar, noteObj);
    }

    /// <summary>
    /// 触发按键判定 - 核心逻辑
    /// 根据 KeyCode 获取所有可能的音符字符（白键+黑键），自动选择最近的
    /// 例如：按下 Q 键 → 同时检查 'Q'(白键) 和 'q'(黑键) 轨道
    /// </summary>
    public TriggerResult Trigger(KeyCode key, bool shiftPressed = false)
    {
        lock (lockObj)
        {
            // 1. 根据 KeyCode 和 Shift 状态获取可能的音符字符
            List<char> possibleChars = GetPossibleChars(key, shiftPressed);
            
            if (DebugMode)
            {
                Debug.Log($"[RhythmCore] Trigger: key={key}, shift={shiftPressed}, 可能的字符=[{string.Join(",", possibleChars)}]");
            }
            
            // 2. 在所有可能的轨道中找候选音符
            List<NoteInfo> candidates = new List<NoteInfo>();
            foreach (char ch in possibleChars)
            {
                if (!tracks.ContainsKey(ch)) continue;
                
                foreach (var note in tracks[ch])
                {
                    if (note.IsDeleted) continue;
                    if (note.NoteObj == null) continue;
                    
                    float yPos = note.GetYPos();
                    // 只考虑已经进入判定区的音符（y位置 <= 判定区上限）
                    if (yPos <= HitRangeTop)
                    {
                        candidates.Add(note);
                        if (DebugMode)
                        {
                            Debug.Log($"[RhythmCore] 候选音符: char='{note.KeyChar}', yPos={yPos:F3}, ID={note.NoteId}");
                        }
                    }
                }
            }

            // 3. 没有候选音符
            if (candidates.Count == 0)
            {
                if (DebugMode)
                {
                    Debug.Log($"[RhythmCore] 没有候选音符，返回 IGNORE");
                    // 打印当前所有轨道状态
                    foreach (var kvp in tracks)
                    {
                        var activeNotes = kvp.Value.Where(n => !n.IsDeleted).ToList();
                        if (activeNotes.Count > 0)
                        {
                            Debug.Log($"[RhythmCore] 轨道 '{kvp.Key}': {activeNotes.Count} 个活跃音符, 最近的 yPos={activeNotes.Min(n => n.GetYPos()):F3}");
                        }
                    }
                }
                return new TriggerResult { Status = HitStatus.IGNORE };
            }

            // 4. 选择最接近判定线的音符（y_pos 绝对值最小的）
            NoteInfo targetNote = candidates.OrderBy(n => Mathf.Abs(n.GetYPos())).First();
            float targetYPos = targetNote.GetYPos();

            if (DebugMode)
            {
                Debug.Log($"[RhythmCore] 选中音符: char='{targetNote.KeyChar}', yPos={targetYPos:F3}, ID={targetNote.NoteId}");
            }

            // 5. 判断判定等级（没有严格的 MISS，只有暂停等待+auto模式）
            TriggerResult result = new TriggerResult();
            result.NoteInfo = targetNote;
            result.Offset = targetYPos;

            // 根据偏移量判定等级
            if (Mathf.Abs(targetYPos) <= PerfectRange)
            {
                result.Status = HitStatus.PERFECT;
            }
            else if (targetYPos > GoodRangeTop)
            {
                result.Status = HitStatus.EARLY;  // 按早了
            }
            else if (targetYPos < GoodRangeBottom)
            {
                result.Status = HitStatus.LATE;   // 按晚了（但不是 MISS，只是扣分少一点）
            }
            else
            {
                result.Status = HitStatus.GOOD;
            }

            // 6. 标记为已删除（实际删除由 GameMenager 处理）
            targetNote.IsDeleted = true;
            
            if (DebugMode)
            {
                Debug.Log($"[RhythmCore] 判定结果: {result.Status}, offset={targetYPos:F3}");
            }

            return result;
        }
    }
    
    /// <summary>
    /// 根据 KeyCode 获取所有可能的音符字符（白键+黑键）
    /// 不再强制要求按 Shift，而是同时返回白键和黑键
    /// 让系统自动选择有音符且最接近判定线的那个
    /// 
    /// 映射规则：
    /// - 字母键：大写=白键，小写=黑键
    /// - 数字键：数字=白键，符号=黑键
    /// </summary>
    private List<char> GetPossibleChars(KeyCode key, bool shiftPressed)
    {
        List<char> result = new List<char>();
        
        switch (key)
        {
            // 第一排：ZzXxCVvBbNnM
            case KeyCode.Z: result.Add('Z'); result.Add('z'); break;
            case KeyCode.X: result.Add('X'); result.Add('x'); break;
            case KeyCode.C: result.Add('C'); break; // C 没有黑键
            case KeyCode.V: result.Add('V'); result.Add('v'); break;
            case KeyCode.B: result.Add('B'); result.Add('b'); break;
            case KeyCode.N: result.Add('N'); result.Add('n'); break;
            case KeyCode.M: result.Add('M'); break; // M 没有黑键
            
            // 第二排：AaSsDFfGgHhJ
            case KeyCode.A: result.Add('A'); result.Add('a'); break;
            case KeyCode.S: result.Add('S'); result.Add('s'); break;
            case KeyCode.D: result.Add('D'); break; // D 没有黑键
            case KeyCode.F: result.Add('F'); result.Add('f'); break;
            case KeyCode.G: result.Add('G'); result.Add('g'); break;
            case KeyCode.H: result.Add('H'); result.Add('h'); break;
            case KeyCode.J: result.Add('J'); break; // J 没有黑键
            
            // 第三排：QqWwERrTtYyU
            case KeyCode.Q: result.Add('Q'); result.Add('q'); break;
            case KeyCode.W: result.Add('W'); result.Add('w'); break;
            case KeyCode.E: result.Add('E'); break; // E 没有黑键
            case KeyCode.R: result.Add('R'); result.Add('r'); break;
            case KeyCode.T: result.Add('T'); result.Add('t'); break;
            case KeyCode.Y: result.Add('Y'); result.Add('y'); break;
            case KeyCode.U: result.Add('U'); break; // U 没有黑键
            
            // 数字键：1!2@34$5%6^7
            case KeyCode.Alpha1: result.Add('1'); result.Add('!'); break;
            case KeyCode.Alpha2: result.Add('2'); result.Add('@'); break;
            case KeyCode.Alpha3: result.Add('3'); break; // 3 没有黑键
            case KeyCode.Alpha4: result.Add('4'); result.Add('$'); break;
            case KeyCode.Alpha5: result.Add('5'); result.Add('%'); break;
            case KeyCode.Alpha6: result.Add('6'); result.Add('^'); break;
            case KeyCode.Alpha7: result.Add('7'); break; // 7 没有黑键
            
            // 高音区：8*9(0IiOoKkL
            case KeyCode.Alpha8: result.Add('8'); result.Add('*'); break;
            case KeyCode.Alpha9: result.Add('9'); result.Add('('); break;
            case KeyCode.Alpha0: result.Add('0'); break; // 0 没有黑键
            case KeyCode.I: result.Add('I'); result.Add('i'); break;
            case KeyCode.O: result.Add('O'); result.Add('o'); break;
            case KeyCode.K: result.Add('K'); result.Add('k'); break;
            case KeyCode.L: result.Add('L'); break; // L 没有黑键
        }
        
        return result;
    }

    /// <summary>
    /// 延迟删除音符（在 Trigger 后调用）
    /// </summary>
    public void RemoveNote(NoteInfo noteInfo)
    {
        lock (lockObj)
        {
            if (noteInfo == null) return;
            
            if (DebugMode)
            {
                Debug.Log($"[RhythmCore] 删除音符: char='{noteInfo.KeyChar}', ID={noteInfo.NoteId}");
            }
            
            // 隐藏游戏对象
            if (noteInfo.NoteObj != null && noteInfo.NoteObj.gameObject != null)
            {
                noteInfo.NoteObj.gameObject.SetActive(false);
            }

            // 从轨道中移除（使用 char 作为 key）
            char keyChar = noteInfo.KeyChar;
            if (tracks.ContainsKey(keyChar))
            {
                tracks[keyChar].Remove(noteInfo);
            }

            // 从全局列表移除
            allNotes.Remove(noteInfo);
        }
    }

    /// <summary>
    /// 根据 char 直接删除音符（用于 Miss 时）
    /// </summary>
    public void RemoveNoteByChar(char keyChar)
    {
        lock (lockObj)
        {
            if (!tracks.ContainsKey(keyChar)) return;

            // 找到最前面的未删除音符
            NoteInfo noteToRemove = tracks[keyChar].FirstOrDefault(n => !n.IsDeleted);
            if (noteToRemove != null)
            {
                if (DebugMode)
                {
                    Debug.Log($"[RhythmCore] RemoveNoteByChar: char='{keyChar}', 删除音符 ID={noteToRemove.NoteId}");
                }
                RemoveNote(noteToRemove);
            }
        }
    }

    /// <summary>
    /// 获取指定轨道最前面的音符（用于自动播放等）
    /// </summary>
    public NoteInfo GetFirstNote(char keyChar)
    {
        lock (lockObj)
        {
            if (!tracks.ContainsKey(keyChar)) return null;
            return tracks[keyChar].FirstOrDefault(n => !n.IsDeleted);
        }
    }

    /// <summary>
    /// 获取所有剩余音符数量
    /// </summary>
    public int Count
    {
        get
        {
            lock (lockObj)
            {
                return allNotes.Count(n => !n.IsDeleted);
            }
        }
    }

    /// <summary>
    /// 设置所有音符的自动模式
    /// </summary>
    public void SetAutoMode(bool auto)
    {
        lock (lockObj)
        {
            foreach (var note in allNotes)
            {
                if (!note.IsDeleted && note.NoteObj != null)
                {
                    note.NoteObj.autoMode = auto;
                }
            }
        }
    }

    /// <summary>
    /// 清除所有音符
    /// </summary>
    public void ClearAll()
    {
        lock (lockObj)
        {
            foreach (var note in allNotes)
            {
                if (note.NoteObj != null && note.NoteObj.gameObject != null)
                {
                    note.NoteObj.gameObject.SetActive(false);
                }
            }
            tracks.Clear();
            allNotes.Clear();
        }
    }

    /// <summary>
    /// 获取所有轨道的迭代器
    /// </summary>
    public IEnumerable<NoteInfo> GetAllNotes()
    {
        lock (lockObj)
        {
            return allNotes.Where(n => !n.IsDeleted).ToList();
        }
    }

    /// <summary>
    /// char 转 KeyCode
    /// </summary>
    public static KeyCode CharToKeyCode(char ch)
    {
        switch (ch)
        {
            case 'Z': case 'z': return KeyCode.Z;
            case 'X': case 'x': return KeyCode.X;
            case 'C': return KeyCode.C;
            case 'V': case 'v': return KeyCode.V;
            case 'B': case 'b': return KeyCode.B;
            case 'N': case 'n': return KeyCode.N;
            case 'M': return KeyCode.M;
            case 'A': case 'a': return KeyCode.A;
            case 'S': case 's': return KeyCode.S;
            case 'D': return KeyCode.D;
            case 'F': case 'f': return KeyCode.F;
            case 'G': case 'g': return KeyCode.G;
            case 'H': case 'h': return KeyCode.H;
            case 'J': return KeyCode.J;
            case 'Q': case 'q': return KeyCode.Q;
            case 'W': case 'w': return KeyCode.W;
            case 'E': return KeyCode.E;
            case 'R': case 'r': return KeyCode.R;
            case 'T': case 't': return KeyCode.T;
            case 'Y': case 'y': return KeyCode.Y;
            case 'U': return KeyCode.U;
            case '1': case '!': return KeyCode.Alpha1;
            case '2': case '@': return KeyCode.Alpha2;
            case '3': return KeyCode.Alpha3;
            case '4': case '$': return KeyCode.Alpha4;
            case '5': case '%': return KeyCode.Alpha5;
            case '6': case '^': return KeyCode.Alpha6;
            case '7': return KeyCode.Alpha7;
            case '8': case '*': return KeyCode.Alpha8;
            case '9': case '(': return KeyCode.Alpha9;
            case '0': return KeyCode.Alpha0;
            case 'I': case 'i': return KeyCode.I;
            case 'O': case 'o': return KeyCode.O;
            case 'K': case 'k': return KeyCode.K;
            case 'L': return KeyCode.L;
            default: return KeyCode.None;
        }
    }
}

/// <summary>
/// 判定状态枚举
/// </summary>
public enum HitStatus
{
    IGNORE,   // 忽略（没有音符可判定）
    PERFECT,  // 完美
    GOOD,     // 良好
    EARLY,    // 按早了
    LATE,     // 按晚了
    MISS      // 错过
}

/// <summary>
/// 触发结果类
/// </summary>
public class TriggerResult
{
    public HitStatus Status;
    public NoteInfo NoteInfo;
    public float Offset;  // 偏移量（正=早，负=晚）
}

// ================== 保留旧的类以保持兼容性 ==================

/// <summary>
/// [已废弃] 旧的队列类 - 保留以兼容旧代码
/// </summary>
public class CharGameObjectQueue : IEnumerable<KeyValuePair<char, NoteObject>>
{
    private Queue<KeyValuePair<char, NoteObject>> note_queue = new Queue<KeyValuePair<char, NoteObject>>();

    public void Enqueue(char key, NoteObject obj)
    {
        note_queue.Enqueue(new KeyValuePair<char, NoteObject>(key, obj));
    }

    public KeyValuePair<char, NoteObject>? Dequeue()
    {
        if (note_queue.Count == 0) return null;
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

    public int Count => note_queue.Count;

    public void NoteAuto(bool autofactor)
    {
        foreach (var track in note_queue)
        {
            NoteObject temp_node = track.Value.GetComponent<NoteObject>();
            temp_node.autoMode = autofactor;
        }
    }

    public IEnumerator<KeyValuePair<char, NoteObject>> GetEnumerator() => note_queue.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public void ClearAll() => note_queue.Clear();
}

/// <summary>
/// 按键信息类
/// </summary>
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

    public override int GetHashCode()
    {
        return Key.GetHashCode() ^ TimePressed.GetHashCode();
    }
}

/// <summary>
/// 全局按键状态
/// </summary>
public static class GlobalKeyPresses
{
    public static HashSet<KeyPressInfo> KeyPresses = new HashSet<KeyPressInfo>();

    public static StringBuilder PrintAllKeysAsOneString()
    {
        StringBuilder allKeys = new StringBuilder();
        foreach (KeyPressInfo keyPress in KeyPresses)
        {
            allKeys.Append(keyPress.Key.ToString());
            allKeys.Append(", ");
        }
        if (allKeys.Length > 0)
        {
            allKeys.Remove(allKeys.Length - 2, 2);
        }
        return allKeys;
    }
}
