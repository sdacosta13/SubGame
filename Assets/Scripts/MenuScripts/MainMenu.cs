using MLAPI;
using MLAPI.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuScripts
{
    public class MainMenu : NetworkBehaviour
    {
        public static string NetType = "host";
        public static string EnteredPass = "";
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
            NetType = "host";
        }
    
        public void JoinLobbyAsClient()
        {
            NetType = "client";
        }
    
        public void UpdatePassword(string text)
        {
            EnteredPass = text;
        }
    
        // ------------------ LOBBY MENU CODE -------------------------

        public void StartAsHost()
        {
            NetType = "host";
            NetworkSceneManager.SwitchScene("Game");
        }
    
        public void StartAsClient()
        {
            NetType = "client";
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
        }

    }
}
