using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows;

class SpectrogramSimplifier : MonoBehaviour
{
    public static string SimplifySpectrogram(string music_sheet, int minimum_distance = 8, int max_overpressure = 1, float overpressure_probability=0.5f)
    {
        /*
         * ��һ�еĶ�ѹ
         * ������temp_music_sheet��ԭʼ���׵�һ��ͬʱ����ַ�����new_music_sheet���򻯺�����������ַ��������Ժ����.append(�ı��ĵ�һ������)��overpressure_first/overpressure_end����ѹ�ַ�����������Ŀ�ʼ/���� λ��
         * max_overpressure������ѹ������yaml�ļ����ã�shouldAuto����ʱ���һ�����Ƿ�Ӧ��auto����yaml�ļ�minimum_distance��������ó�
         */
        void ProcessTempMusicSheet(ref StringBuilder temp_music_sheet, ref StringBuilder new_music_sheet, int overpressure_first, int overpressure_end, int max_overpressure, bool shouldAuto)
        {
            StringBuilder sort_sheet(StringBuilder temp_music_sheet)
            {
                string order = "ZzXxCVvBbNnMAaSsDFfGgHhJQqWwERrTtYyU1!2@34$5%6^78*9(0IiOoKkL";
                char[] temp_input = temp_music_sheet.ToString().ToCharArray();
                Array.Sort(temp_input, new SpecialComparer(order));
                return new StringBuilder(new string(temp_input));
            }
            temp_music_sheet = sort_sheet(temp_music_sheet);
            //UnityEngine.Debug.Log($"temp_music_sheet.ToString:{temp_music_sheet.ToString()}");
            if (shouldAuto || ((overpressure_end - overpressure_first + 1) >= (max_overpressure>1? max_overpressure:2) && max_overpressure != 0))
            {
                string tempString = temp_music_sheet.ToString();
                string autoString;
                if (shouldAuto)
                    autoString = string.Join("", tempString.Select((c, index) => index < tempString.Length ? "~" + c : c.ToString()));
                else if (temp_music_sheet.Length > 1)
                {
                    System.Random random = new System.Random();
                    if (random.NextDouble() < overpressure_probability)
                    {
                        autoString = string.Join("", tempString.Select((c, index) => index < tempString.Length - max_overpressure ? "~" + c : c.ToString()));
                        //UnityEngine.Debug.Log($"���ʲ���Ч:{autoString}");
                    }
                    else
                    {
                        autoString = string.Join("", tempString.Select((c, index) => index < tempString.Length-1 ? "~" + c : c.ToString()));
                        //UnityEngine.Debug.Log($"������Ч:{autoString}");
                    }
                }
                else
                    autoString = "~" + tempString;

                temp_music_sheet.Clear();
                temp_music_sheet.Append(autoString);
            }

            new_music_sheet.Append(temp_music_sheet);
            temp_music_sheet.Clear();
        }

        StringBuilder new_music_sheet = new StringBuilder("");
        StringBuilder temp_music_sheet = new StringBuilder("");
        float current_distance = 0;
        bool first_key = true;//第一个按键时，current_distance <= minimum_distance ，但期望不简�?
        Dictionary<char, float> interval_values = new Dictionary<char, float>
    {
            { '.', 0.5f},{'=', 1}, {'-', 2}, {'+', 4}
    };

        int overpressure_first = 0;
        for (int i = 0; i < music_sheet.Length; i++)
        {
            if (char.IsLetter(music_sheet[i]) || char.IsDigit(music_sheet[i]) || "!@$%^*(".Contains(music_sheet[i])) // Notes
            {
                temp_music_sheet.Append(music_sheet[i]);
                if (overpressure_first == 0)
                {
                    overpressure_first = i; // Mark the start of potential overpressure
                }
            }
            else if ("-=+.".IndexOf(music_sheet[i]) != -1)
            {
                current_distance += interval_values[music_sheet[i]];
                //UnityEngine.Debug.Log($"current_distance :{current_distance }");
                Console.WriteLine($"current_distance:{current_distance}");
                bool shouldAuto = false;
                if (temp_music_sheet.ToString() != "")
                {
                    if (current_distance < minimum_distance && !first_key)
                        shouldAuto = true;
                    else
                    {
                        shouldAuto = false; // Reset distance after processing
                        current_distance = 0;
                        first_key = first_key ? false : first_key;
                    }
                    ProcessTempMusicSheet(ref temp_music_sheet, ref new_music_sheet, overpressure_first, i - 1, max_overpressure, shouldAuto);
                }

                overpressure_first = 0;
                new_music_sheet.Append(music_sheet[i]);
            }
            else 
                new_music_sheet.Append(music_sheet[i]);
        }
        if (temp_music_sheet.Length > 0)
            ProcessTempMusicSheet(ref temp_music_sheet, ref new_music_sheet, overpressure_first, music_sheet.Length - 1, max_overpressure, current_distance < minimum_distance);
        return new_music_sheet.ToString();
    }
    public static string MapGroups(string sheet_content, int offset = 1) //����������ƫ��һλ
    {
        static string CharAt(string input, int index)
        {
            if (index < 0 || index >= input.Length)
                return "";
            else
                return input[index].ToString();
        }
        string groups = "ZzXxCVvBbNnMAaSsDFfGgHhJQqWwERrTtYyU1!2@34$5%6^78*9(0IiOoKkL";
        string result = "";
        foreach (char ch in sheet_content)
        {
            char temp_ch = char.IsLetter(ch) && !"zxvbnasfghqwrty!@$%^*(iok".Contains(ch) ? char.ToUpper(ch) : ch;
            int index = groups.IndexOf(temp_ch);
            if (index != -1)
                result += CharAt(groups, index + offset * 12);
            else
                result += temp_ch.ToString();
        }
        return result;
    }
}

public class SpecialComparer : IComparer<char>
{
    private readonly string order;

    public SpecialComparer(string order)
    {
        this.order = order;
    }

    public int Compare(char x, char y)
    {
        // ���ĳ���ַ�����˳���ַ����У���ô��Ϊ����˳��ܸ�
        int indexX = order.IndexOf(x);
        int indexY = order.IndexOf(y);

        indexX = indexX == -1 ? int.MaxValue : indexX;
        indexY = indexY == -1 ? int.MaxValue : indexY;

        return indexX.CompareTo(indexY);
    }
}