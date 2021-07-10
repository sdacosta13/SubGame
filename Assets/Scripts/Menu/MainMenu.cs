using MLAPI;
using UnityEngine;

namespace Menu
{
    public class MainMenu : MonoBehaviour
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
            NetType = "server";
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
