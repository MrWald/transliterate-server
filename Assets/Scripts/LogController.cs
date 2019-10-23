using Server;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LogController : MonoBehaviour
{
    private Text text;
    [FormerlySerializedAs("Limit")] public int limit;
    private int chars;
        
    // Use this for initialization
    private void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    private void Update () 
    {
        if (ConsoleMessenger.LogStack.Count <= 0) return;
        foreach (var line in ConsoleMessenger.LogStack)
        {
            chars += line.Length;
            while (chars >= limit)
            {
                var substr = text.text.Substring(text.text.LastIndexOf('\n') + 1);
                chars -= substr.Length;
                text.text = text.text.Substring(0, text.text.LastIndexOf('\n'));
            }
            text.text = line + (chars == 1 ? "" : "\n") + text.text;
        }

        ConsoleMessenger.LogStack.Clear();
    }

    public void Clear()
    {
        chars = 0;
        text.text = "";
    }

    public void ChangeStatus(int type)
    {
        ConsoleMessenger.ShowPrefixes[type] = !ConsoleMessenger.ShowPrefixes[type];
    }
}