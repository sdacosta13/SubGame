using System.Collections.Generic;
using MLAPI;
using MLAPI.SceneManagement;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MenuScripts
{
    public class MainMenu : NetworkBehaviour
    {

        public static string NetType = "host";
        public static string EnteredPass = "";

        public Canvas[] menus;
        
        // ------------------ MAIN MENU CODE -------------------------

        public void OpenMainMenu()
        {
            foreach (var menu in menus)
            {
                menu.gameObject.SetActive(false);
                if (menu.name != "MainMenu") continue;
                menu.gameObject.SetActive(true);
            }
        }
        
        public void QuitGame()
        {
            Debug.Log("Quit");
            Application.Quit();
        }
    
        // ------------------ LOBBY MENU CODE -------------------------

        public void OpenLobbyMenuAsHost()
        {
            NetType = "host";
            foreach (var menu in menus)
            {
                menu.gameObject.SetActive(false);
                if (menu.name != "LobbyMenu") continue;
                menu.gameObject.SetActive(true);
                foreach (Transform child in menu.transform)
                {
                    if (child.gameObject.name == "StartButton")
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }
        }
    
        public void OpenLobbyMenuAsClient()
        {
            NetType = "client";
            foreach (var menu in menus)
            {
                menu.gameObject.SetActive(false);
                if (menu.name != "LobbyMenu") continue;
                menu.gameObject.SetActive(true);
                foreach (Transform child in menu.transform)
                {
                    if (child.gameObject.name == "StartButton")
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }
    
        // ------------------ PLAY MENU CODE -------------------------

        public void OpenPlayMenu()
        {
            foreach (var menu in menus)
            {
                menu.gameObject.SetActive(false);
                if (menu.name != "PlayMenu") continue;
                menu.gameObject.SetActive(true);
            }
        }

        public void UpdatePassword(string text)
        {
            EnteredPass = text;
        }

        // ------------------ SETTINGS MENU CODE -------------------------

        public void OpenSettingsMenu()
        {
            foreach (var menu in menus)
            {
                menu.gameObject.SetActive(false);
                if (menu.name != "SettingsMenu") continue;
                menu.gameObject.SetActive(true);
            }
        }
    }
}
