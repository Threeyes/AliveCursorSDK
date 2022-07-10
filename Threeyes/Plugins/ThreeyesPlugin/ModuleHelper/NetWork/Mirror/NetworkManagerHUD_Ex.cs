// vis2k: GUILayout instead of spacey += ...; removed Update hotkeys to avoid
// confusion if someone accidentally presses one.
using System.Linq;
using System.Net;
using UnityEngine;
using System.Collections.Generic;
#if MIRROR
using Mirror;
#endif

/// <summary>Shows NetworkManager controls in a GUI at runtime.
/// PS:
///     1.让UI支持动态缩放
/// Ref：NetworkManagerHUD
/// </summary>
[DisallowMultipleComponent]
#if MIRROR
//[AddComponentMenu("Network/NetworkManagerHUD")]
[RequireComponent(typeof(NetworkManager))]
#endif
public class NetworkManagerHUD_Ex : MonoBehaviour
{
    public int offsetX;
    public int offsetY;
    public Vector2 displayAreaSize = new Vector2(215, 9999);
    public bool isExpandHeight = false;//设置为true，适用于需要自行扩充的场景
    public bool isFadeOnConnected = false;//ToUse:连接后透明度减少
    GUILayoutOption gUILayoutOptionExpandHeight => GUILayout.ExpandHeight(isExpandHeight);

#if MIRROR
    protected NetworkManager manager;
    void Awake()
    {
        manager = GetComponent<NetworkManager>();
    }
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, displayAreaSize.x, displayAreaSize.y));
        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        // client ready
        if (NetworkClient.isConnected && !NetworkClient.ready)
        {
            if (DrawButton("Client Ready"))
            {
                NetworkClient.Ready();
                if (NetworkClient.localPlayer == null)
                {
                    NetworkClient.AddPlayer();
                }
            }
        }

        StopButtons();

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (!NetworkClient.active)
        {
            // Server + Client
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (DrawButton("Host (Server + Client)"))
                {
                    manager.StartHost();
                }
            }

            // Client + IP
            GUILayout.BeginHorizontal();
            if (DrawButton("Client"))
            {
                manager.StartClient();
                OnClickClient();
            }
            // This updates networkAddress every frame from the TextField
            manager.networkAddress = GUILayout.TextField(manager.networkAddress, gUILayoutOptionExpandHeight);
            GUILayout.EndHorizontal();

            // Server Only
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // cant be a server in webgl build
                GUILayout.Box("(  WebGL cannot be server  )");
            }
            else
            {
                if (DrawButton("Server Only")) manager.StartServer();
            }
        }
        else
        {
            // Connecting
            DrawLabel($"Connecting to {manager.networkAddress}..");
            if (DrawButton("Cancel Connection Attempt"))
            {
                manager.StopClient();
            }
        }
    }

    protected virtual void OnClickClient()
    {

    }

    void StatusLabels()
    {
        // host mode
        // display separately because this always confused people:
        //   Server: ...
        //   Client: ...
        if (NetworkServer.active && NetworkClient.active)
        {
            DrawLabel($"<b>Host ({GetLocalIPv4()})</b>: running via {Transport.activeTransport}");
        }
        // server only
        else if (NetworkServer.active)
        {
            DrawLabel($"<b>Server ({GetLocalIPv4()})</b>: running via {Transport.activeTransport}");
        }
        // client only
        else if (NetworkClient.isConnected)
        {
            DrawLabel($"<b>Client ({GetLocalIPv4()})</b>: connected to {manager.networkAddress} via {Transport.activeTransport}");
        }
    }

    void StopButtons()
    {
        // stop host if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            if (DrawButton("Stop Host"))
            {
                manager.StopHost();
            }
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            if (DrawButton("Stop Client"))
            {
                manager.StopClient();
            }
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            if (DrawButton("Stop Server"))
            {
                manager.StopServer();
            }
        }
    }

    public string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
            .AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            .ToString();
    }

    void DrawLabel(string text, params GUILayoutOption[] options)
    {
        List<GUILayoutOption> listOption = new List<GUILayoutOption> { gUILayoutOptionExpandHeight };
        listOption.AddRange(options);
        GUILayout.Label(text, listOption.ToArray());
    }

    bool DrawButton(string text, params GUILayoutOption[] options)
    {
        GUILayoutOption gUILayoutOptionExpandHeight = GUILayout.ExpandHeight(isExpandHeight);
        List<GUILayoutOption> listOption = new List<GUILayoutOption> { gUILayoutOptionExpandHeight };
        listOption.AddRange(options);
        return GUILayout.Button(text, listOption.ToArray());
    }

#endif
}
