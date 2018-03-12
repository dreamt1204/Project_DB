using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchLobbyManager : Photon.MonoBehaviour
{
    GameObject startButtonObject;
    GameObject readyLabelObject;
    UILabel Label_TeamCount_Blue;
    UILabel Label_TeamCount_Red;

    #region PUN Auto Connect

    public byte Version = 1;
    private bool ConnectInUpdate = true;

    void TryConnectAndJoin()
    {
        if (ConnectInUpdate && !PhotonNetwork.connected)
        {
            ConnectInUpdate = false;
            PhotonNetwork.ConnectUsingSettings(Version + "." + SceneManagerHelper.ActiveSceneBuildIndex);
        }
    }

    public virtual void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public virtual void OnJoinedLobby()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public virtual void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 6 }, null);
    }

    public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        Debug.LogError("Cause: " + cause);
    }

    public void OnJoinedRoom()
    {
        Init();
    }

    #endregion

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = false;    // we join randomly. always. no need to join a lobby to get the list of rooms.

        startButtonObject = GameObject.Find("Button_Start");
        startButtonObject.SetActive(false);
        readyLabelObject = GameObject.Find("Label_Ready");
        readyLabelObject.SetActive(false);

        Label_TeamCount_Blue = GameObject.Find("Widget_TeamCount_Blue").transform.Find("Label_Count").GetComponent<UILabel>();
        Label_TeamCount_Red = GameObject.Find("Widget_TeamCount_Red").transform.Find("Label_Count").GetComponent<UILabel>();
    }

    void Init()
    {
        // Auto assign team
        PhotonNetwork.player.AutoAssignTeam();

        // Update game start button
        UpdateStartButton();
    }

    void UpdateStartButton()
    {
        if (PhotonNetwork.isMasterClient)
            startButtonObject.SetActive(true);
        else
            readyLabelObject.SetActive(true);
    }

    void Update()
    {
        // PUN Auto Connect
        TryConnectAndJoin();

        // Update Team Count
        UpdateTeamCount();
    }

    void UpdateTeamCount()
    {
        Label_TeamCount_Blue.text = TeamManager.PlayersPerTeam[Team.Blue].Count.ToString();
        Label_TeamCount_Red.text = TeamManager.PlayersPerTeam[Team.Red].Count.ToString();
    }
}
