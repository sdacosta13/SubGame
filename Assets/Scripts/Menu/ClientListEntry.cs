using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClientListEntry : MonoBehaviour
{
    public TextMeshProUGUI displayName;
    [SerializeField]
    private TextMeshProUGUI ping;

    public void UpdatePing(string pingVal)
    {
        ping.text = "Ping: " + pingVal;
    }
    public void UpdatePing(int pingVal)
    {
        ping.text = "Ping: " + pingVal;
    }
}
