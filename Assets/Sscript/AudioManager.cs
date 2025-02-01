using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class KeyNotePlayer : MonoBehaviour
{
    private List<AudioSource> audioSources = new List<AudioSource>();
    private int poolSize = 200;
    private int currentIndex = 0;

    public KeyNotePlayer(GameObject parentObject)
    {
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = parentObject.AddComponent<AudioSource>();
            audioSources.Add(source);
        }
    }


    public void PlayNoteSound(char noteKey)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager.Instance is null!");
            return;
        }

        if (AudioManager.Instance.audioClips == null)
        {
            Debug.LogError("AudioManager.Instance.audioClips is null!");
            return;
        }

        if (AudioManager.Instance.audioClips.ContainsKey(noteKey))
        {
            AudioClip clip = AudioManager.Instance.audioClips[noteKey];
            AudioSource source = GetNextAudioSource();
            source.PlayOneShot(clip);
            //Debug.Log("播放音符: " + clip.name);
        }
        else
        {
            Debug.LogError($"音符 {noteKey} 的音频未找到！");
        }
    }

    private AudioSource GetNextAudioSource()
    {
        AudioSource source = audioSources[currentIndex];
        currentIndex = (currentIndex + 1) % poolSize;
        return source;
    }
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public Dictionary<char, AudioClip> audioClips = new Dictionary<char, AudioClip>();

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            StartCoroutine(PreloadAudioClips());
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 如果需要在特定场景销毁 AudioManager，可以在这里处理
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "front_page")
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator PreloadAudioClips()
    {
        while (GameMenager.instance == null || GameMenager.instance.noteAudio == null)
        {
            yield return null;
        }
        string keys = "ZzXxCVvBbNnMAaSsDFfGgHhJQqWwERrTtYyU1!2@34$5%6^78*9(0IiOoKkL";
        foreach (char key in keys)
        {
            if (GameMenager.instance.noteAudio == null)
                Debug.Log("GameMenager.insrance.noteAudio==null");
            string audioName = GameMenager.instance.noteAudio[key];
            string path = Path.Combine(Application.streamingAssetsPath, "SheetMusic/audio/" + audioName + ".mp3");
            yield return StartCoroutine(LoadAudioClip(key, path));
        }
    }

    private IEnumerator LoadAudioClip(char key, string path)
    {
        string url = "file://" + path;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading audio: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                clip.name = Path.GetFileNameWithoutExtension(path);
                audioClips[key] = clip;
                //Debug.Log($"音符 {key} 的音频已加载并存储。");
            }
        }
    }
}

