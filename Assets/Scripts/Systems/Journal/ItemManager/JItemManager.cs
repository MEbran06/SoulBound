using UnityEngine;
using TMPro;

public class JItemManager : MonoBehaviour
{
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;

    private string[] itemNames = { "Key", "Hammer", "Family Photo" };
    private string[] itemDescriptions = {
        "An old iron key found near the fireplace. It looks like it could open the front gate of the manor.",
        "A heavy sledgehammer from the basement. Useful for breaking through weakened walls.",
        "A faded photograph of the Johnson family. They look happy. Something feels important about this."
    };

    void OnEnable()
    {
        ShowItem(0);
    }

    public void ShowItem(int index)
    {
        itemNameText.text = itemNames[index];
        itemDescriptionText.text = itemDescriptions[index];
    }
}
