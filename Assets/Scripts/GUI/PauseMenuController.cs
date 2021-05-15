using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    /// <summary>
    /// Game is paused
    /// </summary>
    public static bool isPaused;

    [SerializeField]
    private GameObject defaultActivePanel;
    //[SerializeField]
    //private Player playerAvatar;
    [SerializeField]
    private Image characterPortrait;
    [SerializeField]
    private Text characterName,
        characterHistory,
        characterAgility,
        characterStrenght,
        characterResistance,
        characterVitality;
    private static string lastActivePanel = "";
    private AudioSource AudioSource;

    private void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
        isPaused = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        lastActivePanel = defaultActivePanel.name;
        //AudioSource = GetComponent<AudioSource>();
        AudioSource.ignoreListenerPause = true;
        //isPaused = false;

        //transform.Find("No_Button")
        //    .GetComponent<Button>()
        //    .onClick.AddListener(GotoMainMenu);

        ChangeCharacterPortrait();
        ChangeCharacterHistory();
        ChangeShownAttributes();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeCharacterPortrait();
        ChangeCharacterHistory();
        ChangeShownAttributes();
    }

    //public void Show_Hide()
    //{
    //    TargetPanel.SetActive(!TargetPanel.activeSelf);
    //}
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Target"></param>
    public void HidePanel(GameObject Target)
    {
        Target.SetActive(false);
    }

    public void ShowPanel(GameObject Target)
    {
        Target.SetActive(true);
        //SetLastActive(Target.name);
    }

    public void Show_HideLastActive(GameObject Target)
    {
        if (string.IsNullOrEmpty(lastActivePanel))
        {
            Show_Hide(Target);
        }
        else
        {
            GameObject LastActive = GameObject.Find(lastActivePanel);
            Show_Hide(LastActive, Target);
        }
    }

    public void Show_Hide(GameObject Target)
    {
        Target.SetActive(!Target.activeSelf);
        //if (Target.activeSelf)
        //    SetLastActive(Target.name);
    }

    public void Show_Hide(GameObject PreviouslyActive, GameObject Target)
    {
        PreviouslyActive.SetActive(false);
        Target.SetActive(true);
        SetLastActive(Target.name);
    }

    public void Pause_Resume()
    {
        if (!isPaused)
        {
            //Time.timeScale = 0;
            //AudioListener.pause = true;
            LevelController.Pause();
            if (AudioSource.time < 7f)
                AudioSource.time = 7f;
            AudioSource.Play();
        }
        else
        {
            //Time.timeScale = 1;
            //AudioListener.pause = false;

            LevelController.Resume();
            AudioSource.Pause();
        }

        isPaused = !isPaused;
    }

    public void Load_Scene(string SceneName)
    {
        GameObject.Find("GameManager")
            .GetComponent<GameManager>()
            .LoadSceneAsync(SceneName);
    }

    public void GotoMainMenu()
    {
        GameObject.Find("GameManager")
            .GetComponent<GameManager>()
            .LoadSceneAsync("MainMenu", true);
    }

    public void RestartLevel()
    {
        string LevelName = SceneManager.GetActiveScene().path;
        Load_Scene(LevelName);
    }

    public void Exit_Game()
    {
        Debug.Log(
            message: "Exiting game.");
        AppHelper.Quit();
    }

    public void DeleteSave()
    {
        Debug.Log(
            message:"Deleting save.");
    }

    private void SetLastActive(string TargetName)
    {
        lastActivePanel = (lastActivePanel.Equals(TargetName)) ? string.Empty : TargetName;
    }

    private void ChangeCharacterPortrait()
    {
        if (characterPortrait.sprite.name == LevelController.Player.GetImage().name)
            return;
        characterPortrait.sprite = LevelController.Player.GetImage();
    }

    private void ChangeCharacterHistory()
    {
        characterName.text = LevelController.Player.Name;
        characterHistory.text = LevelController.Player.GetHistory();
    }

    private void ChangeShownAttributes()
    {
        characterAgility.text = System.Math.Round(
            LevelController.Player.GetAgility(),
            1).ToString();
        characterStrenght.text = System.Math.Round(
            LevelController.Player.GetStrength(),
            1).ToString();
        characterResistance.text = System.Math.Round(
            LevelController.Player.GetResistance(),
            1).ToString();
        characterVitality.text = System.Math.Round(
            LevelController.Player.GetVitality(),
            1).ToString();
    }
}
