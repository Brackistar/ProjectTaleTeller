using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject TargetPanel;
    public GameObject LoadButton;
    public string NewGameScene;
    private bool HasSave;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Show_Hide()
    {
        TargetPanel.SetActive(!TargetPanel.activeSelf);
    }

    public void Quit()
    {
        Debug.Log(
            message: "Exiting game.");
        Application.Quit();
    }

    public void NewGame()
    {
        SceneManager.LoadScene(
            sceneName: NewGameScene);
    }
}
