using System;
using System.Linq;
using MLAPI;
using MLAPI.Spawning;
using UnityEngine;

namespace MenuScripts
{
    public class ConnectionScript : NetworkBehaviour
    {
        public GameObject ConListPrefab;
        public GameObject ConList;
        private GameObject _conListItem;

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }

        public void Connect()
        {
            switch (MainMenu.NetType)
            {
                case "server":
                    NetworkManager.Singleton.StartServer();
                    break;
                case "host":
                    NetworkManager.Singleton.StartHost();
                    break;
                case "client":
                    // to set up connection (is local machine by default)
                    // NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = "127.0.0.1";
                    // NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = 7777;
                
                    NetworkManager.Singleton.NetworkConfig.ConnectionData =
                        System.Text.Encoding.Default.GetBytes("password:" + MainMenu.EnteredPass);
                    NetworkManager.Singleton.StartClient();
                    break;
            }
        }

        private void ApprovalCheck(byte[] connectionData, ulong clientId,
            NetworkManager.ConnectionApprovedDelegate callback)
        {
            //parsing connectionData
            var cDataArray = System.Text.Encoding.Default.GetString(connectionData)
                .Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries)      // splits lines
                .Select(a => a.Split(':'));                               // splits var name from var        
            var approve = true;
            foreach (var operation in cDataArray)
            {
                switch (operation[0])   
                {
                    case "password":
                        approve &= operation[1] == MainMenu.EnteredPass;
                        Debug.Log(operation[1] + " == " + MainMenu.EnteredPass + " ? " + approve);
                        break;
                    case "steam_id":
                        break;
                    case "display_name":
                        break;
                }
            }

            // maybe decide this with operation loop
            ulong? prefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("Player");

            Debug.Log("hash = " + prefabHash);

            //If approve is true, the connection gets added. If it's false. The client gets disconnected
            callback(false, prefabHash, approve, new Vector3(0, 0, 0), Quaternion.identity);
        }
    
        public void Disconnect()
        {
            if (IsHost) 
            {
                NetworkManager.Singleton.StopHost();
            }
            else if (IsServer) 
            {
                NetworkManager.Singleton.StopServer();
            }
            else if (IsClient) 
            {
                _conListItem.GetComponent<NetworkObject>().Despawn(true);
                NetworkManager.Singleton.StopClient();
            }
       
    
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }

        private void OnClientConnect(ulong id)
        {
            if (IsClient)
            {
                _conListItem = Instantiate(ConListPrefab, ConList.transform);
                _conListItem.GetComponent<NetworkObject>().Spawn();
            }
            else if (IsHost)
                return;
            else if (IsServer)
                return;
        }

        private void OnClientDisconnect(ulong id)
        {
            if (IsHost)
                return;
            else if (IsServer)
                return;
            else if (IsClient)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }

    }
}