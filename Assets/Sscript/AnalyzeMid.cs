using System;
using System.Collections;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Text.RegularExpressions;
using SanfordMidi = Sanford.Multimedia.Midi;
using NAudioMidi = NAudio.Midi;
using UnityEngine;
using YamlDotNet.RepresentationModel;
using NAudio.Midi;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using Sanford.Multimedia.Midi;
using YamlDotNet.Core.Tokens;

public class AnalyzeMid : MonoBehaviour
{
    public static int mid_start=2;
    public static List<(int bpm, int ticks)> bpm_data = new List<(int bpm, int ticks)>();
    public static List<string> MidKeyList = new List<string> { 
        "Z", "z", "X", "x", "C", "V", "v", "B", "b", "N", "n", "M",
        "A", "a", "S", "s", "D", "F", "f", "G", "g", "H", "h", "J",
        "Q", "q", "W", "w", "E", "R", "r", "T", "t", "Y", "y", "U",
        "1", "!", "2", "@", "3", "4", "$", "5", "%", "6", "^", "7",
        "8", "*", "9", "(", "0", "I", "i", "O", "o", "K", "k", "L", };
   public static SortedDictionary<int, int> ScaleStatistics = new SortedDictionary<int, int> { };
   
    public static IEnumerator AnalyzeMidFile()
    {
        string GetMid()// ��ȡmid�ļ�����
        {
            string Wheeze_path = Path.Combine(Application.streamingAssetsPath, "SheetMusic");
            string[] MidFiles = Directory.GetFiles(Wheeze_path, "*.mid");
            Debug.Log($"Wheeze_path:{Wheeze_path}");
            if (Directory.Exists(Wheeze_path))
            {
                // ��ȡ����mid�ļ�·�����ص�һ��
                if (MidFiles.Length > 0)
                    return MidFiles[0];
                else
                    return "";
            }
            else
                return "";
        }
        void EditYaml(int base_interval,string mid_path) //�޸�yaml�ļ�base_interval
        {
            string sheetMusic = Path.Combine(Application.streamingAssetsPath, "SheetMusic");//��ȡ�����ļ�
            string FileConfig = sheetMusic + "/test_sheet_music.yaml";
            var yamlContent = File.ReadAllText(FileConfig);
            var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance) // �»��������淶
            .Build();
            var config = deserializer.Deserialize<GameMenager.Config>(yamlContent);
            if (config.slice_mid)
                SplitMidiByMultipleBpm(mid_path, mid_path);
            config.base_interval = base_interval;
            mid_start = config.mid_start;
            var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
            var newYamlContent = serializer.Serialize(config);
            File.WriteAllText(FileConfig, newYamlContent);
        }
        List<(int, int)> GetBpmData(string midiFilePath) //��ȡmid�ļ��ж��bpm�Ͷ�Ӧʱ�䣬����bpm-������ ���б�
        {
            var midiFile = new MidiFile(midiFilePath, false);
            string midi_content = "";
            for (int i = 0; i < midiFile.Tracks; i++)
            {
                midi_content += $"Track {i}:";
                var track = midiFile.Events[i];
                foreach (var midiEvent in track)
                {
                    midi_content += midiEvent;
                }
            }
            Debug.Log($"midi_content:{midi_content}");
            string pattern = @"SetTempo (\d+)+bpm \(\d+\)(\d+)";
            MatchCollection matches = Regex.Matches(midi_content, pattern);
            List<(int bpm, int ticks)> bpm_data = new List<(int bpm, int ticks)>();
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    bpm_data.Add((int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value)));
                    Debug.Log($"bpm:{match.Groups[1].Value},ticks:{match.Groups[2].Value}");
                }
            }
            bpm_data.Add((1, Int32.MaxValue));//����int���ֵticks �Ա�ʾ��������bpm�����֮�������õ�
            return bpm_data;
        }
        string IntervalStr(float temp_interval_count)//�������϶ת��Ϊ�ַ���
        {
            if (temp_interval_count == 0)
                return "";
            else if (temp_interval_count < 1)
                return temp_interval_count <0.5?".":"=";
            float fractional = temp_interval_count - (int)temp_interval_count;
            string temp_str = "";
            if (fractional > 0.3)
            {
                if (fractional > 0.7)
                    temp_str += "=";
                else
                    temp_str += '.';
            }
            int int_temp_interval_count = (int)(temp_interval_count);
            string[] symbols = { "=", "-", "+"};
            int digit=(int)(Math.Log(int_temp_interval_count) / Math.Log(2)) + 1;
            for (int i = 0; i < digit; i++) 
            {
                if ((int_temp_interval_count & Convert.ToInt32(Math.Pow(2,i)))!=0) // ���� i λ�Ƿ�Ϊ 1
                    if (i >= symbols.Length)
                        temp_str += new string('+', i-1);
                    else
                        temp_str += symbols[i];
            }
            return temp_str;   
        }
        string GetMidKey(int map_mid_key)//����mid�ļ����ֻ�ȡ��Ӧ�ַ���
        {
            if (map_mid_key < 0 || map_mid_key >= 60)
            {
                Debug.Log($"���ԣ�{map_mid_key}");
                return "";
        }
            else
                return MidKeyList[map_mid_key];
        }
        List<(int ticks, int key_number)> GetMidContent(string midi_file_path)
        {
            Dictionary<int, int> note_records = new Dictionary<int, int>();
            SanfordMidi.Sequence sequence = new SanfordMidi.Sequence();
            try
            {
                sequence.Load(midi_file_path);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.LogError($"Error loading MIDI file: {ex.Message}");
                return null;
            }

            List<(int ticks, int key_number)> mid_data = new List<(int ticks, int key_number)>();
            string sanfor_mid_content = "";
            foreach (var track in sequence)//����mid�ļ���������
            {
                foreach (SanfordMidi.MidiEvent midiEvent in track.Iterator())// ����ÿ������е�����MIDI�¼�
                {
                    var message = midiEvent.MidiMessage;
                    if (message is SanfordMidi.ChannelMessage channelMessage)
                    {
                        if (channelMessage.Command == ChannelCommand.NoteOn && channelMessage.Data2 > 0)
                            mid_data.Add((midiEvent.AbsoluteTicks, channelMessage.Data1));
                        sanfor_mid_content += $"\n ticks:{midiEvent.AbsoluteTicks},key:{channelMessage.Data1} Data2:{channelMessage.Data2}";
                    }
                }
            }
            Debug.Log($"sanfor_mid_content:{sanfor_mid_content}");
            return mid_data;
        }
        string CreateMusicSheet(string midi_file_path, List<(int bpm, int ticks)> bpm_data)//����mid�ļ����ݹ��������ַ���
        {
            List<(int ticks, int key_number)> mid_data = GetMidContent(midi_file_path);
            string music_sheet = "";
            float current_interval = 60000/bpm_data[0].bpm/4;
            int next_interval_count = bpm_data.Count == 1 ? 0 : 1;
            int current_ticks = 0;
            for(int i=0; i!=mid_data.Count;i++)//����mid�ļ��������������
            {
                Debug.Log($"mid_data[i].ticks:{mid_data[i].ticks}  mid_data[i].key_number:{mid_data[i].key_number}  current_interval:{current_interval}");
                int key_number = mid_data[i].key_number-12;
                int map_mid_key = key_number - mid_start * 12;
                ScaleStatistics[key_number / 12] = ScaleStatistics.ContainsKey(key_number / 12) ? ScaleStatistics[key_number / 12] + 1 : 1;
                if (mid_data[i].ticks == current_ticks)
                    music_sheet += GetMidKey(map_mid_key);
                else
                {
                    float temp_interval_count;
                    if (mid_data[i].ticks >= bpm_data[next_interval_count].ticks)
                    {
                        current_interval = 60000 / bpm_data[next_interval_count].bpm / 4;
                        music_sheet += ">";
                        next_interval_count++;
                    }
                    temp_interval_count = ((mid_data[i].ticks - current_ticks) / current_interval);
                    music_sheet += IntervalStr(temp_interval_count);
                    music_sheet += GetMidKey(map_mid_key);
                    Debug.Log($"�����{IntervalStr(temp_interval_count)},������{GetMidKey(map_mid_key)}");
                    Debug.Log($"music_sheet :{music_sheet}");
                    current_ticks = mid_data[i].ticks;
                }
            }
            string sheet_music_path = Path.Combine(Application.streamingAssetsPath, "SheetMusic") + "/test_sheet_music.txt";//�����ļ�
            using (StreamWriter sheet_writer = new StreamWriter(sheet_music_path))
            {
                sheet_writer.WriteLine(music_sheet);
            }
            string temp_sheet_music_path = Path.Combine(Application.streamingAssetsPath, "SheetMusic") + "/temp_sheet_music.txt";//��ʱ�����ļ�
            using (StreamWriter temp_sheet_writer = new StreamWriter(temp_sheet_music_path,false))
            {
                temp_sheet_writer.WriteLine("ÿ�����׵�������\n");
                foreach (var kvp in ScaleStatistics)
                    temp_sheet_writer.WriteLine($"����:{kvp.Key},����:{kvp.Value}");
            }
            return music_sheet;
        }
        bpm_data.Clear();
        string midi_file_path = GetMid();
        if (midi_file_path == "")
        {
            yield return null;
            yield break;
        }
        
        bpm_data = GetBpmData(midi_file_path);
        Debug.Log($"bpm_data[0].bpm:{bpm_data[0].bpm})");
        EditYaml((60000 / bpm_data[0].bpm) / 4, midi_file_path);
        string music_sheet = CreateMusicSheet(midi_file_path, bpm_data);
        Debug.Log(music_sheet);
    }

    public static void SplitMidiByMultipleBpm(string inputFilePath, string outputFilePrefix)//�ָͬbpm��mid�ļ�
    {
        SanfordMidi.Sequence sequence = new SanfordMidi.Sequence();
        sequence.Load(inputFilePath);
        List<SanfordMidi.MidiEvent> currentSegmentEvents = new List<SanfordMidi.MidiEvent>();
        int currentBpm = 0;
        int segmentCount = 0;
        foreach (Track track in sequence)
        {
            foreach (SanfordMidi.MidiEvent midiEvent in track.Iterator())
            {
                var message = midiEvent.MidiMessage;
                if (message is MetaMessage metaMessage && metaMessage.MetaType == MetaType.Tempo)
                {
                    if (currentSegmentEvents.Count > 0)
                    {
                        SaveSegmentAsMidiFile(currentSegmentEvents, outputFilePrefix, segmentCount, currentBpm);
                        segmentCount++;
                        currentSegmentEvents.Clear();  // ��յ�ǰ����
                    }
                    currentBpm = GetBpmFromMetaMessage(metaMessage);
                }
                currentSegmentEvents.Add(midiEvent);
            }
            if (currentSegmentEvents.Count > 0)
                SaveSegmentAsMidiFile(currentSegmentEvents, outputFilePrefix, segmentCount, currentBpm);
        }
    }

    private static void SaveSegmentAsMidiFile(List<SanfordMidi.MidiEvent> events, string outputFilePrefix, int segmentCount, int bpm) // ���浱ǰ���䵽MIDI�ļ�
    {
        SanfordMidi.Sequence newSequence = new SanfordMidi.Sequence();
        Track newTrack = new Track();
        foreach (SanfordMidi.MidiEvent midiEvent in events)
            newTrack.Insert(midiEvent.AbsoluteTicks, midiEvent.MidiMessage);
        newSequence.Add(newTrack);
        string inputFileNameWithoutExtension = Path.GetFileNameWithoutExtension(outputFilePrefix);
        string outputFilePath = Path.Combine(Path.GetDirectoryName(outputFilePrefix), $"{inputFileNameWithoutExtension}_Segment_{segmentCount}_Bpm_{bpm}.mid");
        newSequence.Save(outputFilePath);
    }

    private static int GetBpmFromMetaMessage(MetaMessage metaMessage)
    {
        byte[] data = metaMessage.GetBytes();
        int tempo = (data[0] << 16) | (data[1] << 8) | data[2];  // ��������ֽڳ�����
        int bpm = 60000000 / tempo;  // ΢��ת��ΪBPM
        return bpm;
    }
}

