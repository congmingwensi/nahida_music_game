using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public Sprite defaulImage;
    public Sprite perfectPressedImage;
    public Sprite quickPressedImage;
    public Sprite slowPressedImage;
    public GameObject effectPrefab;
    private Queue<IEnumerator> imageChangeQueue = new Queue<IEnumerator>();
    private bool isProcessingQueue = false;


    public KeyCode keyToPress;

    private SpriteRenderer theMR;
    // Start is called before the first frame update
    void Start()
    {
        theMR = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    private IEnumerator ProcessQueue()
    {
        isProcessingQueue = true;
        while (imageChangeQueue.Count > 0)
        {
            yield return StartCoroutine(imageChangeQueue.Dequeue());
        }
        isProcessingQueue = false;
    }
    private void CreateEffect(Sprite image)
    {
        //Debug.Log("Creating effect with image: " + image.name);
        GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
        //Debug.Log("Effect position: " + effect.transform.position + ", Sorting Layer: " + effect.GetComponent<SpriteRenderer>().sortingLayerName);
        EffectController effectController = effect.GetComponent<EffectController>();
        if (effectController != null)
        {
            effectController.StartEffect(image, 0.1f, 0.3f);
            //Debug.Log("Effect started");
        }
        else
        {
            Debug.LogError("EffectController not found on the instantiated effectPrefab.");
        }
    }

    public void ChangePpercevtImage()
    {
        CreateEffect(perfectPressedImage);
    }
    public void ChangePQuickImage()
    {
        CreateEffect(quickPressedImage);
    }
    public void ChangePSlowImage()
    {
        CreateEffect(slowPressedImage);
    }
}