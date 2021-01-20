using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField]
    InputField ipInputField;
    [SerializeField]
    InputField portInputField;

    public Camera startCamera;
    public GameObject Minimap;

    public GameManager gameManager;

    public void OpenServer()
    {
        if(portInputField.text.Length > 0)
            networkPort = int.Parse(portInputField.text);

        startCamera.transform.GetChild(0).gameObject.SetActive(false);
        StartServer();
    }

    public void OpenHost()
    {
        if (portInputField.text.Length > 0)
            networkPort = int.Parse(portInputField.text);

        startCamera.gameObject.SetActive(false);
        Minimap.SetActive(true);
        StartHost();
    }

    public void ConnectClientToServer()
    {
        if (ipInputField.text.Length > 0)
            networkAddress = ipInputField.text;

        if (portInputField.text.Length > 0)
            networkPort = int.Parse(portInputField.text);

        startCamera.gameObject.SetActive(false);
        Minimap.SetActive(true);
        StartClient();
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject playerObject;

        playerObject = Instantiate(playerPrefab, GetStartPosition().position, Quaternion.identity);

        NetworkServer.AddPlayerForConnection(conn, playerObject, playerControllerId);

        gameManager.AddPlayer(playerObject.GetComponent<PlayerMovement>());
    }
}