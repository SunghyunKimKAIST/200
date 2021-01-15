using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    bool first = true;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player;

        if (first)
        {
            first = false;

            player = Instantiate(NetworkManager.singleton.spawnPrefabs[0], new Vector3(0, -3.3f, 0), Quaternion.identity);
        }
        else
        {
            player = Instantiate(NetworkManager.singleton.spawnPrefabs[1], new Vector3(0, 3.3f, 0), Quaternion.identity);
        }

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}