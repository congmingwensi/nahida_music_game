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
    public AudioSource audioSource; //��ȡ�ļ������ڴ洢��Ƶ
    public string sheetMusic;//����
    private bool startPlaying;//��������
    public string[] WheezeList;//�����ļ��б�
    
    private int CurrentScore;
    public int scorePrefict = 100;
    public int scoreGood = 75;
    public Text scoreText;
    public Text comboText;

    private int currentMultiplier;
    private int combo;
    public int[] multiplierThresholds;

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
    private int currentOrder = Int32.MaxValue;//����node��ʾ���ȼ�����int���ֵ�ݼ�
    private Dictionary<char, float> noteTime;//�����ļ���ʱ������
    public Dictionary<char, string> noteAudio;
    private bool nodeStill;
    private bool nodeAuto;
    private Dictionary<char, AudioSource> playingNotes = new Dictionary<char, AudioSource>();
    CharacterControl character;
    
    public Dictionary<char, KeyNotePlayer> notePlayers = new Dictionary<char, KeyNotePlayer>();  // ÿ���ټ�һ��������
    public AudioSource audioSourcePrefab;
    private HashSet<KeyCode> processedKeys = new HashSet<KeyCode>();
    private Dictionary<KeyCode, bool> previousKeyStates = new Dictionary<KeyCode, bool>();
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
        managerNote = new CharGameObjectQueue();
        KeyCode[] keys = new KeyCode[] {//��Ҫ��ÿ��keycode��value����һ��NodeObject list���󡣲�Ȼû�취�ѵ���NodeObject��ӽ�ȥ
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B,
        KeyCode.N, KeyCode.M, KeyCode.A, KeyCode.S, KeyCode.D,
        KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.Q,
        KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y,
        KeyCode.U, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7,
        KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0, KeyCode.I,
        KeyCode.O, KeyCode.K, KeyCode.L
        };
        buttonMap = new Dictionary<KeyCode, ButtonController>(); //key:button ��key���ƶ�Ӧ��button����һЩ��Ϊ
        ButtonController[] buttons = FindObjectsOfType<ButtonController>();
        foreach (ButtonController button in buttons)
        {
            buttonMap[button.keyToPress] = button;
        }
        foreach (char note in "ZzXxCVvBbNnMAaSsDFfGgHhJQqWwERrTtYyU1!2@34$5%6^78*9(0IiOoKkL")  // ���ÿ������
        {
            GameObject audioObject = new GameObject("NotePlayer_" + note);
            audioObject.transform.SetParent(transform);
            KeyNotePlayer player = new KeyNotePlayer(audioObject);
            notePlayers[note] = player;
        }
        noteDictionary = new Dictionary<char, GameObject>//������Ӧ��Ԥ�Ƽ�����ȡ�����ļ������ɶ�ӦԤ�Ƽ�
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
        noteLocation = new Dictionary<char, Vector3> //��ͬ�������ɵĳ�ʼλ�ã���Ҫ��x�᲻ͬ
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
        noteTime = new Dictionary<char, float>//�����ļ���ʱ������
        {
            { '.', 0.5f},
            { '=', 1 },
            { '-', 2 },
            { '+', 4 },
        };
       
        
        sheetMusic = Path.Combine(Application.streamingAssetsPath, "SheetMusic");//��ȡ�����ļ�
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
        var expiredKeys = GlobalKeyPresses.KeyPresses.Where(keyPress => Time.time - keyPress.TimePressed > 0.2f).ToList();
        foreach (var expiredKey in expiredKeys)
        {
            GlobalKeyPresses.KeyPresses.Remove(expiredKey);
            if (processedKeys.Contains(expiredKey.Key))
            {
                processedKeys.Remove(expiredKey.Key);
                Debug.Log($"���� {expiredKey.Key} ��ʱ���� processedKeys ���Ƴ�");
            }
        }
        CheckForKeyPress(KeyCode.Z);//��ͣ�ؼ�ⰴ������
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

        if (!startPlaying)//�����Ƶδ���ţ�������Ƶ
        {
            startPlaying = true;
            string[] sheet_music_path = GetAudioFiles(sheetMusic);
            if (sheet_music_path.Length > 0)
            {
                StartCoroutine(PlayBgm(sheet_music_path[0])); // ���ŵ�һ����Ƶ�ļ�
            }
        }
    }

    public void PlayNote(char key='\0') //�ƶ��˲��Ű�����
    {
        if (WheezeList.Length!=0)
        {
            string selectedAudioFile = WheezeList[UnityEngine.Random.Range(0, WheezeList.Length)];
            PlayAudio(selectedAudioFile);
        }
    }
    KeyCode charToKeycode(char ch)
    {
        switch (ch)
        {
            case 'Z': return KeyCode.Z;
            case 'z': return KeyCode.Z;
            case 'X': return KeyCode.X;
            case 'x': return KeyCode.X;
            case 'C': return KeyCode.C;
            case 'V': return KeyCode.V;
            case 'v': return KeyCode.V;
            case 'B': return KeyCode.B;
            case 'b': return KeyCode.B;
            case 'N': return KeyCode.N;
            case 'n': return KeyCode.N;
            case 'M': return KeyCode.M;

            case 'A': return KeyCode.A;
            case 'a': return KeyCode.A;
            case 'S': return KeyCode.S;
            case 's': return KeyCode.S;
            case 'D': return KeyCode.D;
            case 'F': return KeyCode.F;
            case 'f': return KeyCode.F;
            case 'G': return KeyCode.G;
            case 'g': return KeyCode.G;
            case 'H': return KeyCode.H;
            case 'h': return KeyCode.H;
            case 'J': return KeyCode.J;

            case 'Q': return KeyCode.Q;
            case 'q': return KeyCode.Q;
            case 'W': return KeyCode.W;
            case 'w': return KeyCode.W;
            case 'E': return KeyCode.E;
            case 'R': return KeyCode.R;
            case 'r': return KeyCode.R;
            case 'T': return KeyCode.T;
            case 't': return KeyCode.T;
            case 'Y': return KeyCode.Y;
            case 'y': return KeyCode.Y;
            case 'U': return KeyCode.U;

            case '1': return KeyCode.Alpha1;
            case '!': return KeyCode.Alpha1;
            case '2': return KeyCode.Alpha2;
            case '@': return KeyCode.Alpha2;
            case '3': return KeyCode.Alpha3;
            case '4': return KeyCode.Alpha4;
            case '$': return KeyCode.Alpha4;
            case '5': return KeyCode.Alpha5;
            case '%': return KeyCode.Alpha5;
            case '6': return KeyCode.Alpha6;
            case '^': return KeyCode.Alpha6;
            case '7': return KeyCode.Alpha7;

            case '8': return KeyCode.Alpha8;
            case '*': return KeyCode.Alpha8;
            case '9': return KeyCode.Alpha9;
            case '(': return KeyCode.Alpha9;
            case '0': return KeyCode.Alpha0;
            case 'I': return KeyCode.I;
            case 'i': return KeyCode.I;
            case 'O': return KeyCode.O;
            case 'o': return KeyCode.O;
            case 'K': return KeyCode.K;
            case 'k': return KeyCode.K;
            case 'L': return KeyCode.L;

            default: return KeyCode.None; // �������
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

            default: return '\0'; // �������
        }
    }
    public void CheckForKeyPress(KeyCode key, bool key_down = false)
    {
        // ��鰴�����з�Χ
        void CheckValueRange(KeyCode key, float value)
        {
            if (buttonMap.TryGetValue(key, out ButtonController buttonController))
            {
                if (value > 0.3f)
                {
                    buttonController.ChangePQuickImage();
                    CurrentScore += scoreGood;
                }
                else if (value < -0.4f)
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

        // �����������¼�
        int ProcessingPress(KeyValuePair<char, NoteObject> temp_pare)
        {
            int factor = 0;
            if (managerNote.Count > 0)
            {
                // �жϰ����Ƿ��ѱ�������������ظ�
                if (!GlobalKeyPresses.KeyPresses.Any(keyPress => keyPress.Key == key))
                {
                    GlobalKeyPresses.KeyPresses.Add(new KeyPressInfo(key, Time.time)); // �洢����ʱ��
                    Debug.Log($"��ӣ�{key.ToString()}����ǰ���У�{GlobalKeyPresses.PrintAllKeysAsOneString()}");
                    factor = 1;
                }
                // ��鰴���Ƿ�������ƥ�䣬�����λƥ��
                if (temp_pare.Value.canBePressed && GlobalKeyPresses.KeyPresses.Any(keyPress => charToKeycode(temp_pare.Key) == keyPress.Key))
                {
                    // �Ƴ��Ѿ�����İ���
                    GlobalKeyPresses.KeyPresses.RemoveWhere(keyPress => charToKeycode(temp_pare.Key) == keyPress.Key);
                    Debug.Log($"ɾ��keyPress.Key֮��{temp_pare.Key},��ʣ������{GlobalKeyPresses.PrintAllKeysAsOneString()}");
                    KeyCode processedKey = charToKeycode(temp_pare.Key);
                    if (processedKeys.Contains(processedKey))
                    {
                        processedKeys.Remove(processedKey);
                        Debug.Log($"�� processedKeys ���Ƴ�������{processedKey}");
                    }
                    NoteObject noteToHit = temp_pare.Value;
                    CheckValueRange(key, noteToHit.HitNote());
                    NoteHit();
                    factor = 2;
                }
            }
            return factor;
        }

        // ��ⰴ������
        bool keyDownThisFrame = Input.GetKeyDown(key);
        bool keyUpThisFrame = Input.GetKeyUp(key);
        bool keyIsPressed = Input.GetKey(key);
        if (!previousKeyStates.ContainsKey(key))
        {
            previousKeyStates[key] = false;
        }
        bool wasKeyDown = previousKeyStates[key];
        if ((keyIsPressed && !wasKeyDown) || key_down || GlobalKeyPresses.KeyPresses.Any(keyPress => keyPress.Key == key))
        {
            Debug.Log($"Frame: {Time.frameCount}, Key: {key}, IsKeyDown: {keyDownThisFrame}, WasKeyDown: {wasKeyDown}, �Ѱ���: {string.Join(", ", processedKeys)}");
            if (!processedKeys.Contains(key))
            {
                processedKeys.Add(key);
                KeyValuePair<char, NoteObject> temp_pare = managerNote.GetTopElement();
                Debug.Log($"���� Key: {key}, GetKeyDown: {keyDownThisFrame}, GetKeyUp: {keyUpThisFrame}, GetKey: {keyIsPressed}���Ѱ���:{string.Join(", ", processedKeys)}\n" +
                    $"��ǰ���У�{GlobalKeyPresses.PrintAllKeysAsOneString()},��ǰ�Ӷ���{temp_pare.Key}");
                NoteAuto(false);  // ��ͣ�Զ�����
                int factor = ProcessingPress(temp_pare);
                if (factor == 2)
                {
                    if ("zxvbnasfghqwrty!@$%^*(iok".Contains(temp_pare.Key))
                        Debug.Log("��");
                    else
                        Debug.Log("��");
                    notePlayers[temp_pare.Key].PlayNoteSound(temp_pare.Key);
                }
                else if (factor==1)
                {
                    char keyChar = keyCodeToChar(key);
                    if (notePlayers.ContainsKey(keyChar))
                        notePlayers[keyChar].PlayNoteSound(keyChar);
                }
            }
        }
        if (!keyIsPressed && wasKeyDown && processedKeys.Contains(key))
        {
            processedKeys.Remove(key);
            Debug.Log($"Key: {key} ���ɿ����� processedKeys ���Ƴ�");
        }
        previousKeyStates[key] = keyIsPressed;
    }

    public void RemoveNoteFromTrack(char trackKey)//���б��е�nodeɾ�����Ѱ��»�missʱ����
    {
        if (managerNote.Count != 0 && managerNote.GetTopElement().Key == trackKey)
            managerNote.Dequeue();
    }
    public void NoteHit() //��ʾ������combo
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
    public void NoteMissed()//missʱ�ı�combo��combo�ӳ�
    {
        combo = 0;
        currentMultiplier = 1;
        comboText.text = "Combo:" + combo* currentMultiplier;
    }

    public void NoteStill()
    {
        IEnumerator WaitAllNotesResume()
        {
            nodeStill = true;
            var startTime = Time.time;
            managerNote.NoteSports(false);
            NoteObject still_key = managerNote.GetTopElement().Value;

            while (Time.time - startTime < 3f)
            {
                if (managerNote.GetTopElement().Value != still_key)
                {
                    break;
                }
                else
                {
                    managerNote.NoteSports(false);
                }
                yield return new WaitForSeconds(0.01f);
            }

            if (Time.time - startTime >= 1f)
            {
                character.ChangeImage(1);
                combo = 0;
            }
            managerNote.NoteSports(true);
            nodeStill = false;
        }
        StartCoroutine(WaitAllNotesResume());
    }

    public void NoteAuto(bool autofactor)
    {
        nodeAuto = autofactor;
        managerNote.NoteAuto(autofactor);
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
    IEnumerator LoadAndPlay(string sheet_path, string config_path) //��ȡ�����yaml�ļ�����
    {
        string[] get_content(string base_sheet, string base_config)
        {
            string sheet_content = get_file_context(base_sheet);
            string config_content = get_file_context(base_config);
            return new string[] { sheet_content, config_content };
        }

        string get_file_context(string file_path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR //����������׿�˰������䣬�������֣���װ̫�Ѷ�ȡ�����ļ��У����Դ˹�����ʱû�á�else��Pc�˶�ȡ�ļ�����
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

        void SpawnNote(char ch, bool harbor, float bear_tempo)//���ݹ̶��ַ����ɶ�ӦԤ�Ƽ�
        {
            ch = (!"zxvbnasfghqwrty!@$%^*(iok".Contains(ch)) &&char.IsLetter(ch) ? char.ToUpper(ch) : ch;

            if (noteDictionary.TryGetValue(ch, out GameObject notePrefab) && noteLocation.TryGetValue(ch, out Vector3 notePosition))
            {
                GameObject temp_node = Instantiate(notePrefab, notePosition, Quaternion.identity) as GameObject;
                SpriteRenderer temp_renderer = temp_node.GetComponent<SpriteRenderer>();
                if (harbor)//�������ذ�����tag����͸��
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
                managerNote.Enqueue(ch, note_object);
                //Debug.Log($"note_object.gameObject.name:{note_object.gameObject.name}");
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
        StartCoroutine(AnalyzeMid.AnalyzeMidFile());
        string[] content_result = get_content(sheet_path, config_path);
        string sheet_content = content_result[0];
        string config_content = content_result[1];

        var yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance) // Use underscored naming convention
                .Build();
        Config config = yamlDeserializer.Deserialize<Config>(config_content);

        //Debug.Log($"config result:{config.base_interval},{config.et}");
        bool harbor = false;
        string lines = SpectrogramSimplifier.SimplifySpectrogram(sheet_content, config.minimum_distance, config.max_overpressure,config.overpressure_probability);
        lines = SpectrogramSimplifier.MapGroups(lines, config.offset);
        string temp_sheet_music_path = Path.Combine(Application.streamingAssetsPath, "SheetMusic") + "/temp_sheet_music.txt";//��ʱ�����ļ�
        using (StreamWriter temp_sheet_writer = new StreamWriter(temp_sheet_music_path))
        {
            temp_sheet_writer.WriteLine($"�򻯺����棺\n{lines}");
            string piano_key = "";
            foreach (char ch in lines)
                piano_key += noteAudio.ContainsKey(ch) ? noteAudio[ch] : ch.ToString();
            temp_sheet_writer.WriteLine($"\n�ټ���Ӧ�ף�{piano_key}");
        }
        int next_bpm = 1;
        foreach (char ch in lines)
        {
            //Debug.Log($"ch:{ch},current_bpm:{config.base_interval}");
            if (char.IsLetter(ch) || char.IsDigit(ch) || "!@$%^*(".Contains(ch))
            {
                yield return new WaitWhile(() => nodeStill);
                if (harbor)
                {
                    SpawnNote(ch, true, config.bear_tempo);
                    harbor = false;
                }
                else
                    SpawnNote(ch, false, config.bear_tempo);
            }
            else if ("-=+.".IndexOf(ch) != -1)
            {
                float interval = noteTime[ch] * config.base_interval / 1000f;
                if (interval > 0)
                    yield return new WaitForSeconds(interval);
                else
                    Debug.Log("error interval");
                yield return new WaitWhile(() => nodeStill);
            }
            else if (ch == '~')
                harbor = true;
            else if (ch == '>') //��bpm
            {
                if (next_bpm < AnalyzeMid.bpm_data.Count)
                {
                    config.base_interval = 60000/AnalyzeMid.bpm_data[next_bpm].bpm/4;
                    next_bpm += 1;
                }
            }
        }

        while (managerNote.Count != 0)
            yield return null;  // ÿ֡�������
        yield return new WaitForSeconds(3);  // �ȴ� 3 ��
        SceneManager.LoadScene("front_page");
    }


    string[] GetAudioFiles(string folderPath)
    {
        string[] mp3Files = Directory.GetFiles(folderPath, "*.mp3");
        string[] oggFiles = Directory.GetFiles(folderPath, "*.ogg");
        string[] wavFiles = Directory.GetFiles(folderPath, "*.wav");
        return mp3Files.Concat(oggFiles).Concat(wavFiles).ToArray();
    }

    IEnumerator PlayBgm(string filePath) //��������
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
            // ��ȡ������Ƶ�ļ�·��
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
        // ��·������ URL ���룬ȷ�����������ַ����� #��
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
                // ����AudioSource�������Ƶ�������
                Destroy(audioSource, clip.length);
            }
        }
    }
}
