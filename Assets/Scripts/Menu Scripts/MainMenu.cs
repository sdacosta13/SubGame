using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static string netType = "host";
    public static string enteredPass = "";
    //public static string requiredPass = "";
    // ------------------ MAIN MENU CODE -------------------------

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
    
    // ------------------ PLAY MENU CODE -------------------------

    public void StartAsHost()
    {
        netType = "host";
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }
    
    public void StartAsClient()
    {
        netType = "client";
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }
    
    public void UpdatePass(string text)
    {
        enteredPass = text;
    }
}
