using Server;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ServerController : MonoBehaviour 
{
    private ServerManager manager;
    public Text status;
    public Image statusImg;
    [FormerlySerializedAs("connected")] public Sprite started;
    [FormerlySerializedAs("disconnected")] public Sprite stopped;

    private void Start()
    {
        manager?.Stop();
        status.text = "Stopped";
        statusImg.sprite = stopped;
        manager = new ServerManager();
    }

    public void StartServer()
    {
        manager.Start();
        status.text = "Started";
        statusImg.sprite = started;
    }

    public void StopServer()
    {
        manager.Stop();
        status.text = "Stopped";
        statusImg.sprite = stopped;
    }
}