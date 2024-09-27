using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Sprite pressedImage;
    private Sprite defaulImage;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    public Image transitionImage;
    public float transitionDuration = 2f;
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
        StartCoroutine(PlayTransition());
    }
    IEnumerator PlayTransition()
    {
        // Make the image visible
        transitionImage.gameObject.SetActive(true);

        // Wait for the specified duration
        yield return new WaitForSeconds(transitionDuration);

        // Load the next scene or hide the image
        if (!string.IsNullOrEmpty("music_scene"))
        {

            SceneManager.LoadScene("music_scene");
        }
        else
        {
            transitionImage.gameObject.SetActive(false);
        }
    }
}
