using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    public GameManager gameManager;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject playerObject;

        playerObject = Instantiate(playerPrefab, GetStartPosition().position, Quaternion.identity);

        NetworkServer.AddPlayerForConnection(conn, playerObject, playerControllerId);

        gameManager.AddPlayer(playerObject.GetComponent<PlayerMovement>());
    }
}