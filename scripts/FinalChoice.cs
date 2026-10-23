using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalChoice : MonoBehaviour
{
    public Transform player;
    public float triggerDistance = 4f;

    int stage = 0;        // 0 waiting, 1 reward, 2 choice, 3 ending
    string ending = "";
    float baseY;

    void Start()
    {
        baseY = transform.position.y;
    }

    void Update()
    {
        // the core spins and floats like a prize
        transform.Rotate(0f, 50f * Time.deltaTime, 0f);
        Vector3 p = transform.position;
        p.y = baseY + Mathf.Sin(Time.time * 2f) * 0.15f;
        transform.position = p;

        if (stage != 0) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsUnlocked()) return;

        if (Vector3.Distance(player.position, transform.position) <= triggerDistance)
        {
            stage = 1;
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnGUI()
    {
        if (stage == 0) return;

        GUI.depth = -10;
        float w = Screen.width;
        float h = Screen.height;

        GUI.color = new Color(0f, 0f, 0.05f, 0.94f);
        GUI.DrawTexture(new Rect(0, 0, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle title = new GUIStyle(GUI.skin.label);
        title.fontSize = 40;
        title.alignment = TextAnchor.MiddleCenter;
        title.wordWrap = true;
        title.normal.textColor = new Color(0f, 0.94f, 1f);

        GUIStyle gold = new GUIStyle(title);
        gold.normal.textColor = new Color(1f, 0.8f, 0f);

        GUIStyle body = new GUIStyle(GUI.skin.label);
        body.fontSize = 24;
        body.alignment = TextAnchor.MiddleCenter;
        body.wordWrap = true;
        body.normal.textColor = Color.white;

        GUIStyle button = new GUIStyle(GUI.skin.button);
        button.fontSize = 24;
        button.wordWrap = true;

        if (stage == 1)
        {
            GUI.Label(new Rect(w * 0.1f, h * 0.06f, w * 0.8f, 70), "VAULT CORE REACHED", title);
            GUI.Label(new Rect(w * 0.1f, h * 0.16f, w * 0.8f, 70), "REWARD: MINDCORP MASTER KEY", gold);

            string all = "";
            foreach (string f in GameManager.Instance.fragments)
                all += f + "\n\n";

            GUI.Label(new Rect(w * 0.1f, h * 0.28f, w * 0.8f, h * 0.45f),
                "Your recovered memories:\n\n" + all, body);

            if (GUI.Button(new Rect(w * 0.38f, h * 0.78f, w * 0.24f, 80), "CONTINUE", button))
                stage = 2;
        }
        else if (stage == 2)
        {
            GUI.Label(new Rect(w * 0.1f, h * 0.10f, w * 0.8f, 80), "THE SIMULATION BREAKS", title);
            GUI.Label(new Rect(w * 0.1f, h * 0.22f, w * 0.8f, 130),
                "The Master Key can switch off MINDCORP's memory machine.\nYou can save yourself, or save the city.", body);

            if (GUI.Button(new Rect(w * 0.15f, h * 0.50f, w * 0.30f, 110),
                "RESTORE YOURSELF\n(get your memories back)", button))
            { ending = "A"; stage = 3; }

            if (GUI.Button(new Rect(w * 0.55f, h * 0.50f, w * 0.30f, 110),
                "FREE THE CITY\n(you lose your own memories)", button))
            { ending = "B"; stage = 3; }
        }
        else if (stage == 3)
        {
            if (ending == "A")
            {
                GUI.Label(new Rect(w * 0.1f, h * 0.15f, w * 0.8f, 80), "ENDING A: YOU REMEMBER", title);
                GUI.Label(new Rect(w * 0.1f, h * 0.30f, w * 0.8f, 160),
                    "ZERO gets every memory back.\nBut the city stays under MINDCORP, and nobody else ever knows.", body);
            }
            else
            {
                GUI.Label(new Rect(w * 0.1f, h * 0.15f, w * 0.8f, 80), "ENDING B: THE CITY IS FREE", title);
                GUI.Label(new Rect(w * 0.1f, h * 0.30f, w * 0.8f, 160),
                    "The truth reaches every screen in NEON-9.\nZERO's own memories are gone.", body);
                GUI.Label(new Rect(w * 0.1f, h * 0.48f, w * 0.8f, 80),
                    "THE CITY REMEMBERS. YOU DO NOT.", title);
            }

            if (GUI.Button(new Rect(w * 0.35f, h * 0.70f, w * 0.30f, 90), "PLAY AGAIN", button))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}