using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTool : MonoBehaviour
{
    public bool isDebugging;

    void OnGUI()
    {
        if (!isDebugging)
            return;

        GUI.Label(new Rect(0, 0, 100, 100), "Ping: " + PhotonNetwork.GetPing());
    }
}
