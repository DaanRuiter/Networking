﻿using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    public static HostData[] hostData;
    public static bool isRefreshing;

    private const string REGISTERED_GAME_NAME = "MainserverName-0";
    private const int MAX_CLIENTS = 15;
    private const string IP = "127.0.0.1";
    private const int PORT = 23466;

    private static float refreshRequestLength = 3f;

    private void Start()
    {
        //MasterServer.ipAddress = IP;
        //MasterServer.port = PORT;
        //Network.natFacilitatorIP = IP;
        //Network.natFacilitatorPort = 50005;
    }

	public static void StartServer()
    {
        Network.InitializeServer(MAX_CLIENTS, 25002, false);
        MasterServer.RegisterHost(REGISTERED_GAME_NAME, "Master test server", "comment");
    }

    private void OnServerInitialized()
    {
        Debug.Log("Server has been initialized");
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(65, 65, 120, 35), "Start Server"))
        {
            StartServer();
            Invoke("RefreshAndJoin", 0.5f);
        }
    }

    private void OnMasterServerEvent(MasterServerEvent mEvent)
    {
        if(mEvent == MasterServerEvent.RegistrationSucceeded)
        {
            Debug.Log("Registration succesful!");
        }
    }

    public void RefreshAndJoin()
    {
        StartCoroutine(RefreshHostList(true));
    }

    public static IEnumerator RefreshHostList(bool joinLatest = false)
    {
        Debug.Log("Refreshing..");
        isRefreshing = true;

        MasterServer.RequestHostList(REGISTERED_GAME_NAME);

        float timeStarted = Time.time;
        float timeEnd = Time.time + refreshRequestLength;

        while(Time.time < timeEnd)
        {
            hostData = MasterServer.PollHostList();
            yield return new WaitForEndOfFrame();
        }

        if (hostData == null || hostData.Length == 0)
        {
            Debug.Log("No active servers has been found");
        }
        Debug.Log(hostData.Length + " servers found.");
        isRefreshing = false;

        if(joinLatest)
        {
            SceneManager.Instance.JoinGame(hostData[hostData.Length - 1]);
        }
    }

    public static void UnRegisterGame()
    {
        if (Network.isServer)
        {
            Network.Disconnect(300);
            MasterServer.UnregisterHost();
        }
        if (Network.isClient)
            Network.Disconnect(300);
    }

    private void OnApplicationQuit()
    {
        NetworkManager.UnRegisterGame();
    }
}
