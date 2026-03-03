using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenU: MonoBehaviour
{

    public void playGame()
    {
        SceneManager.LoadScene("Main");
    }
    
    public void quitGame()
    {
        Application.Quit();
        // for testing purposes
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
