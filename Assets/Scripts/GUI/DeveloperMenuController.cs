using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class DeveloperMenuController : MonoBehaviour
{
    public static bool viewAIRaytrace { get; private set; }
    TextMeshProUGUI FPSCounterText;
    Button ActivationButton;
    private void Awake()
    {
        if (!FPSCounterText)
            FPSCounterText = transform.Find("FPSCounter")
                .Find("Value")
                .GetComponent<TextMeshProUGUI>();

        if (!ActivationButton)
            ActivationButton = GameObject.Find("Debug Button")
                .GetComponent<Button>();

        viewAIRaytrace = false;
    }

    //---- Original JS by Aras Pranckevicius (NeARAZ), C# code by Opless
    // Attach this to a FPSCounterText to make a frames/second indicator.
    //
    // It calculates frames/second over each updateInterval,
    // so the display does not keep changing wildly.
    //
    // It is also fairly accurate at very low FPS counts (<10).
    // We do this not by simply counting frames per interval, but
    // by accumulating FPS for each frame. This way we end up with
    // correct overall FPS even if the interval renders something like
    // 5.5 frames.

    public float updateInterval = 0.5F;

    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    void Start()
    {
        if (!FPSCounterText)
        {
            Debug.Log("UtilityFramesPerSecond needs a Text component!");
            enabled = false;
            return;
        }
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            float fps = accum / frames;
            string format = System.String.Format("{0:F2}", fps);
            FPSCounterText.text = format;

            //if (fps < 30)
            //    FPSCounterText.material.color = Color.yellow;
            //else
            //    if (fps < 10)
            //    FPSCounterText.material.color = Color.red;
            //else
            //    FPSCounterText.material.color = Color.green;
            //	DebugConsole.Log(format,level);
            timeleft = updateInterval;
            accum = 0.0F;
            frames = 0;
        }
        // --- End of third party code
    }

    /// <summary>
    /// Adds 1 level of experience to the player's character.
    /// </summary>
    public void LevelUpPlayer()
    {
        int neededXP = LevelController.Player.NextLevelXP;
        LevelController.Player.AddXP(XP: neededXP);

        Debug.Log(message: name + " Player manual level up.");
    }
    /// <summary>
    /// Kills a character.
    /// </summary>
    /// <param name="character">Character to kill.</param>
    public void KillCharacter(Character character)
    {
        Debug.Log(
            message: name + " character: \'" + character.name + "\' killed manually.");
    }
    /// <summary>
    /// Changes the value of the ReturnArgs parameter to match the viewAIRaytrace value.
    /// </summary>
    /// <param name="ReturnArgs"></param>
    public void GetAIRayTraceState(DeveloperOptionBoolController.ReturnArgs ReturnArgs)
    {
        ReturnArgs.value = viewAIRaytrace;
    }
    /// <summary>
    /// Changes teh state of the viewAIRaytrace.
    /// </summary>
    /// <param name="state"></param>
    public void ChangeAIRayTraceState(bool state)
    {
        viewAIRaytrace = state;
    }
}