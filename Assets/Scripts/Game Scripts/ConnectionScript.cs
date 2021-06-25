using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MLAPI;
using MLAPI.Spawning;
using UnityEngine;

public class ConnectionScript : MonoBehaviour
{
    private bool _connectionSuccess;
    void Start()
    {
        switch (MainMenu.netType)
        {
            case "server":
                NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
                NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
                NetworkManager.Singleton.StartServer();
                break;
            case "host":
                NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
                NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
                NetworkManager.Singleton.StartHost();
                break;
            case "client":
                NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
                NetworkManager.Singleton.NetworkConfig.ConnectionData =
                    System.Text.Encoding.Default.GetBytes("password:" + MainMenu.enteredPass);
                NetworkManager.Singleton.StartClient();
                // some code somewhere to send back to menu if connection failure
                break;
        }
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId,
        NetworkManager.ConnectionApprovedDelegate callback)
    {
        //parsing connectionData
        var cDataString = System.Text.Encoding.Default.GetString(connectionData).Split(';')
            .Select(a => a.Split(':'));
        var approve = true;
        foreach (var operation in cDataString)
        {
            switch (operation[0])
            {
                case "password":
                    approve &= operation[1] == MainMenu.enteredPass;
                    Debug.Log(operation[1] + " == " + MainMenu.enteredPass + " ? " + approve);
                    break;
            }
        }
        // happens later?
        //bool createPlayerObject = true;

        // maybe decide this with operation loop
        ulong? prefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("Player");

        Debug.Log("hash = " + prefabHash);

        _connectionSuccess = approve;
        
        //If approve is true, the connection gets added. If it's false. The client gets disconnected
        callback(true, prefabHash, approve, new Vector3(0, 0, 0), Quaternion.identity);
    }
}