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

        private int _frame = 0;

        // server/host maintains a dictionary of clientIds against a string of data to send to all clients every frame
        // string contains username: , ping: , etc.
        private readonly Dictionary<ulong, ConData> _conListData = new Dictionary<ulong, ConData>();

        // dictionary so that ConList items can be update without destruction maintained by all clients
        private readonly Dictionary<ulong, GameObject> _conListItems = new Dictionary<ulong, GameObject>();

        private void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

            // stuff to do when the server has finished starting
            NetworkManager.Singleton.OnServerStarted += OnServerStart;

            //NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }

        private void Update()
        {
            _frame++;

            if (_frame >= 30)
            {
                if (IsServer)
                {
                    UpdateClientsPing();
                }

                _frame = 0;
            }
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
                .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries) // splits lines
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
        private void SendClientDataServerRpc(string clientData)
        {
            Debug.Log("Received new clients data");
            var cData = new ConData(clientData);
            _conListData[cData.ClientId] = cData;
            var allCondDataStr = GetAllConDataAsString();
            Debug.Log("Client data after aggregate: " + allCondDataStr);
            UpdateClientListClientRpc(allCondDataStr);
        }

        [ClientRpc]
        private void UpdateClientListClientRpc(string allClientData)
        {
            // this is also received by the host client
            Debug.Log("Remaking client data list");
            _conListData.Clear();

            // remove all
            foreach (Transform child in ConList.transform)
            {
                Destroy(child.gameObject);
            }

            // then re-add
            foreach (var clientData in allClientData.Split(new[] {Environment.NewLine},
                StringSplitOptions.RemoveEmptyEntries))
            {
                var temp = new ConData(clientData);
                _conListData[temp.ClientId] = temp;
                var go = Instantiate(ConListPrefab, ConList.transform);
                _conListItems[temp.ClientId] = go;
                go.GetComponent<ClientListEntry>().displayName.text = temp.DisplayName;
                go.GetComponent<ClientListEntry>().UpdatePing(temp.Ping);
                // set some values in list
            }
        }

        private void UpdateClientsPing()
        {
            var clientPingStr = "";
            foreach (var client in NetworkManager.ConnectedClients)
                clientPingStr += client.Key + ":" +
                                 NetworkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(client.Key) + ";";

            UpdateClientPingDisplayClientRpc(clientPingStr);
        }

        [ClientRpc]
        private void UpdateClientPingDisplayClientRpc(string clientPingStr)
        {
            // parse string
            var clientList = clientPingStr.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries));

            foreach (var client in clientList)
            {
                _conListData[ulong.Parse(client[0])].Ping = int.Parse(client[1]);
                _conListItems[ulong.Parse(client[0])].GetComponent<ClientListEntry>().UpdatePing(client[1]);
            }
        }

        private void OnServerStart()
        {
            if (IsHost)
            {
                _conListData[NetworkManager.LocalClientId] =
                    new ConData(NetworkManager.LocalClientId, "steam121", "ciaran");
                UpdateClientListClientRpc(GetAllConDataAsString());
            }
        }

        private void OnClientConnect(ulong id)
        {
            if (IsClient)
            {
                Debug.Log("IsClient connects with ID " + id);
                // maybe collect clientData at approval check then use here, tbd when steam integration
                SendClientDataServerRpc("steam_id:1234;display_name:hello4321;client_id:" + id + ";");
            }

            if (IsHost)
                Debug.Log("IsHost");
            if (IsServer)
            {
                Debug.Log("IsServer");
            }
        }

        // this is called when the server is closed, on the client side,
        // not when a client disconnects from the server for some dumb ass reason
        // so there is no way to tell when a client disconnects?
        // dumb as fuck, hurts my brain
        // so instead check ConnectedClients every frame and compare to _conListItems on server
        // nvm it works now idk why god help me
        private void OnClientDisconnect(ulong id)
        {
            if (IsServer || IsHost)
            {
                _conListData.Remove(id);
                Debug.Log("Client " + id + " disconnected from server");
                UpdateClientListClientRpc(GetAllConDataAsString());
            }
            else if (IsClient)
                Debug.Log("Server closed?");
        }

        // only ever call this on server
        private string GetAllConDataAsString()
        {
            return _conListData.Aggregate("",
                (current, conData) => current + (conData.Value + Environment.NewLine));
        }
    }

    public class ConData
    {
        public ulong ClientId { get; private set; }
        public int Ping { get; set; }
        public string SteamId { get; private set; }
        public string DisplayName { get; set; }

        public ConData(ulong clientId, string steamId, string displayName)
        {
            ClientId = clientId;
            SteamId = steamId;
            DisplayName = displayName;
        }

        public ConData(string data) : this(data.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
        {
            Debug.Log(data);
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