using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    
    public Sprite defaulImage;
    public Sprite perfectPressedImage;
    public Sprite quickPressedImage;
    public Sprite slowPressedImage;

    public KeyCode keyToPress;

    private SpriteRenderer theMR;
    public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        theMR = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            audioSource.Play();
            //theMR.sprite = pressedImage;
        }
        if (Input.GetKeyUp(keyToPress))
        {
            theMR.sprite = defaulImage;
        }
    }
    public void ChangePpercevtImage()
    {
        theMR.sprite = perfectPressedImage;
    }
    public void ChangePQuickImage()
    {
        theMR.sprite = quickPressedImage;
    }
    public void ChangePSlowImage()
    {
        theMR.sprite = slowPressedImage;
    }
}
