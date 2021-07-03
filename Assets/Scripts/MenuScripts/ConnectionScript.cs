using System;
using System.Collections.Generic;
using System.Linq;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using UnityEngine;

namespace MenuScripts
{
    public class ConnectionScript : NetworkBehaviour
    {
        public GameObject ConListPrefab;
        public GameObject ConList;

        private Dictionary<ulong, GameObject> conListItems = new Dictionary<ulong, GameObject>();

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
                .Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries) // splits lines
                .Select(a => a.Split(':')); // splits var name from var        
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
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.StopHost();
            }
            else if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.StopServer();
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.StopClient();
            }


            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }

        [ClientRpc]
        void SetNetObjectParentClientRpc(ulong netObjId)
        {
            
        }

        private void OnClientConnect(ulong id)
        {
            if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("IsClient");
            }

            if (NetworkManager.Singleton.IsHost)
                Debug.Log("IsHost");
            if (NetworkManager.Singleton.IsServer)
            {
                var go = Instantiate(ConListPrefab, ConList.transform);
                go.GetComponent<NetworkObject>().SpawnWithOwnership(id);
                var netId = go.GetComponent<NetworkObject>().NetworkObjectId;
                // now RPC the clients to change the parent transform
                Debug.Log("IsServer");
            }
        }

        private void OnClientDisconnect(ulong id)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                conListItems[id].GetComponent<NetworkObject>().Despawn(true);
                conListItems.Remove(id);
            }
            else if (NetworkManager.Singleton.IsClient)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }
}