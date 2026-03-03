using UnityEngine;
using UnityEngine.InputSystem;


public class JournalManager : MonoBehaviour
{
    public GameObject journalPanel;
    public GameObject storyPage, questsPage, ghostsPage, itemsPage;

    private bool isOpen = false;

    void Start()
    {
        journalPanel.SetActive(false);
    
        // Hide all pages on start
        storyPage.SetActive(false);
        questsPage.SetActive(false);
        ghostsPage.SetActive(false);
        itemsPage.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
            journalPanel.SetActive(isOpen);
            Time.timeScale = isOpen ? 0f : 1f;
            if (isOpen) ShowPage(storyPage);
        }
    }

    public void ShowPage(GameObject pageToShow)
    {
        storyPage.SetActive(false);
        questsPage.SetActive(false);
        ghostsPage.SetActive(false);
        itemsPage.SetActive(false);
        pageToShow.SetActive(true);
    }

    public void CloseJournal()
    {
        isOpen = false;
        journalPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
