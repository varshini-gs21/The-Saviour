using UnityEngine;
using UnityEngine.InputSystem;

public class Terminal : MonoBehaviour
{
    public Transform player;
    public int terminalNumber = 1;       // 1, 2 or 3 = the order
    public float interactDistance = 3f;

    [TextArea] public string question = "";
    [TextArea] public string hint = "";
    public string answers = "";          // several accepted answers: separate with |
    [TextArea] public string memoryText = "";

    public Color lockedColor = new Color(1f, 0.1f, 0.2f);
    public Color activeColor = new Color(0f, 0.94f, 1f);
    public Color solvedColor = new Color(0f, 1f, 0.4f);

    static bool puzzleOpen = false;

    bool solved = false;
    bool near = false;
    bool open = false;
    bool showHint = false;
    string input = "";
    string feedback = "";
    float wrongUntil = 0f;
    float openedAt = 0f;
    int state = -1;
    Renderer rend;

    GUIStyle titleStyle, bodyStyle, fieldStyle, buttonStyle, promptStyle;

    void Start()
    {
        rend = GetComponent<Renderer>();
        puzzleOpen = false;
        FillDefaults();
    }

    void FillDefaults()
    {
        if (question != "") return;

        if (terminalNumber == 1)
        {
            question = "Memory chips follow a pattern.\nFind the next number:\n\n2,  4,  8,  16,  ?";
            hint = "Each number is double the one before it.";
            answers = "32";
            memoryText = "FRAGMENT 1: A crowded street. People laughing without fear.";
        }
        else if (terminalNumber == 2)
        {
            question = "I have no voice, yet I hold your past.\nTake me, and you take who you are.\n\nWhat am I?";
            hint = "MINDCORP buys and sells me. 6 letters.";
            answers = "memory|memories|a memory";
            memoryText = "FRAGMENT 2: A voice whispers 'Remember us.' MINDCORP is deleting the memories of freedom.";
        }
        else
        {
            question = "Decode the final password.\nEvery letter was moved ONE step forward in the alphabet:\n\nTBWJPS";
            hint = "Move each letter one step BACK. T becomes S.";
            answers = "savior|saviour|the savior|the saviour";
            memoryText = "FRAGMENT 3: The deletion list has your name. ZERO was given to you after they erased the real one.";
        }
    }

    void Update()
    {
        int newState = solved ? 2 : (GameManager.Instance.IsNext(terminalNumber) ? 1 : 0);
        if (newState != state)
        {
            state = newState;
            Paint();
        }

        if (open)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                ClosePuzzle();
            return;
        }

        near = !puzzleOpen && !solved &&
               Vector3.Distance(transform.position, player.position) <= interactDistance;

        if (near && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            TryOpen();
    }

    void Paint()
    {
        Color c = lockedColor;
        if (state == 1) c = activeColor;
        if (state == 2) c = solvedColor;

        rend.material.color = c;
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", c * 3f);
    }

    void TryOpen()
    {
        if (!GameManager.Instance.IsNext(terminalNumber))
        {
            GameManager.Instance.ShowBanner(
                "ACCESS DENIED. Hack Terminal " + GameManager.Instance.NextTerminal() + " first.", 3f);
            return;
        }

        open = true;
        puzzleOpen = true;
        showHint = false;
        input = "";
        feedback = "";
        openedAt = Time.unscaledTime;
        SetPlayerControl(false);
    }

    void ClosePuzzle()
    {
        open = false;
        puzzleOpen = false;
        SetPlayerControl(true);
    }

    void SetPlayerControl(bool on)
    {
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = on;
        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !on;
    }

    void Submit()
    {
        string typed = input.Trim().ToLower();
        bool ok = false;
        foreach (string a in answers.Split('|'))
        {
            if (typed == a.Trim().ToLower()) ok = true;
        }

        if (ok)
        {
            solved = true;
            feedback = "";
            GameManager.Instance.TerminalSolved(terminalNumber, memoryText);
        }
        else
        {
            feedback = "ACCESS DENIED. Try again.";
            wrongUntil = Time.unscaledTime + 1f;
            input = "";
        }
    }

    void MakeStyles()
    {
        if (titleStyle != null) return;

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 36;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.wordWrap = true;
        titleStyle.normal.textColor = new Color(0f, 0.94f, 1f);

        bodyStyle = new GUIStyle(GUI.skin.label);
        bodyStyle.fontSize = 26;
        bodyStyle.alignment = TextAnchor.MiddleCenter;
        bodyStyle.wordWrap = true;
        bodyStyle.normal.textColor = Color.white;

        fieldStyle = new GUIStyle(GUI.skin.textField);
        fieldStyle.fontSize = 30;
        fieldStyle.alignment = TextAnchor.MiddleCenter;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;

        promptStyle = new GUIStyle(GUI.skin.label);
        promptStyle.fontSize = 28;
        promptStyle.alignment = TextAnchor.MiddleCenter;
        promptStyle.normal.textColor = new Color(0f, 0.94f, 1f);
    }

    void OnGUI()
    {
        MakeStyles();
        float w = Screen.width;
        float h = Screen.height;

        if (near && !open)
        {
            GUI.Label(new Rect(w / 2 - 300, h - 130, 600, 50),
                "Press E to use Terminal " + terminalNumber, promptStyle);
            return;
        }

        if (!open) return;

        GUI.depth = -5;

        // panel color changes with what is happening
        Color panelColor = new Color(0f, 0.12f, 0.18f, 0.96f);
        if (solved) panelColor = new Color(0f, 0.22f, 0.08f, 0.96f);
        else if (Time.unscaledTime < wrongUntil) panelColor = new Color(0.35f, 0f, 0.06f, 0.96f);

        GUI.color = panelColor;
        GUI.DrawTexture(new Rect(w * 0.12f, h * 0.10f, w * 0.76f, h * 0.80f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        Event e = Event.current;
        if (!solved && e.type == EventType.KeyDown &&
            (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
        {
            Submit();
            e.Use();
        }

        if (!solved)
        {
            GUI.Label(new Rect(w * 0.15f, h * 0.12f, w * 0.7f, 60),
                "TERMINAL " + terminalNumber + "  //  MINDCORP ACCESS", titleStyle);

            GUI.Label(new Rect(w * 0.18f, h * 0.22f, w * 0.64f, h * 0.26f), question, bodyStyle);

            if (showHint)
            {
                GUIStyle hintStyle = new GUIStyle(bodyStyle);
                hintStyle.fontSize = 22;
                hintStyle.normal.textColor = new Color(1f, 0.9f, 0.1f);
                GUI.Label(new Rect(w * 0.18f, h * 0.48f, w * 0.64f, 60), "HINT: " + hint, hintStyle);
            }

            GUI.SetNextControlName("answer");
            input = GUI.TextField(new Rect(w * 0.30f, h * 0.58f, w * 0.40f, 55), input, 24, fieldStyle);
            if (Time.unscaledTime - openedAt > 0.2f && GUI.GetNameOfFocusedControl() != "answer")
                GUI.FocusControl("answer");

            if (GUI.Button(new Rect(w * 0.30f, h * 0.67f, w * 0.19f, 55), "SUBMIT (Enter)", buttonStyle))
                Submit();
            if (GUI.Button(new Rect(w * 0.51f, h * 0.67f, w * 0.19f, 55), "SHOW HINT", buttonStyle))
                showHint = true;

            GUIStyle redStyle = new GUIStyle(bodyStyle);
            redStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
            GUI.Label(new Rect(w * 0.18f, h * 0.76f, w * 0.64f, 50), feedback, redStyle);

            GUIStyle small = new GUIStyle(bodyStyle);
            small.fontSize = 18;
            GUI.Label(new Rect(w * 0.18f, h * 0.83f, w * 0.64f, 40), "Press Esc to close the terminal", small);
        }
        else
        {
            GUIStyle green = new GUIStyle(titleStyle);
            green.normal.textColor = new Color(0f, 1f, 0.4f);
            GUI.Label(new Rect(w * 0.15f, h * 0.15f, w * 0.7f, 70), "ACCESS GRANTED", green);
            GUI.Label(new Rect(w * 0.18f, h * 0.30f, w * 0.64f, h * 0.30f), memoryText, bodyStyle);

            if (GUI.Button(new Rect(w * 0.38f, h * 0.70f, w * 0.24f, 65), "CONTINUE", buttonStyle))
                ClosePuzzle();
        }
    }
}