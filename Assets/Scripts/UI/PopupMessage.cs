using UnityEngine;
using TMPro;
using System.Collections;

public class PopupMessage : MonoBehaviour
{
    public static PopupMessage Instance;

    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text messageText;
    [SerializeField] float duration = 2f;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        StopAllCoroutines();
        StartCoroutine(Show(message));
    }

    IEnumerator Show(string message)
    {
        messageText.text = message;
        panel.SetActive(true);

        yield return new WaitForSeconds(duration);

        panel.SetActive(false);
    }
}