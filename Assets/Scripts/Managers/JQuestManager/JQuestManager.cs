using UnityEngine;
using TMPro;

public class JQuestManager : MonoBehaviour
{
    [System.Serializable]
    public class Quest
    {
        public string questName;
        public bool isComplete;
        public TMP_Text questText;
    }

    public Quest[] quests;

    void Start()
    {
        UpdateQuestDisplay();
    }

    public void ToggleQuest(int index)
    {
        quests[index].isComplete = !quests[index].isComplete;
        UpdateQuestDisplay();
    }

    void UpdateQuestDisplay()
    {
        foreach (Quest q in quests)
        {
            string checkbox = q.isComplete ? "[X]" : "[ ]";
            q.questText.text = checkbox + " " + q.questName;
        }
    }
}
