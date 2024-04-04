using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GamePresent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Sprite pressedImage;
    private Sprite defaulImage;
    private SpriteRenderer spriteRenderer;
    public Image present_content;
    private bool isActive = false;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaulImage = spriteRenderer.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        spriteRenderer.sprite = pressedImage; // Change to the hover sprite
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        spriteRenderer.sprite = defaulImage; // Change back to the original sprite
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        present_content.gameObject.SetActive(!isActive);
        isActive = !isActive;
    }
}
