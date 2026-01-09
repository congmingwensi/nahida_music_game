using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows;

class SpectrogramSimplifier : MonoBehaviour
{
    private const string KEY_ORDER = "ZzXxCVvBbNnMAaSsDFfGgHhJQqWwERrTtYyU1!2@34$5%6^78*9(0IiOoKkL";
    
    private static readonly Dictionary<char, int> RankMap;
    private static readonly Dictionary<char, float> TimeVals = new Dictionary<char, float>
    {
        { '.', 0.5f },
        { '=', 1.0f },
        { '-', 2.0f },
        { '+', 4.0f }
    };

    static SpectrogramSimplifier()
    {
        RankMap = new Dictionary<char, int>();
        for (int i = 0; i < KEY_ORDER.Length; i++)
        {
            RankMap[KEY_ORDER[i]] = i;
        }
    }

    private class Token
    {
        public char Char { get; }
        public bool IsNote { get; }
        public bool IsTime { get; }
        public bool IsAuto { get; set; }

        public Token(char c)
        {
            Char = c;
            IsNote = RankMap.ContainsKey(c);
            IsTime = TimeVals.ContainsKey(c);
            IsAuto = false;
        }
    }

    public static string SimplifySpectrogram(string music_sheet, int minimum_distance = 8, int max_overpressure = 1, float overpressure_probability = 0.5f)
    {
        // Convert minimum_distance to float for time-based comparison
        float minDist = minimum_distance;
        int maxConcurrent = max_overpressure;

        // Parse sheet into tokens
        List<Token> tokens = new List<Token>();
        foreach (char c in music_sheet)
        {
            tokens.Add(new Token(c));
        }

        float lastClickTime = -minDist;
        float currentTime = 0.0f;

        int i = 0;
        while (i < tokens.Count)
        {
            // Collect notes in this beat (until we hit a time symbol)
            List<int> beatIndices = new List<int>();
            int j = i;
            bool hasTime = false;
            float duration = 0.0f;

            // Scan forward to find all notes before the next time symbol
            while (j < tokens.Count)
            {
                if (tokens[j].IsTime)
                {
                    hasTime = true;
                    duration = TimeVals[tokens[j].Char];
                    break;
                }
                if (tokens[j].IsNote)
                {
                    beatIndices.Add(j);
                }
                j++;
            }

            int nextStart = j + 1;

            // Handle case of pure irrelevant characters
            if (beatIndices.Count == 0 && !hasTime)
            {
                i++;
                continue;
            }

            // Handle pure wait (only time symbol, no notes)
            if (beatIndices.Count == 0)
            {
                currentTime += duration;
                i = nextStart;
                continue;
            }

            // --- Process this beat ---
            // A. Density check
            float dist = currentTime - lastClickTime;
            bool densityAuto = false;

            if (dist < minDist)
            {
                densityAuto = true;
                foreach (int idx in beatIndices)
                {
                    tokens[idx].IsAuto = true;
                }
            }

            // B. Multi-press check (only if not density_auto)
            if (!densityAuto)
            {
                // Sort by rank descending (high pitch first - higher index in KEY_ORDER)
                List<int> beatNotes = beatIndices
                    .OrderByDescending(idx => RankMap.ContainsKey(tokens[idx].Char) ? RankMap[tokens[idx].Char] : -1)
                    .ToList();

                bool hasClick = false;
                for (int rankIdx = 0; rankIdx < beatNotes.Count; rankIdx++)
                {
                    int tokenIdx = beatNotes[rankIdx];
                    if (rankIdx < maxConcurrent)
                    {
                        hasClick = true;
                    }
                    else
                    {
                        tokens[tokenIdx].IsAuto = true;
                    }
                }

                if (hasClick)
                {
                    lastClickTime = currentTime;
                }
            }

            currentTime += duration;
            i = nextStart;
        }

        // Reconstruct the result
        StringBuilder result = new StringBuilder();
        foreach (Token t in tokens)
        {
            if (t.IsAuto)
            {
                result.Append("~");
                result.Append(t.Char);
            }
            else
            {
                result.Append(t.Char);
            }
        }

        return result.ToString();
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
