using UnityEngine;
using TMPro;

public class JGhostsManager : MonoBehaviour
{
    public TMP_Text descriptionText;

    private string[] ghostNames = { "Dad", "Mom", "Child" };
    private string[] ghostDescriptions = {
        "Father of the Johnson family. Stern and protective in life, now twisted by the curse. He roams the east wing, guarding his old study.",
        "Mother of the Johnson family. She loved her garden and her children. The curse has made her hollow and wandering. Found near the kitchen.",
        "The youngest Johnson. Innocent and confused, not yet fully turned. Sometimes found near the nursery, clutching an old toy."
    };

    void Start()
    {
        ShowGhost(0);
    }

    public void ShowGhost(int index)
    {
        descriptionText.text = 
            "<b>" + ghostNames[index] + "</b>\n\n" + 
            ghostDescriptions[index];
    }
}