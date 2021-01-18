using UnityEngine.Networking;

public class Item : NetworkBehaviour
{
    [ClientRpc]
    void RpcOffActive()
    {
        gameObject.SetActive(false);
    }

    // Server
    public void OffActive()
    {
        gameObject.SetActive(false);
        RpcOffActive();
    }
}