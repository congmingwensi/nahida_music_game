using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameMenager : MonoBehaviour
{
    public static GameMenager instance;
    public AudioSource audioSource; //读取文件后，用于存储音频
    public string sheetMusic;//谱面
    private bool startPlaying;//谱面音乐
    public string[] WheezeList;//娇喘文件列表
    
    // 谱面显示相关 - 用于在屏幕上滚动显示当前演奏的谱面
    public static string OriginalSheetContent = "";  // 原始谱面内容（简化之前）
    public static int CurrentSheetIndex = 0;         // 当前演奏位置（在原始谱面中的索引）
    public static bool IsPlaying = false;            // 是否正在演奏
    
    private int CurrentScore;
    public int scorePrefict = 100;
    public int scoreGood = 75;
    public Text scoreText;
    public Text comboText;

    private int currentMultiplier;
    private int combo;
    public int[] multiplierThresholds;

    // 新的音游核心（使用 Dict 结构，支持逆序按下）
    public RhythmGameCore rhythmCore;
    
    // 旧的队列（保留兼容）
    public CharGameObjectQueue managerNote;
    private Dictionary<KeyCode, ButtonController> buttonMap;
    [SerializeField]
    private GameObject NoteZ;
    [SerializeField]
    private GameObject NoteX;
    [SerializeField]
    private GameObject NoteC;
    [SerializeField]
    private GameObject NoteV;
    [SerializeField]
    private GameObject NoteB;
    [SerializeField]
    private GameObject NoteN;
    [SerializeField]
    private GameObject NoteM;
    [SerializeField]
    private GameObject NoteA;
    [SerializeField]
    private GameObject NoteS;
    [SerializeField]
    private GameObject NoteD;
    [SerializeField]
    private GameObject NoteF;
    [SerializeField]
    private GameObject NoteG;
    [SerializeField]
    private GameObject NoteH;
    [SerializeField]
    private GameObject NoteJ;
    [SerializeField]
    private GameObject NoteQ;
    [SerializeField]
    private GameObject NoteW;
    [SerializeField]
    private GameObject NoteE;
    [SerializeField]
    private GameObject NoteR;
    [SerializeField]
    private GameObject NoteT;
    [SerializeField]
    private GameObject NoteY;
    [SerializeField]
    private GameObject NoteU;
    [SerializeField]
    private GameObject Note1;
    [SerializeField]
    private GameObject Note2;
    [SerializeField]
    private GameObject Note3;
    [SerializeField]
    private GameObject Note4;
    [SerializeField]
    private GameObject Note5;
    [SerializeField]
    private GameObject Note6;
    [SerializeField]
    private GameObject Note7;
    [SerializeField]
    private GameObject Note8;
    [SerializeField]
    private GameObject Note9;
    [SerializeField]
    private GameObject Note0;
    [SerializeField]
    private GameObject NoteI;
    [SerializeField]
    private GameObject NoteO;
    [SerializeField]
    private GameObject NoteK;
    [SerializeField]
    private GameObject NoteL;

    private Dictionary<char, GameObject> noteDictionary;
    private Dictionary<char, Vector3> noteLocation;
    private int currentOrder = Int32.MaxValue;//控制node显示优先级，从int最大值递减
    private Dictionary<char, float> noteTime;//谱面文件的时间间隔符
    public Dictionary<char, string> noteAudio;
    public bool nodeStill;  // 全局暂停状态（音符在 Update 中检查）
    public bool nodeAuto;   // 全局 auto 状态（音符在 Update 中检查）
    CharacterControl character;
    
    public Dictionary<char, KeyNotePlayer> notePlayers = new Dictionary<char, KeyNotePlayer>();  // 每个琴键一个播放器
    public AudioSource audioSourcePrefab;
    private HashSet<KeyCode> processedKeys = new HashSet<KeyCode>();
    private Dictionary<KeyCode, bool> previousKeyStates = new Dictionary<KeyCode, bool>();
    
    // 所有需要检测的琴键
    private static readonly KeyCode[] allKeys = {
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M,
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J,
        KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U,
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7,
        KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.I, KeyCode.O, KeyCode.K, KeyCode.L
    };
    
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        noteAudio = new Dictionary<char, string>
         {
            {'Z',"C1"},
            {'z',"C_1"},
            {'X',"D1"},
            {'x',"D_1"},
            {'C',"E1"},
            {'V',"F1"},
            {'v',"F_1"},
            {'B',"G1"},
            {'b',"G_1"},
            {'N',"A1"},
            {'n',"A_1"},
            {'M',"B1"},

            {'A',"C2"},
            {'a',"C_2"},
            {'S',"D2"},
            {'s',"D_2"},
            {'D',"E2"},
            {'F',"F2"},
            {'f',"F_2"},
            {'G',"G2"},
            {'g',"G_2"},
            {'H',"A2"},
            {'h',"A_2"},
            {'J',"B2"},

            {'Q',"C3"},
            {'q',"C_3"},
            {'W',"D3"},
            {'w',"D_3"},
            {'E',"E3"},
            {'R',"F3"},
            {'r',"F_3"},
            {'T',"G3"},
            {'t',"G_3"},
            {'Y',"A3"},
            {'y',"A_3"},
            {'U',"B3"},

            {'1',"C4"},
            {'!',"C_4"},
            {'2',"D4"},
            {'@',"D_4"},
            {'3',"E4"},
            {'4',"F4"},
            {'$',"F_4"},
            {'5',"G4"},
            {'%',"G_4"},
            {'6',"A4"},
            {'^',"A_4"},
            {'7',"B4"},

            {'8',"C5"},
            {'*',"C_5"},
            {'9',"D5"},
            {'(',"D_5"},
            {'0',"E5"},
            {'I',"F5"},
            {'i',"F_5"},
            {'O',"G5"},
            {'o',"G_5"},
            {'K',"A5"},
            {'k',"A_5"},
            {'L',"B5"},
         };
        nodeStill = false;
        CurrentScore = 0;
        currentMultiplier = 1;
        scoreText.text = "Score:0";
        
        // 初始化新的音游核心
        rhythmCore = new RhythmGameCore();
        
        // 保留旧的队列（兼容）
        managerNote = new CharGameObjectQueue();
        buttonMap = new Dictionary<KeyCode, ButtonController>(); //key:button 用key控制对应的button做出一些行为
        ButtonController[] buttons = FindObjectsOfType<ButtonController>();
        foreach (ButtonController button in buttons)
        {
            buttonMap[button.keyToPress] = button;
        }
        foreach (char note in "ZzXxCVvBbNnMAaSsDFfGgHhJQqWwERrTtYyU1!2@34$5%6^78*9(0IiOoKkL")  // 针对每个音符
        {
            GameObject audioObject = new GameObject("NotePlayer_" + note);
            audioObject.transform.SetParent(transform);
            KeyNotePlayer player = new KeyNotePlayer(audioObject);
            notePlayers[note] = player;
        }
        noteDictionary = new Dictionary<char, GameObject>//按键对应的预制件。读取谱面文件后，生成对应预制件
        {
            {'Z', NoteZ},
            {'z', NoteZ},
            {'X', NoteX},
            {'x', NoteX},
            {'C', NoteC},
            {'V', NoteV},
            {'v', NoteV},
            {'B', NoteB},
            {'b', NoteB},
            {'N', NoteN},
            {'n', NoteN},
            {'M', NoteM},
            {'A', NoteA},
            {'a', NoteA},
            {'S', NoteS},
            {'s', NoteS},
            {'D', NoteD},
            {'F', NoteF},
            {'f', NoteF},
            {'G', NoteG},
            {'g', NoteG},
            {'H', NoteH},
            {'h', NoteH},
            {'J', NoteJ},
            {'Q', NoteQ},
            {'q', NoteQ},
            {'W', NoteW},
            {'w', NoteW},
            {'E', NoteE},
            {'R', NoteR},
            {'r', NoteR},
            {'T', NoteT},
            {'t', NoteT},
            {'Y', NoteY},
            {'y', NoteY},
            {'U', NoteU},
            {'1', Note1},
            {'!', Note1},
            {'2', Note2},
            {'@', Note2},
            {'3', Note3},
            {'4', Note4},
            {'$', Note4},
            {'5', Note5},
            {'%', Note5},
            {'6', Note6},
            {'^', Note6},
            {'7', Note7},
            {'8', Note8},
            {'*', Note8},
            {'9', Note9},
            {'(', Note9},
            {'0', Note0},
            {'I', NoteI},
            {'i', NoteI},
            {'O', NoteO},
            {'o', NoteO},
            {'K', NoteK},
            {'k', NoteK},
            {'L', NoteL}
        };
        noteLocation = new Dictionary<char, Vector3> //不同音符生成的初始位置，主要是x轴不同
        {
            { 'Z',new Vector3(-9.65f, 11.5f, 0f)},
            { 'z',new Vector3(-9.65f, 11.5f, 0f)},
            { 'X',new Vector3(-9.14f, 11.5f, 0f)},
            { 'x',new Vector3(-9.14f, 11.5f, 0f)},
            { 'C',new Vector3(-8.59f, 11.5f, 0f)},
            { 'V',new Vector3(-8.02f, 11.5f, 0f)},
            { 'v',new Vector3(-8.02f, 11.5f, 0f)},
            { 'B',new Vector3(-7.44f, 11.5f, 0f)},
            { 'b',new Vector3(-7.44f, 11.5f, 0f)},
            { 'N',new Vector3(-6.89f, 11.5f, 0f)},
            { 'n',new Vector3(-6.89f, 11.5f, 0f)},
            { 'M',new Vector3(-6.26f, 11.5f, 0f)},

            { 'A',new Vector3(-5.60f, 11.5f, 0f)},
            { 'a',new Vector3(-5.60f, 11.5f, 0f)},
            { 'S',new Vector3(-5.13f, 11.5f, 0f)},
            { 's',new Vector3(-5.13f, 11.5f, 0f)},
            { 'D',new Vector3(-4.58f, 11.5f, 0f)},
            { 'F',new Vector3(-4.00f, 11.5f, 0f)},
            { 'f',new Vector3(-4.00f, 11.5f, 0f)},
            { 'G',new Vector3(-3.42f, 11.5f, 0f)},
            { 'g',new Vector3(-3.42f, 11.5f, 0f)},
            { 'H',new Vector3(-2.87f, 11.5f, 0f)},
            { 'h',new Vector3(-2.87f, 11.5f, 0f)},
            { 'J',new Vector3(-2.30f, 11.5f, 0f)},

            { 'Q',new Vector3(-1.73f, 11.5f, 0f)},
            { 'q',new Vector3(-1.73f, 11.5f, 0f)},
            { 'W',new Vector3(-1.15f, 11.5f, 0f)},
            { 'w',new Vector3(-1.15f, 11.5f, 0f)},
            { 'E',new Vector3(-0.60f, 11.5f, 0f)},
            { 'R',new Vector3(-0.02f, 11.5f, 0f)},
            { 'r',new Vector3(-0.02f, 11.5f, 0f)},
            { 'T',new Vector3(0.55f, 11.5f, 0f)},
            { 't',new Vector3(0.55f, 11.5f, 0f)},
            { 'Y',new Vector3(1.10f, 11.5f, 0f)},
            { 'y',new Vector3(1.10f, 11.5f, 0f)},
            { 'U',new Vector3(1.68f, 11.5f, 0f)},

            { '1',new Vector3(2.26f, 11.5f, 0f)},
            { '!',new Vector3(2.26f, 11.5f, 0f)},
            { '2',new Vector3(2.84f, 11.5f, 0f)},
            { '@',new Vector3(2.84f, 11.5f, 0f)},
            { '3',new Vector3(3.39f, 11.5f, 0f)},
            { '4',new Vector3(3.97f, 11.5f, 0f)},
            { '$',new Vector3(3.97f, 11.5f, 0f)},
            { '5',new Vector3(4.54f, 11.5f, 0f)},
            { '%',new Vector3(4.54f, 11.5f, 0f)},
            { '6',new Vector3(5.09f, 11.5f, 0f)},
            { '^',new Vector3(5.09f, 11.5f, 0f)},
            { '7',new Vector3(5.67f, 11.5f, 0f)},

            { '8',new Vector3(6.26f, 11.5f, 0f)},
            { '*',new Vector3(6.26f, 11.5f, 0f)},
            { '9',new Vector3(6.84f, 11.5f, 0f)},
            { '(',new Vector3(6.84f, 11.5f, 0f)},
            { '0',new Vector3(7.39f, 11.5f, 0f)},
            { 'I',new Vector3(7.97f, 11.5f, 0f)},
            { 'i',new Vector3(7.97f, 11.5f, 0f)},
            { 'O',new Vector3(8.55f, 11.5f, 0f)},
            { 'o',new Vector3(8.55f, 11.5f, 0f)},
            { 'K',new Vector3(9.10f, 11.5f, 0f)},
            { 'k',new Vector3(9.10f, 11.5f, 0f)},
            { 'L',new Vector3(9.68f, 11.5f, 0f)},
        };
        noteTime = new Dictionary<char, float>//谱面文件的时间间隔符
        {
            { '.', 0.5f},
            { '=', 1 },
            { '-', 2 },
            { '+', 4 },
        };
       
        
        sheetMusic = Path.Combine(Application.streamingAssetsPath, "SheetMusic");//读取谱面文件
        string FileSheet = sheetMusic + "/test_sheet_music.txt";
        string FileConfig = sheetMusic + "/test_sheet_music.yaml";
        StartCoroutine(LoadAndPlay(FileSheet, FileConfig));
        WheezeList = GetWheezeList();
        if (character == null)
        {
            character = FindObjectOfType<CharacterControl>();
        }
}
// Update is called once per frame
void Update()
    {
        // 每帧检查暂停超时
        CheckStillTimeout();
        
        var expiredKeys = GlobalKeyPresses.KeyPresses.Where(keyPress => Time.time - keyPress.TimePressed > 0.3f).ToList();
        foreach (var expiredKey in expiredKeys)
        {
            GlobalKeyPresses.KeyPresses.Remove(expiredKey);
            if (processedKeys.Contains(expiredKey.Key))
            {
                processedKeys.Remove(expiredKey.Key);
                //Debug.Log($"按键 {expiredKey.Key} 超时，从 processedKeys 中移除");
            }
        }
        // 检测所有琴键按下
        foreach (KeyCode key in allKeys)
            CheckForKeyPress(key);

        if (!startPlaying)//检测音频未播放，播放音频
        {
            startPlaying = true;
            string[] sheet_music_path = GetAudioFiles(sheetMusic);
            if (sheet_music_path.Length > 0)
            {
                StartCoroutine(PlayBgm(sheet_music_path[0])); // 播放第一个音频文件
            }
        }
    }

    public void PlayNote(char key='\0') //移动端播放按键音
    {
        if (WheezeList.Length!=0)
        {
            string selectedAudioFile = WheezeList[UnityEngine.Random.Range(0, WheezeList.Length)];
            PlayAudio(selectedAudioFile);
        }
    }
    char keyCodeToChar(KeyCode keycode){
        switch (keycode)
        {
            case KeyCode.Z: return 'Z';
            case KeyCode.X: return 'X';
            case KeyCode.C: return 'C';
            case KeyCode.V: return 'V';
            case KeyCode.B: return 'B';
            case KeyCode.N: return 'N';
            case KeyCode.M: return 'M';

            case KeyCode.A : return 'A';
            case KeyCode.S : return 'S';
            case KeyCode.D : return 'D';
            case KeyCode.F : return 'F';
            case KeyCode.G : return 'G';
            case KeyCode.H : return 'H';
            case KeyCode.J : return 'J';

            case KeyCode.Q: return 'Q';
            case KeyCode.W: return 'W';
            case KeyCode.E: return 'E';
            case KeyCode.R: return 'R';
            case KeyCode.T: return 'T';
            case KeyCode.Y: return 'Y';
            case KeyCode.U: return 'U';

            case KeyCode.Alpha1: return '1';
            case KeyCode.Alpha2: return '2';
            case KeyCode.Alpha3: return '3';
            case KeyCode.Alpha4: return '4';
            case KeyCode.Alpha5: return '5';
            case KeyCode.Alpha6: return '6';
            case KeyCode.Alpha7: return '7';

            case KeyCode.Alpha8: return '8';
            case KeyCode.Alpha9: return '9';
            case KeyCode.Alpha0: return '0';
            case KeyCode.I: return 'I';
            case KeyCode.O: return 'O';
            case KeyCode.K: return 'K';
            case KeyCode.L: return 'L';

            default: return '\0'; // 其他情况
        }
    }
    /// <summary>
    /// 新的按键检测方法 - 使用 RhythmGameCore 的 Dict 结构
    /// 支持逆序按下：可以先按后面的键再按前面的键
    /// </summary>
    public void CheckForKeyPress(KeyCode key, bool key_down = false)
    {
        // 处理判定结果的显示
        void HandleTriggerResult(TriggerResult result)
        {
            if (result.Status == HitStatus.IGNORE) return;
            
            if (buttonMap.TryGetValue(key, out ButtonController buttonController))
            {
                switch (result.Status)
                {
                    case HitStatus.PERFECT:
                        buttonController.ChangePpercevtImage();
                        CurrentScore += scorePrefict;
                        NoteHit();
                        break;
                    case HitStatus.GOOD:
                        buttonController.ChangePpercevtImage();
                        CurrentScore += scoreGood;
                        NoteHit();
                        break;
                    case HitStatus.EARLY:
                        buttonController.ChangePQuickImage();
                        CurrentScore += scoreGood;
                        NoteHit();
                        break;
                    case HitStatus.LATE:
                        buttonController.ChangePSlowImage();
                        CurrentScore += scoreGood;
                        NoteHit();
                        break;
                    case HitStatus.MISS:
                        NoteMissed();
                        break;
                }
            }
            
            // 调用 NoteObject.HitNote() 同步状态并处理删除
            if (result.NoteInfo != null && result.NoteInfo.NoteObj != null)
            {
                result.NoteInfo.NoteObj.HitNote(false);
            }
        }

        // 检测按键按下
        bool keyIsPressed = Input.GetKey(key);
        if (!previousKeyStates.ContainsKey(key))
        {
            previousKeyStates[key] = false;
        }
        bool wasKeyDown = previousKeyStates[key];
        
        // 检测按键按下（上升沿）
        if ((keyIsPressed && !wasKeyDown) || key_down)
        {
            if (!processedKeys.Contains(key))
            {
                processedKeys.Add(key);
                
                // 任意按键都退出auto模式（用户开始演奏）
                NoteAuto(false);
                
                // 使用新的判定系统 - 同时检查白键和黑键轨道，自动选择最近的音符
                TriggerResult result = rhythmCore.Trigger(key);
                
                if (result.Status != HitStatus.IGNORE && result.Status != HitStatus.MISS)
                {
                    // 命中音符时恢复运动
                    ResumeNoteMovement();
                    
                    // 播放按键音
                    if (result.NoteInfo != null)
                    {
                        notePlayers[result.NoteInfo.KeyChar].PlayNoteSound(result.NoteInfo.KeyChar);
                    }
                    HandleTriggerResult(result);
                }
                else
                {
                    // 没有命中音符，播放按键音（不恢复运动，保持暂停等待正确按键）
                    char keyChar = keyCodeToChar(key);
                    if (notePlayers.ContainsKey(keyChar))
                    {
                        notePlayers[keyChar].PlayNoteSound(keyChar);
                    }
                }
            }
        }
        
        // 检测按键释放
        if (!keyIsPressed && wasKeyDown && processedKeys.Contains(key))
        {
            processedKeys.Remove(key);
        }
        
        previousKeyStates[key] = keyIsPressed;
    }
    
    public void NoteHit() //显示分数和combo
    {
        combo++;
        if (combo > 100 && combo < 300)
            character.ChangeImage(2);
        else if (combo > 300 && combo < 500)
            character.ChangeImage(3);
        else if (combo > 500 && combo < 1000)
            character.ChangeImage(4);
        else if (combo > 1000)
            character.ChangeImage(5);

        comboText.text = "Combo:" + combo;
        if (multiplierThresholds[currentMultiplier - 1] < combo && currentMultiplier != 3)
        {
            currentMultiplier++;
        }
        scoreText.text = "Score:" + CurrentScore;
    }
    public void NoteMissed()//miss时改变combo和combo加成
    {
        combo = 0;
        currentMultiplier = 1;
        comboText.text = "Combo:" + combo* currentMultiplier;
    }

    /// <summary>
    /// 触发暂停（由音符越过判定线时调用）
    /// 只设置状态，不启动协程
    /// </summary>
    public void TriggerStill()
    {
        if (!nodeStill)
        {
            nodeStill = true;
            nodeAuto = true; // 同时启动 auto
            stillStartTime = Time.time;
        }
    }
    
    private float stillStartTime = 0f;
    
    /// <summary>
    /// 每帧检查暂停状态（在 Update 中调用）
    /// </summary>
    private void CheckStillTimeout()
    {
        if (nodeStill)
        {
            // 检查是否超时（3秒）
            if (Time.time - stillStartTime >= 3f)
            {
                // 超时，自动恢复
                nodeStill = false;
                // nodeAuto 保持 true，继续自动弹奏
                
                // 显示疲惫表情
                if (character != null)
                {
                    character.ChangeImage(1);
                }
                combo = 0;
            }
        }
    }
    
    public void NoteAuto(bool autofactor)
    {
        // 只设置全局状态，每个音符会在 Update 中自己检查
        nodeAuto = autofactor;
    }
    
    /// <summary>
    /// 恢复所有音符的运动（用户按键时调用）
    /// 只需设置全局状态，每个音符会在 Update 中自己检查
    /// </summary>
    public void ResumeNoteMovement()
    {
        nodeStill = false;
        nodeAuto = false; // 停止 auto
    }

    public class Config
    {
        public int base_interval { get; set; }
        public int et { get; set; }
        public int minimum_distance { get; set; }
        public int max_overpressure { get; set; }
        public float overpressure_probability { get; set; }
        public int offset { get; set; }
        public float bear_tempo { get; set; }
        public int mid_start { get; set; }
        public bool slice_mid { get; set; }
    }
    string[] get_content(string sheet_path, string config_path)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // For Android, we cannot use File.ReadAllText on StreamingAssets directly if inside APK.
        // However, since this method must be synchronous (based on usage in LoadAndPlay), 
        // and UnityWebRequest is async, we have a structural problem for Android.
        // For now, to fix compilation, we use File.ReadAllText which works on Editor/PC.
        // If Android support is needed, LoadAndPlay needs refactoring to yield return the read request.
        Debug.LogError("Synchronous streaming assets read on Android not fully supported in this structure.");
        return new string[] { "", "" }; 
#else
        // Use standard file IO for Editor/PC
        // Input paths are already full paths from Start()
        string p1 = File.ReadAllText(sheet_path);
        string p2 = File.ReadAllText(config_path);
        return new string[] { p1, p2 };
#endif
    }

        void SpawnNote(char ch, bool harbor, float bear_tempo)//根据固定字符生成对应预制件
        {
            ch = (!"zxvbnasfghqwrty!@$%^*(iok".Contains(ch)) &&char.IsLetter(ch) ? char.ToUpper(ch) : ch;

            if (noteDictionary.TryGetValue(ch, out GameObject notePrefab) && noteLocation.TryGetValue(ch, out Vector3 notePosition))
            {
                GameObject temp_node = Instantiate(notePrefab, notePosition, Quaternion.identity) as GameObject;
                SpriteRenderer temp_renderer = temp_node.GetComponent<SpriteRenderer>();
                if (harbor)//设置隐藏按键的tag及半透明
                {
                    temp_node.tag = "Harbor";
                    var color = temp_renderer.color;
                    color.a = 0.3f; // Adjust alpha to make semi-transparent, set value between 0 (transparent) to 1 (opaque)
                    temp_renderer.color = color;
                }
                temp_renderer.sortingOrder = currentOrder--;

                NoteObject note_object = temp_node.GetComponent<NoteObject>();
                note_object.initialTime = Time.time;
                note_object.bearTempo = bear_tempo;
                note_object.keyChar = ch;
                
                // 添加到新的音游核心（使用 char 作为轨道 key）
                rhythmCore.AddNote(ch, note_object);
                
                // 同时添加到旧的队列（保持兼容）
                managerNote.Enqueue(ch, note_object);
                
                if (nodeAuto)
                {
                    note_object.autoMode = true;
                }
            }
            else
            {
                Debug.Log($"\"{ch}\" not found in noteDictionary");
            }
        }
    public IEnumerator LoadAndPlay(string sheet_path, string config_path)
    {
        StartCoroutine(AnalyzeMid.AnalyzeMidFile());
        string[] content_result = get_content(sheet_path, config_path);
        string sheet_content = content_result[0];
        string config_content = content_result[1];
        
        // 存储原始谱面内容用于显示
        OriginalSheetContent = sheet_content;
        CurrentSheetIndex = 0;
        IsPlaying = true;

        var yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance) // Use underscored naming convention
                .Build();
        Config config = yamlDeserializer.Deserialize<Config>(config_content);

        //Debug.Log($"config result:{config.base_interval},{config.et}");
        string lines = SpectrogramSimplifier.SimplifySpectrogram(sheet_content, config.minimum_distance, config.max_overpressure,config.overpressure_probability);
        lines = SpectrogramSimplifier.MapGroups(lines, config.offset);
        string temp_sheet_music_path = Path.Combine(Application.streamingAssetsPath, "SheetMusic") + "/temp_sheet_music.txt";//临时谱面文件
        using (StreamWriter temp_sheet_writer = new StreamWriter(temp_sheet_music_path))
        {
            temp_sheet_writer.WriteLine($"简化后谱面：\n{lines}");
            string piano_key = "";
            foreach (char ch in lines)
                piano_key += noteAudio.ContainsKey(ch) ? noteAudio[ch] : ch.ToString();
            temp_sheet_writer.WriteLine($"\n琴键对应谱：{piano_key}");
        }
        int next_bpm = 1;
        int index = 0;
        int originalIndex = 0; // 跟踪原始谱面中的位置
        while (index < lines.Length)
        {
            List<(char noteChar, bool harborFlag)> simultaneousNotes = new List<(char noteChar, bool harborFlag)>();
            bool tempHarborFlag = false; // 临时的 harborFlag，用于下一个音符

            // Collect simultaneous notes
            while (index < lines.Length)
            {
                char ch = lines[index];
                if (ch == '~')
                {
                    tempHarborFlag = true; // 设置临时的 harborFlag
                    index++;
                }
                else if (char.IsLetterOrDigit(ch) || "!@$%^*(".Contains(ch))
                {
                    // 将音符和对应的 harborFlag 添加到列表中
                    simultaneousNotes.Add((ch, tempHarborFlag));
                    tempHarborFlag = false; // 重置临时 harborFlag
                    index++;
                }
                else
                    break;
            }

            // Spawn all collected notes together
            foreach (var (noteChar, harbor) in simultaneousNotes)
            {
                SpawnNote(noteChar, harbor, config.bear_tempo);
                // 更新原始谱面位置：在原始谱面中查找对应的音符
                while (originalIndex < sheet_content.Length)
                {
                    char origChar = sheet_content[originalIndex];
                    originalIndex++;
                    // 找到匹配的音符字符就停止
                    if (char.IsLetterOrDigit(origChar) || "!@$%^*(".Contains(origChar))
                    {
                        CurrentSheetIndex = originalIndex;
                        break;
                    }
                }
            }

            // Handle timing character
            if (index < lines.Length)
            {
                yield return new WaitWhile(() => nodeStill);
                char ch = lines[index];
                if ("-=+.".IndexOf(ch) != -1)
                {
                    float interval = noteTime[ch] * config.base_interval / 1000f;
                    if (interval > 0) { 
                        yield return new WaitWhile(() => nodeStill);
                        yield return new WaitForSeconds(interval);
                        yield return new WaitWhile(() => nodeStill);
                    }
                    else
                        Debug.Log("error interval");
                    index++;
                    // 更新原始谱面位置：跳过时间符号
                    while (originalIndex < sheet_content.Length && "-=+.".IndexOf(sheet_content[originalIndex]) != -1)
                    {
                        originalIndex++;
                        CurrentSheetIndex = originalIndex;
                    }
                }
                else if (ch == '>')
                {
                    if (next_bpm < AnalyzeMid.bpm_data.Count)
                    {
                        config.base_interval = 60000 / AnalyzeMid.bpm_data[next_bpm].bpm / 4;
                        next_bpm += 1;
                    }
                    index++;
                    // 更新原始谱面位置：跳过BPM变化符号
                    if (originalIndex < sheet_content.Length && sheet_content[originalIndex] == '>')
                    {
                        originalIndex++;
                        CurrentSheetIndex = originalIndex;
                    }
                }
                else
                    index++;
            }
        }
        IsPlaying = false; // 演奏结束
        // 等待所有音符处理完毕（使用新的 rhythmCore 系统）
        while (rhythmCore.Count > 0)
            yield return null;
        yield return new WaitForSeconds(3);  // 等待 3 秒后返回选谱界面
        SceneManager.LoadScene("front_page");
    }


    string[] GetAudioFiles(string folderPath)
    {
        string[] mp3Files = Directory.GetFiles(folderPath, "*.mp3");
        string[] oggFiles = Directory.GetFiles(folderPath, "*.ogg");
        string[] wavFiles = Directory.GetFiles(folderPath, "*.wav");
        return mp3Files.Concat(oggFiles).Concat(wavFiles).ToArray();
    }

    IEnumerator PlayBgm(string filePath) //背景音乐
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }
   
    string[] GetWheezeList()
    {
        string Wheeze_path = Path.Combine(Application.streamingAssetsPath, "SheetMusic/extra");
        if (Directory.Exists(Wheeze_path))
        {
            // 获取所有音频文件路径
            if (Wheeze_path.Length > 0)
            {
                string[] mp3Files = Directory.GetFiles(Wheeze_path, "*.mp3");
                string[] oggFiles = Directory.GetFiles(Wheeze_path, "*.ogg");
                string[] wavFiles = Directory.GetFiles(Wheeze_path, "*.wav");
                return mp3Files.Concat(oggFiles).Concat(wavFiles).ToArray();
                
            }
            else
            {
                return new string[0];
            }
        }
        else
        {
            return new string[0];
        }
    }
    public void PlayAudio(string filePath,char key='\0')
    {
        StartCoroutine(PlayAudioClip(filePath,key));
    }
    private IEnumerator PlayAudioClip(string filePath, char key = '\0')
    {
        // 对路径进行 URL 编码，确保处理特殊字符（如 #）
        string url = "file://" + Uri.EscapeUriString(filePath);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading audio: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.Play();
                // 销毁AudioSource组件当音频播放完毕
                Destroy(audioSource, clip.length);
            }
        }
    }
}
