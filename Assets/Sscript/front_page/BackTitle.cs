using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackTitle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Sprite pressedImage;
    public Sprite defaulImage;
    private Image buttonImage;
    // Start is called before the first frame update
    void Start()
    {
        buttonImage = GetComponent<Image>(); // Get the Image component
        buttonImage.sprite = defaulImage;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.sprite = pressedImage; // Change to the hover sprite
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.sprite = defaulImage; // Change back to the original sprite
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        GameMenager.instance.managerNote.ClearAll();
        SceneManager.LoadScene("front_page");
    }
}
