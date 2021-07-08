using System;
using System.Collections.Generic;
using System.Linq;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using UnityEngine;

namespace Menu
{
    public class ConnectionScript : NetworkBehaviour
    {
        public GameObject ConListPrefab;
        public GameObject ConList;

        // server/host maintains a dictionary of clientIds against a string of data to send to all clients every frame
        // string contains username: , ping: , etc.
        private Dictionary<ulong, ConData> _conListItems = new Dictionary<ulong, ConData>();

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
                    // set up ConData for host client - steamId etc
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
            callback(false, prefabHash, approve, Vector3.zero, Quaternion.identity);
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

        [ServerRpc]
        private void SendClientDataServerRpc(ulong clientId, string clientData)
        {
            var cData = new ConData(clientData);
            _conListItems[clientId] = cData;
            UpdateClientListClientRpc(GetAllConDataAsString());
        }

        [ClientRpc]
        private void UpdateClientListClientRpc(string allClientData)
        {
            // remake content

            // remove all content first
            for (var i = ConList.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            // then re-add
            foreach (var clientData in allClientData.Split(new[] {Environment.NewLine},
                StringSplitOptions.RemoveEmptyEntries))
            {
                var temp = new ConData(clientData);
                var go = Instantiate(ConListPrefab, ConList.transform);
            }
        }

        private void OnClientConnect(ulong id)
        {
            if (NetworkManager.Singleton.IsClient)
            {
                Debug.Log("IsClient");
                // maybe collect clientData at approval check then use here, tbd when steam integration
                SendClientDataServerRpc(id, "steam_id:1234;display_name:hello4321;");
            }

            if (NetworkManager.Singleton.IsHost)
                Debug.Log("IsHost");
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log("IsServer");
            }
        }

        private void OnClientDisconnect(ulong id)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                _conListItems.Remove(id);
                UpdateClientListClientRpc(GetAllConDataAsString());
            }
            else if (NetworkManager.Singleton.IsClient)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }

        // only ever call this on server
        private string GetAllConDataAsString()
        {
            return _conListItems.Aggregate("",
                (current, conData) => current + (conData.Key.ToString() + Environment.NewLine));
        }
    }

    public struct ConData
    {
        public int Ping { get; set; }
        public string SteamId { get; }
        public string DisplayName { get; set; }

        public ConData(string data) : this(data.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
        {
        }

        public ConData(IEnumerable<string> data)
        {
            // defaults
            Ping = 0;
            SteamId = "undefined";
            DisplayName = "default";
            foreach (var dataLine in data)
            {
                var dataSplit = dataLine.Split(':');
                switch (dataSplit[0])
                {
                    case "ping":
                        Ping = int.Parse(dataSplit[1]);
                        break;
                    case "steam_id":
                        SteamId = dataSplit[1];
                        break;
                    case "display_name":
                        DisplayName = dataSplit[1];
                        break;
                    default:
                        Debug.Log("ConData variable not implemented: " + dataSplit[0]);
                        break;
                }
            }
        }

        // update method for each new member var
        public override string ToString()
        {
            return "steam_id:" + SteamId + ";"
                   + "display_name:" + DisplayName + ";"
                   + "ping:" + Ping + ";";
        }
    }
}