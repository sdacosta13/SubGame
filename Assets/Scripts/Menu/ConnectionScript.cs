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
        private readonly Dictionary<ulong, ConData> _conListItems = new Dictionary<ulong, ConData>();

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

            //NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }

        public void Connect(string netType)
        {
            Debug.Log("Trying to start with " + netType);
            switch (netType)
            {
                case "server":
                    Debug.Log("server");
                    NetworkManager.Singleton.StartServer();
                    break;
                case "host":
                    Debug.Log("host");
                    NetworkManager.Singleton.StartHost();
                    // set up ConData for host client - steamId etc
                    break;
                case "client":
                    Debug.Log("client");
                    // to set up connection (is local machine by default)
                    // NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = "127.0.0.1";
                    // NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = 7777;

                    // NetworkManager.Singleton.NetworkConfig.ConnectionData =
                    //     System.Text.Encoding.Default.GetBytes("password:" + MainMenu.EnteredPass);
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
            if (IsHost)
            {
                NetworkManager.StopHost();
            }
            else if (IsServer)
            {
                NetworkManager.StopServer();
            }
            else if (IsClient)
            {
                NetworkManager.StopClient();
            }


            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }

        [ServerRpc]
        private void SendClientDataServerRpc(ulong clientId, string clientData)
        {
            var cData = new ConData(clientData);
            //_conListItems[clientId] = cData;
            UpdateClientListClientRpc(GetAllConDataAsString());
        }

        [ClientRpc]
        private void UpdateClientListClientRpc(string allClientData)
        {
            Debug.Log("Remaking client data list");
            //RemakeClientList(allClientData);
        }

        private void RemakeClientList(string allClientData)
        {
            _conListItems.Clear();
            
            // remove all
            for (var i = ConList.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            // then re-add
            foreach (var clientData in allClientData.Split(new[] {Environment.NewLine},
                StringSplitOptions.RemoveEmptyEntries))
            {
                Debug.Log("Attempting to create new con data");
                var temp = new ConData(clientData);
                _conListItems[temp.ClientId] = temp;
                var go = Instantiate(ConListPrefab, ConList.transform);
                // set some values in list
            }
        }
        
        private void RemakeHostList(string allClientData)
        {
            // remove all
            for (var i = ConList.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            // then re-add
            foreach (var cData in _conListItems)
            {
                Debug.Log("Attempting to create new con data");
                var data = cData.Value;
                var go = Instantiate(ConListPrefab, ConList.transform);
                // set some values in list
            }
        }

        private void OnClientConnect(ulong id)
        {
            if (IsClient)
            {
                Debug.Log("IsClient");
                // maybe collect clientData at approval check then use here, tbd when steam integration
                //SendClientDataServerRpc(id, "steam_id:1234;display_name:hello4321;");
            }

            if (IsHost)
                Debug.Log("IsHost");
            if (IsServer)
            {
                Debug.Log("IsServer");
            }
        }

        private void OnClientDisconnect(ulong id)
        {
            if (IsServer)
            {
                _conListItems.Remove(id);
                //UpdateClientListClientRpc(GetAllConDataAsString());
            }
            else if (IsClient)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }

        // only ever call this on server
        private string GetAllConDataAsString()
        {
            return _conListItems.Aggregate("",
                (current, conData) => current + (conData.Key + Environment.NewLine));
        }
    }

    public struct ConData
    {
        public ulong ClientId { get; private set; }
        public int Ping { get; set; }
        public string SteamId { get; private set; }
        public string DisplayName { get; set; }

        public ConData(string data) : this(data.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
        {
        }

        public ConData(IEnumerable<string> data)
        {
            // defaults
            ClientId = 0;
            Ping = 0;
            SteamId = "undefined";
            DisplayName = "default";
            foreach (var dataLine in data)
            {
                var dataSplit = dataLine.Split(':');
                switch (dataSplit[0])
                {
                    case "client_id":
                        ClientId = ulong.Parse(dataSplit[1]);
                        break;
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
                   + "ping:" + Ping + ";"
                   + "client_id:" + ClientId + ";";
        }
    }
}