/*
    This script allows for the dots signaling interactable objects to go away smoothly
*/
using UnityEngine;
using UnityEngine.UI;

public class FaderUI : MonoBehaviour
{
    public float fadeSpeed = 6f;
    public bool visible;
    // the image component where the dot image is
    public Image img;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    void Update()
    {
        if (img == null) return;

        Color c = img.color;
        // smoothly transition between visibility modes
        float targetAlpha = visible ? 1f : 0f;
        c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * fadeSpeed);

        img.color = c;
    }

    public void Show()
    {
        visible = true;
    }
    public void Hide()
    {
        visible = false;
    }
}
