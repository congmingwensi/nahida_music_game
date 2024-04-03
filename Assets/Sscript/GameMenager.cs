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
using System.Text;

public class GameMenager : MonoBehaviour
{
    public static GameMenager insrance;//其他脚本会调用GameMenager
    public AudioSource audioSource; //读取文件后，用于存储音频
    public string sheetMusic;//谱面
    private bool startPlaying;//谱面音乐


    private int CurrentScore;
    public int NoteScore = 100;

    public int scorePrefict = 150;
    public int scoreGood = 100;
    public Text scoreText;
    public Text multiplierText;
    public Text comboText;

    private int currentMultiplier;
    private int combo;
    public int[] multiplierThresholds;

    private Dictionary<KeyCode, ButtonController> buttonMap;
    private Dictionary<KeyCode, List<NoteObject>> notesOnTracks = new Dictionary<KeyCode, List<NoteObject>>();//所有可按下的note
    private Dictionary<KeyCode, List<NoteObject>> notesGlobal = new Dictionary<KeyCode, List<NoteObject>>();//全局所有的note
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
    private Dictionary<char, int> noteTime;//谱面文件的时间间隔符
    private bool nodeStill;
    private bool nodeAuto;

    //private float differenceWithinOneSecond;//在检测到来不及按下的时候，会停顿所有音符1s，但是在这1s内按下了，会把所有node变为运动状态。所以 所有的node会有（1s-按下）的差值。如果是该情况，需要让所有NoteObject.initialTime-差值。此变量记录该差值的时间

    // Start is called before the first frame update
    void Start()
    {
        nodeStill = false;
        insrance = this;
        CurrentScore = 0;//初始化分数，combo加分比，显示内容
        currentMultiplier = 1;
        scoreText.text = "Score:0";
        multiplierText.text = "Multipllier:1";

        KeyCode[] keys = new KeyCode[] {//需要对每个keycode的value生成一个NodeObject list对象。不然没办法把单个NodeObject添加进去
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B,
        KeyCode.N, KeyCode.M, KeyCode.A, KeyCode.S, KeyCode.D,
        KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.Q,
        KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y,
        KeyCode.U, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7,
        KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.I,
        KeyCode.O, KeyCode.K, KeyCode.L
        };
        foreach (KeyCode key in keys)
        {
            notesOnTracks[key] = new List<NoteObject>();
            notesGlobal[key] = new List<NoteObject>();
        }


        buttonMap = new Dictionary<KeyCode, ButtonController>(); //key:button 用key控制对应的button做出一些行为
        ButtonController[] buttons = FindObjectsOfType<ButtonController>();
        foreach (ButtonController button in buttons)
        {
            buttonMap[button.keyToPress] = button;
        }

        noteDictionary = new Dictionary<char, GameObject>//按键对应的预制件。读取谱面文件后，生成对应预制件
        {
            {'Z', NoteZ},
            {'X', NoteX},
            {'C', NoteC},
            {'V', NoteV},
            {'B', NoteB},
            {'N', NoteN},
            {'M', NoteM},
            {'A', NoteA},
            {'S', NoteS},
            {'D', NoteD},
            {'F', NoteF},
            {'G', NoteG},
            {'H', NoteH},
            {'J', NoteJ},
            {'Q', NoteQ},
            {'W', NoteW},
            {'E', NoteE},
            {'R', NoteR},
            {'T', NoteT},
            {'Y', NoteY},
            {'U', NoteU},
            {'1', Note1},
            {'2', Note2},
            {'3', Note3},
            {'4', Note4},
            {'5', Note5},
            {'6', Note6},
            {'7', Note7},
            {'8', Note8},
            {'9', Note9},
            {'0', Note0},
            {'I', NoteI},
            {'O', NoteO},
            {'K', NoteK},
            {'L', NoteL}
        };
        noteLocation = new Dictionary<char, Vector3> //不同音符生成的初始位置，主要是x轴不同
        {
            { 'Z',new Vector3(-9.65f, 11.5f, 0f)},
            { 'X',new Vector3(-9.14f, 11.5f, 0f)},
            { 'C',new Vector3(-8.59f, 11.5f, 0f)},
            { 'V',new Vector3(-8.02f, 11.5f, 0f)},
            { 'B',new Vector3(-7.44f, 11.5f, 0f)},
            { 'N',new Vector3(-6.89f, 11.5f, 0f)},
            { 'M',new Vector3(-6.26f, 11.5f, 0f)},

            { 'A',new Vector3(-5.60f, 11.5f, 0f)},
            { 'S',new Vector3(-5.13f, 11.5f, 0f)},
            { 'D',new Vector3(-4.58f, 11.5f, 0f)},
            { 'F',new Vector3(-4.00f, 11.5f, 0f)},
            { 'G',new Vector3(-3.42f, 11.5f, 0f)},
            { 'H',new Vector3(-2.87f, 11.5f, 0f)},
            { 'J',new Vector3(-2.30f, 11.5f, 0f)},

            { 'Q',new Vector3(-1.73f, 11.5f, 0f)},
            { 'W',new Vector3(-1.15f, 11.5f, 0f)},
            { 'E',new Vector3(-0.60f, 11.5f, 0f)},
            { 'R',new Vector3(-0.02f, 11.5f, 0f)},
            { 'T',new Vector3(0.55f, 11.5f, 0f)},
            { 'Y',new Vector3(1.10f, 11.5f, 0f)},
            { 'U',new Vector3(1.68f, 11.5f, 0f)},

            { '1',new Vector3(2.26f, 11.5f, 0f)},
            { '2',new Vector3(2.84f, 11.5f, 0f)},
            { '3',new Vector3(3.39f, 11.5f, 0f)},
            { '4',new Vector3(3.97f, 11.5f, 0f)},
            { '5',new Vector3(4.54f, 11.5f, 0f)},
            { '6',new Vector3(5.09f, 11.5f, 0f)},
            { '7',new Vector3(5.67f, 11.5f, 0f)},

            { '8',new Vector3(6.26f, 11.5f, 0f)},
            { '9',new Vector3(6.84f, 11.5f, 0f)},
            { '0',new Vector3(7.39f, 11.5f, 0f)},
            { 'I',new Vector3(7.97f, 11.5f, 0f)},
            { 'O',new Vector3(8.55f, 11.5f, 0f)},
            { 'K',new Vector3(9.10f, 11.5f, 0f)},
            { 'L',new Vector3(9.68f, 11.5f, 0f)},
        };
        noteTime = new Dictionary<char, int>//谱面文件的时间间隔符
        {
            { '=', 1 },
            { '-', 2 },
            { '+', 4 },
        };


        sheetMusic = Path.Combine(Application.streamingAssetsPath, "SheetMusic");//读取谱面文件
        string FileSheet = sheetMusic + "/test_sheet_music.txt";
        string FileConfig = sheetMusic + "/test_sheet_music.yaml";
        StartCoroutine(LoadAndPlay(FileSheet, FileConfig));
    }
    // Update is called once per frame
    void Update()
    {
        CheckForKeyPress(KeyCode.Z);//不停地检测按键按下
        CheckForKeyPress(KeyCode.X);
        CheckForKeyPress(KeyCode.C);
        CheckForKeyPress(KeyCode.V);
        CheckForKeyPress(KeyCode.B);
        CheckForKeyPress(KeyCode.N);
        CheckForKeyPress(KeyCode.M);

        CheckForKeyPress(KeyCode.A);
        CheckForKeyPress(KeyCode.S);
        CheckForKeyPress(KeyCode.D);
        CheckForKeyPress(KeyCode.F);
        CheckForKeyPress(KeyCode.G);
        CheckForKeyPress(KeyCode.H);
        CheckForKeyPress(KeyCode.J);

        CheckForKeyPress(KeyCode.Q);
        CheckForKeyPress(KeyCode.W);
        CheckForKeyPress(KeyCode.E);
        CheckForKeyPress(KeyCode.R);
        CheckForKeyPress(KeyCode.T);
        CheckForKeyPress(KeyCode.Y);
        CheckForKeyPress(KeyCode.U);

        CheckForKeyPress(KeyCode.Alpha1);
        CheckForKeyPress(KeyCode.Alpha2);
        CheckForKeyPress(KeyCode.Alpha3);
        CheckForKeyPress(KeyCode.Alpha4);
        CheckForKeyPress(KeyCode.Alpha5);
        CheckForKeyPress(KeyCode.Alpha6);
        CheckForKeyPress(KeyCode.Alpha7);

        CheckForKeyPress(KeyCode.Alpha8);
        CheckForKeyPress(KeyCode.Alpha9);
        CheckForKeyPress(KeyCode.Alpha0);
        CheckForKeyPress(KeyCode.I);
        CheckForKeyPress(KeyCode.O);
        CheckForKeyPress(KeyCode.K);
        CheckForKeyPress(KeyCode.L);

        if (!startPlaying)//检测音频未播放，播放音频
        {
            startPlaying = true;
            string[] sheet_music_path = GetAudioFiles(sheetMusic);
            if (sheet_music_path.Length > 0)
            {
                StartCoroutine(PlayAudio(sheet_music_path[0])); // 播放第一个音频文件
            }
        }
    }

    public void PlayNote(KeyCode key) //移动端播放按键音
    {
        if (buttonMap.TryGetValue(key, out ButtonController buttonController))
            buttonController.audioSource.Play();
    }

    public void CheckForKeyPress(KeyCode key, bool key_down = false)//处理按键按下。key_down用于移动端强制key_down，在NoteObject.OnPointerClick调用
    {
        void CheckValueRange(KeyCode key, float value)
        {
            if (buttonMap.TryGetValue(key, out ButtonController buttonController))
            {
                if (value > 0.3f)
                {
                    buttonController.ChangePQuickImage();
                    CurrentScore += scoreGood;
                }
                else if (value < -0.3f)
                {
                    buttonController.ChangePSlowImage();
                    CurrentScore += scoreGood;
                }
                else
                {
                    buttonController.ChangePpercevtImage();
                    CurrentScore += scorePrefict;
                }
            }
        }

        if (Input.GetKeyDown(key) || key_down)
        {
            NodeAuto(false);
            if (notesOnTracks[key].Count > 0)
            {
                NoteObject noteToHit = notesOnTracks[key][0];
                CheckValueRange(key, noteToHit.HitNote());
                NodeHit();
                //if((Time.time-differenceWithinOneSecond)<1 && differenceWithinOneSecond!=0)
                //{
                //    NodeReduceError();
                //    differenceWithinOneSecond = 0;//因为多压的时候，所有键都会满足该条件。所以只需要减少一次就够了。赋给0表示已运行。然后待NodeStill重新赋值
                //}
            }
            else
                Debug.Log("Count < 0");

        }
    }
    public void AddNoteToTrack(NoteObject note, KeyCode trackKey)//将node添加到可按下列表，进行管理
    {
        notesOnTracks[trackKey].Add(note);
    }

    public void RemoveNoteFromTrack(KeyCode trackKey)//将列表中的node删除，已按下或miss时调用
    {
        if (notesOnTracks[trackKey].Count != 0)
            notesOnTracks[trackKey].RemoveAt(0);
        if (notesGlobal[trackKey].Count != 0)
            notesGlobal[trackKey].RemoveAt(0);
    }
    public void NodeHit() //显示分数和combo
    {
        combo++;
        comboText.text = "Combo:" + combo;
        if (multiplierThresholds[currentMultiplier - 1] < combo && currentMultiplier != 3)
        {
            currentMultiplier++;
            multiplierText.text = "Multipllier:" + currentMultiplier;
        }
        scoreText.text = "Score:" + CurrentScore;

    }
    public void NodeMissed()//miss时改变combo和combo加成
    {
        combo = 0;
        currentMultiplier = 1;
        multiplierText.text = "Multipllier:" + currentMultiplier;
        comboText.text = "Combo:" + combo;
    }

    public void NodeStill()
    {
        IEnumerator WaitAllNotesResume()
        {
            nodeStill = true;
            var startTime = Time.time;
            foreach (var track in notesGlobal)
            {
                foreach (var note in track.Value)
                {
                    note.nodeSports = false; // 重新启动运动
                }
            }

            // 循环等待，直到时间超过3秒或所有按键都被处理
            while (Time.time - startTime < 3f)
            {
                if (notesOnTracks.All(pair => pair.Value.Count == 0))
                {
                    break; // 如果所有按键都已处理，则提前退出循环
                }
                yield return new WaitForSeconds(0.01f); // 短暂等待，避免持续占用
            }

            // 对所有 note 对象应用后续操作
            foreach (var track in notesGlobal)
            {
                foreach (var note in track.Value)
                {
                    note.nodeSports = true; // 重新启动运动
                }
            }
            nodeStill = false;
        }
        StartCoroutine(WaitAllNotesResume()); // 启动协程
    }


    public void NodeSports(bool sports = true)//当可判定范围内（可按下）的键为0时，不需要傻傻的停止，而是开始下落运动
    {
        nodeStill = !sports;
        foreach (var track in notesGlobal)
        {
            foreach (var note in track.Value)
            {
                note.nodeSports = sports;
            }
        }
    }
    public void NodeAuto(bool autofactor)//设置全局的auto模式。nodeAuto代表新创建的node，note.autoMode为已有的node
    {
        nodeAuto = autofactor;
        foreach (var track in notesGlobal)
        {
            foreach (var note in track.Value)
            {
                note.autoMode = autofactor;
            }
        }
    }
    
    public static string SimplifySpectrogram(string spectrogram)
    {
        StringBuilder simplified = new StringBuilder();
        List<char> notes = new List<char>();
        List<char> intervals = new List<char> { '=', '-', '+' };
        char lastNote = char.MinValue;
        int lastNoteIndex = -1;

        for (int i = 0; i < spectrogram.Length; i++)
        {
            if (!intervals.Contains(spectrogram[i]))
            {
                notes.Add(spectrogram[i]);
            }
            else
            {
                if (notes.Count > 0)
                {
                    // Rule 1: Reduce to two notes if more than two
                    if (notes.Count > 2)
                    {
                        simplified.Append(notes[0]);
                        simplified.Append(notes[notes.Count - 1]);
                    }
                    else
                    {
                        foreach (var note in notes)
                        {
                            simplified.Append(note);
                        }
                    }
                    notes.Clear();
                }

                // Rule 2: Handle repetition and intervals
                if (spectrogram[i] == lastNote && (i - lastNoteIndex) <= 2)
                {
                    // If the same note is repeated too quickly, simplify
                    simplified.Append("!="); // Mark the previous note as not pressed
                }
                else
                {
                    simplified.Append(spectrogram[i]);
                }
                lastNote = spectrogram[i];
                lastNoteIndex = i;
            }
        }

        // Append any remaining notes
        if (notes.Count > 0)
        {
            if (notes.Count > 2)
            {
                simplified.Append(notes[0]);
                simplified.Append(notes[notes.Count - 1]);
            }
            else
            {
                foreach (var note in notes)
                {
                    simplified.Append(note);
                }
            }
        }

        return simplified.ToString();
    }

    class Config//yaml文件的数据结构
    {
        public int base_interval { get; set; }
        public int et { get; set; }
        public int minimum_distance { get; set; }
        public int max_overpressure { get; set; }
        public int offset { get; set; }
        public float bear_tempo { get; set; }
    }
    IEnumerator LoadAndPlay(string sheet_path, string config_path) //读取谱面和yaml文件内容
    {
        string[] get_content(string base_sheet, string base_config)
        {
            string sheet_content = get_file_context(base_sheet);
            string config_content = get_file_context(base_config);
            return new string[] { sheet_content, config_content };
        }

        string get_file_context(string file_path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR //本来想做安卓端按键适配，后来发现，安装太难读取本地文件夹，所以此功能暂时没用。else是Pc端读取文件代码
        string full_path = Path.Combine("jar:file://" + Application.dataPath + "!/assets/", file_path);
        UnityWebRequest request = UnityWebRequest.Get(full_path);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError("Error loading file: " + request.error);
        }
        else
        {
            return request.downloadHandler.text;
        }
#else
            string path = Path.Combine(Application.streamingAssetsPath, file_path);
            return File.ReadAllText(path);
#endif
        }

        void SpawnNote(char ch, bool harbor,float bear_tempo)//根据固定字符生成对应预制件
        {
            KeyCode CharToKeyCode(char ch, bool harbor = false)
            {
                switch (ch)
                {
                    case 'Z': return KeyCode.Z;
                    case 'X': return KeyCode.X;
                    case 'C': return KeyCode.C;
                    case 'V': return KeyCode.V;
                    case 'B': return KeyCode.B;
                    case 'N': return KeyCode.N;
                    case 'M': return KeyCode.M;
                    case 'A': return KeyCode.A;
                    case 'S': return KeyCode.S;
                    case 'D': return KeyCode.D;
                    case 'F': return KeyCode.F;
                    case 'G': return KeyCode.G;
                    case 'H': return KeyCode.H;
                    case 'J': return KeyCode.J;
                    case 'Q': return KeyCode.Q;
                    case 'W': return KeyCode.W;
                    case 'E': return KeyCode.E;
                    case 'R': return KeyCode.R;
                    case 'T': return KeyCode.T;
                    case 'Y': return KeyCode.Y;
                    case 'U': return KeyCode.U;
                    case '1': return KeyCode.Alpha1;
                    case '2': return KeyCode.Alpha2;
                    case '3': return KeyCode.Alpha3;
                    case '4': return KeyCode.Alpha4;
                    case '5': return KeyCode.Alpha5;
                    case '6': return KeyCode.Alpha6;
                    case '7': return KeyCode.Alpha7;
                    case '8': return KeyCode.Alpha8;
                    case '9': return KeyCode.Alpha9;
                    case '0': return KeyCode.Alpha0;
                    case 'I': return KeyCode.I;
                    case 'O': return KeyCode.O;
                    case 'K': return KeyCode.K;
                    case 'L': return KeyCode.L;
                    default: return KeyCode.None; // 或者其他默认行为
                }
            }

            ch = char.IsLetter(ch) ? char.ToUpper(ch) : ch;

            if (noteDictionary.TryGetValue(ch, out GameObject notePrefab) && noteLocation.TryGetValue(ch, out Vector3 notePosition))
            {
                GameObject temp_node = Instantiate(notePrefab, notePosition, Quaternion.identity) as GameObject;
                SpriteRenderer temp_renderer = temp_node.GetComponent<SpriteRenderer>();
                if (harbor)//设置隐藏按键的tag及半透明
                {
                    temp_node.tag = "Harbor";
                    var color = temp_renderer.color;
                    color.a = 0.4f; // Adjust alpha to make semi-transparent, set value between 0 (transparent) to 1 (opaque)
                    temp_renderer.color = color;
                }
                temp_renderer.sortingOrder = currentOrder--;

                NoteObject note_object = temp_node.GetComponent<NoteObject>();
                note_object.initialTime = Time.time;
                note_object.bearTempo = bear_tempo;

                notesGlobal[CharToKeyCode(ch)].Add(note_object);
                //Debug.Log($"note_object.gameObject.name:{note_object.gameObject.name}，GameMenager nodeAuto：{nodeAuto}");
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

        string[] content_result = get_content(sheet_path, config_path);
        string sheet_content = content_result[0];
        string config_content = content_result[1];

        var yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance) // Use underscored naming convention
                .Build();
        Config config = yamlDeserializer.Deserialize<Config>(config_content);

        //Debug.Log($"config result:{config.base_interval},{config.et}");
        bool harbor = false;
        string lines = SpectrogramSimplifier.SimplifySpectrogram(sheet_content,config.minimum_distance,config.max_overpressure);
        lines = SpectrogramSimplifier.MapGroups(lines, config.offset);
        foreach (char ch in lines)
        {
            if (char.IsLetter(ch) || char.IsDigit(ch))
            {
                yield return new WaitWhile(() => nodeStill);
                if (harbor)
                {
                    SpawnNote(ch, true, config.bear_tempo);
                    harbor = false;
                }
                else
                    SpawnNote(ch,false, config.bear_tempo);
            }
            else if ("-=+".IndexOf(ch) != -1)
            {
                float interval = noteTime[ch] * config.base_interval / 1000f;
                if (interval > 0)
                    yield return new WaitForSeconds(interval);
                else
                    Debug.Log("error interval");
                yield return new WaitWhile(() => nodeStill);
            }
            else if (ch == '!')
                harbor = true;
        }
    }


    string[] GetAudioFiles(string folderPath)//判断谱面文件包含音频文件
    {
        string[] mp3Files = Directory.GetFiles(folderPath, "*.mp3");
        string[] oggFiles = Directory.GetFiles(folderPath, "*.ogg");
        string[] wavFiles = Directory.GetFiles(folderPath, "*.wav");

        return mp3Files.Concat(oggFiles).Concat(wavFiles).ToArray();
    }

    IEnumerator PlayAudio(string filePath)//播放谱面文件中的音频文件
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

}
