using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClientListEntry : MonoBehaviour
{
    public TextMeshProUGUI displayName;
    [SerializeField]
    private TextMeshProUGUI ping;

    public void SetPing(string pingVal)
    {
        ping.text = "Ping: " + pingVal;
    }
    public void SetPing(int pingVal)
    {
        ping.text = "Ping: " + pingVal;
    }
    
    public void SetPing(ulong pingVal)
    {
        ping.text = "Ping: " + pingVal;
    }
}
