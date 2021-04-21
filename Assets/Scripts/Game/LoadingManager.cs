using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField]
    private bool isLoading = false;
    // Use this for initialization
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void LoadSceneAsync(string SceneName, bool autoLoad = false)
    {
        if (isLoading)
            return;
        SceneManager.LoadScene("Loading");
        StartCoroutine(AsyncLoad(SceneName,autoLoad));
    }
    private IEnumerator AsyncLoad(string SceneName, bool autoLoad)
    {
        yield return null;

        isLoading = true;
        AsyncOperation ao;
        try
        {
            ao = SceneManager.LoadSceneAsync(SceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
            Application.Quit(403);
            yield break;
        }

        ao.allowSceneActivation = false;

        Debug.Log(
            message: "Scene loading: \'" + SceneName + "\'");
        while (!ao.isDone)
        {
            float progress = Mathf.Clamp01(ao.progress / 0.9f);
            Debug.Log(
                message: "Scene load progress: " + progress*100 + "%");

            GameObject.Find("Loading Bar").GetComponent<Image>().fillAmount = progress;
            if (Mathf.Approximately(ao.progress, 0.9f))
            {
                GameObject.Find("Input Message").GetComponent<TextMeshProUGUI>().text = "Touch the screen to continue";
                
                if (Input.touchCount > 0 || autoLoad || (Application.isPlaying && Input.GetMouseButton(0)))
                {
                    ao.allowSceneActivation = true;
                }
            }
            yield return null;
        }
        isLoading = false;
    }
}