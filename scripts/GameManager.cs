using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject vaultDoor;
    public int totalTerminals = 3;
    public float doorOpenTime = 4f;   // seconds the door takes to rise
    public float doorRise = 6f;       // how far up it rises

    public List<string> fragments = new List<string>();

    int solvedCount = 0;
    bool doorOpen = false;
    string banner = "";
    float bannerUntil = 0f;

    void Awake()
    {
        Instance = this;
        fragments = new List<string>();
    }

    public int NextTerminal() { return solvedCount + 1; }
    public bool IsNext(int n) { return n == solvedCount + 1; }
    public bool IsUnlocked() { return doorOpen; }

    public void ShowBanner(string msg, float seconds)
    {
        banner = msg;
        bannerUntil = Time.time + seconds;
    }

    public void TerminalSolved(int n, string fragment)
    {
        solvedCount++;
        if (fragment != "") fragments.Add(fragment);

        if (solvedCount >= totalTerminals)
            StartCoroutine(OpenVault());
        else
            ShowBanner("MEMORY " + solvedCount + "/" + totalTerminals +
                       " RECOVERED. Next: Terminal " + (solvedCount + 1), 4f);
    }

    IEnumerator OpenVault()
    {
        ShowBanner("ALL MEMORIES RECOVERED... THE VAULT DOOR IS OPENING", doorOpenTime + 1.5f);
        yield return new WaitForSeconds(1.5f);

        if (vaultDoor != null)
        {
            Vector3 start = vaultDoor.transform.position;
            Vector3 end = start + Vector3.up * doorRise;
            float t = 0f;
            while (t < doorOpenTime)
            {
                t += Time.deltaTime;
                vaultDoor.transform.position = Vector3.Lerp(start, end, t / doorOpenTime);
                yield return null;
            }
            vaultDoor.SetActive(false);
        }

        doorOpen = true;
        ShowBanner("VAULT OPEN. Walk to the glowing core.", 5f);
    }

    void OnGUI()
    {
        GUIStyle hud = new GUIStyle(GUI.skin.label);
        hud.fontSize = 26;
        hud.normal.textColor = new Color(0f, 0.94f, 1f);

        GUI.Label(new Rect(20, 15, 700, 40), "MEMORIES: " + solvedCount + "/" + totalTerminals, hud);

        string objective;
        if (solvedCount < totalTerminals)
            objective = "OBJECTIVE: Hack Terminal " + (solvedCount + 1);
        else if (!doorOpen)
            objective = "OBJECTIVE: The vault door is opening...";
        else
            objective = "OBJECTIVE: Reach the glowing vault core";
        GUI.Label(new Rect(20, 50, 1000, 40), objective, hud);

        if (Time.time < bannerUntil)
        {
            GUIStyle b = new GUIStyle(GUI.skin.label);
            b.fontSize = 30;
            b.alignment = TextAnchor.MiddleCenter;
            b.wordWrap = true;
            b.normal.textColor = new Color(1f, 0.17f, 0.84f);

            Rect r = new Rect(Screen.width * 0.15f, Screen.height * 0.78f, Screen.width * 0.7f, 100);
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(r, banner, b);
        }
    }
}