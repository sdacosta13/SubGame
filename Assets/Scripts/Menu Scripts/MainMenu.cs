using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : NetworkBehaviour
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
    
    // ------------------ LOBBY MENU CODE -------------------------

    public void StartLobbyAsHost()
    {
        netType = "host";
    }
    
    public void JoinLobbyAsClient()
    {
        netType = "client";
    }
    
    public void UpdatePassword(string text)
    {
        enteredPass = text;
    }
    
    // ------------------ LOBBY MENU CODE -------------------------

    public void StartAsHost()
    {
        netType = "host";
        NetworkSceneManager.SwitchScene("Game");
    }
    
    public void StartAsClient()
    {
        netType = "client";
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }
    
    
}
