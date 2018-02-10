using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum Team : byte { None, Blue, Red };

public class TeamManager : MonoBehaviour
{
    public static Dictionary<Team, List<PhotonPlayer>> PlayersPerTeam;

    public const string TeamPlayerProp = "team";


    #region Events by Unity and Photon

    public void Start()
    {
        PlayersPerTeam = new Dictionary<Team, List<PhotonPlayer>>();
        Array enumVals = Enum.GetValues(typeof(Team));
        foreach (var enumVal in enumVals)
        {
            PlayersPerTeam[(Team)enumVal] = new List<PhotonPlayer>();
        }
    }

    public void OnDisable()
    {
        PlayersPerTeam = new Dictionary<Team, List<PhotonPlayer>>();
    }

    public void OnJoinedRoom()
    {
        this.UpdateTeams();
    }

    public void OnLeftRoom()
    {
        Start();
    }

    public void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        this.UpdateTeams();
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        this.UpdateTeams();
    }

    public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        this.UpdateTeams();
    }

    #endregion


    public void UpdateTeams()
    {
        Array enumVals = Enum.GetValues(typeof(Team));
        foreach (var enumVal in enumVals)
        {
            PlayersPerTeam[(Team)enumVal].Clear();
        }

        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            PhotonPlayer player = PhotonNetwork.playerList[i];
            Team playerTeam = player.GetTeam();
            PlayersPerTeam[playerTeam].Add(player);
        }
    }
}

public static class TeamUTL
{
    public static Team GetTeam(this PhotonPlayer player)
    {
        object teamId;
        if (player.CustomProperties.TryGetValue(TeamManager.TeamPlayerProp, out teamId))
        {
            return (Team)teamId;
        }

        return Team.None;
    }

    public static string GetTeamString(this PhotonPlayer player)
    {
        string teamString = "None";

        if (player.GetTeam() == Team.Blue)
            teamString = "Blue";
        else if (player.GetTeam() == Team.Red)
            teamString = "Red";

        return teamString;
    }

    public static int GetTeamPos(this PhotonPlayer player)
    {
        Team team = player.GetTeam();
        UTL.TryCatchError(team == Team.None, "Player doesn't have team. Cannot get this player's team position.");

        return TeamManager.PlayersPerTeam[team].IndexOf(player);
    }

    public static void SetTeam(this PhotonPlayer player, Team team)
    {
        if (!PhotonNetwork.connectedAndReady)
        {
            Debug.LogWarning("JoinTeam was called in state: " + PhotonNetwork.connectionStateDetailed + ". Not connectedAndReady.");
            return;
        }

        Team currentTeam = player.GetTeam();
        if (currentTeam != team)
        {
            player.SetCustomProperties(new Hashtable() { { PunTeams.TeamPlayerProp, (byte)team } });
        }
    }

    public static void AutoAssignTeam(this PhotonPlayer player)
    {
        if (TeamManager.PlayersPerTeam[Team.Blue].Count <= TeamManager.PlayersPerTeam[Team.Red].Count)
            player.SetTeam(Team.Blue);
        else
            player.SetTeam(Team.Red);
    }

    public static bool IsTeamMate(this PhotonPlayer player, PhotonPlayer target)
    {
        return (player.GetTeam() == target.GetTeam());
    }

    public static bool IsCharacterTeamMate(Character target)
    {
        return true; // IsTeamMate(target.ownerPlayer);        
    }
}