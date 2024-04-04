using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] sprites; //0:nomal 1:annoyed 2:happy 3:seductive 4:culmination

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void ChangeImage(int index)
    {
        if (index >= 0 && index < sprites.Length)
        {
            targetImage.sprite = sprites[index]; // Set the image to the sprite at the specified index
        }
        else
        {
            Debug.LogWarning("Index out of range: " + index);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
