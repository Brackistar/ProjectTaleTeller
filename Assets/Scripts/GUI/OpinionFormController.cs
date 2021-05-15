using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class OpinionFormController : MonoBehaviour
{
    private TMP_InputField GeneralOpinion,
        Errors,
        Suggestions,
        Email;
    private TextMeshProUGUI ErrorMessage;
    private Button SendButton;
    private string savePath;
    private TouchScreenKeyboard keyboard;
    private GameObject WaitMessage;

    private void Awake()
    {
        if (!SendButton)
            SendButton = GameObject.Find("Send_Button")
                .GetComponent<Button>();

        if (!WaitMessage)
            WaitMessage = Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(text => text.transform.name == "Sending Message Container");

        WaitMessage.SetActive(false);

        savePath = GameManager.savePath + "/mail/";

        Directory.CreateDirectory(savePath);

#if UNITY_EDITOR
        if (!GameObject.Find("GameManager"))
        {
            GameObject GameManager = Instantiate(new GameObject(), null);
            GameManager.name = "GameManager";
            GameManager.AddComponent<GameManager>();
        }
#endif
    }
    // Use this for initialization
    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        GeneralOpinion = GameObject.Find("General Opinion/InputField (TMP)")
            .GetComponent<TMP_InputField>();
        GeneralOpinion.onEndEdit.AddListener(OnTextEditEnd);
        GeneralOpinion.onSelect.AddListener(OnTextSelect);

        Errors = GameObject.Find("Errors/InputField (TMP)")
            .GetComponent<TMP_InputField>();
        Errors.onEndEdit.AddListener(OnTextEditEnd);
        Errors.onSelect.AddListener(OnTextSelect);

        Suggestions = GameObject.Find("Suggestions/InputField (TMP)")
            .GetComponent<TMP_InputField>();
        Suggestions.onEndEdit.AddListener(OnTextEditEnd);
        Suggestions.onSelect.AddListener(OnTextSelect);

        Email = GameObject.Find("Contact Info/InputField (TMP)")
            .GetComponent<TMP_InputField>();
        Email.onEndEdit.AddListener(OnEmailEditEnd);
        Email.onSelect.AddListener(OnTextSelect);

        //GameObject errorMessageContainer = transform.Find("Canvas/Background/Error Message Container")
        //    .gameObject;

        //ErrorMessage = errorMessageContainer
        //    .transform.Find("Error Message")
        //    .GetComponent<TextMeshProUGUI>();

        ErrorMessage = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
            .FirstOrDefault(text => text.transform.name == "Error Message");

        SendButton.interactable = false;

        if (Directory.Exists(savePath))
        {
            string[] savedForms = Directory.GetFiles(
                path: savePath,
                searchPattern: "*.json",
                searchOption: SearchOption.TopDirectoryOnly);

            if (savedForms.Length > 0)
                StartCoroutine(SendSavedMail(savedForms));
        }
    }
    public void SendForm()
    {
        WaitMessage.SetActive(true);

        StartCoroutine(SendFormAsync());

        //string subject = "Opinion form " +
        //    DateTime.Now.ToString("g");
        //string body = "<html><head><title>Opinion form</title></head>" +
        //    "<body><p>";

        //if (!string.IsNullOrWhiteSpace(Email.text))
        //    body += "<h3> Contact info:</h3><li style =\"margin - left: 1em; \">" +
        //            "<b>E-Mail: </b><i>" + Email.text + "</i>" +
        //            "</li><hr>";


        //body += "<main>";

        //if (!string.IsNullOrWhiteSpace(GeneralOpinion.text))
        //    body += "<article><h3>General Opinion:</h3><br><div><pre>" +
        //        GeneralOpinion.text +
        //        "</pre></div></article>";

        //if (!string.IsNullOrWhiteSpace(Errors.text))
        //    body += "<article><h3>Problems found:</h3><br><div><pre>" +
        //        Errors.text +
        //        "</pre></div></article>";

        //if (!string.IsNullOrWhiteSpace(Suggestions.text))
        //    body += "<article><h3>Suggestions:</h3><br><div><pre>" +
        //        Suggestions.text +
        //        "</pre></div></article>";

        //body += "<hr></main>" +
        //    "<b>Opinion form send: </b>" + DateTime.Now.ToString("dd-MM-yyyy") + "<br>" +
        //    "<hr></p></body></html>";

        //Debug.Log(
        //    message: name + " Opinion form ready to send.");

        //bool isMailSend = NetHelper.SendMail(
        //    subject: subject,
        //    body: body,
        //    HTMLBody: true);

        //if (!isMailSend)
        //{
        //    Debug.Log(
        //        message: name + " E-Mail couldn't be send, saving for future re-try.");

        //    string savePath = this.savePath +
        //        SystemInfo.deviceUniqueIdentifier + "_" +
        //        DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") +
        //        ".json";

        //    NetHelper.EmailPrototype prototype = new NetHelper.EmailPrototype(
        //        subject: subject,
        //        body: body,
        //        isHTML: true);

        //    AppHelper.SaveToFile(
        //        filePath: savePath,
        //        content: JsonUtility.ToJson(prototype));
        //}
        //else
        //{
        //    Debug.Log(
        //        message: name + "Form sended.");
        //}
        //GameObject.Find("GameManager")
        //    .GetComponent<GameManager>()
        //    .LoadSceneAsync("MainMenu", true);
    }

    private IEnumerator SendFormAsync()
    {
        yield return new WaitForFixedUpdate();
        string subject = "Opinion form " +
            DateTime.Now.ToString("g");
        string body = "<html><head><title>Opinion form</title></head>" +
            "<body><p>";

        if (!string.IsNullOrWhiteSpace(Email.text))
            body += "<h3> Contact info:</h3><li style =\"margin - left: 1em; \">" +
                    "<b>E-Mail: </b><i>" + Email.text + "</i>" +
                    "</li><hr>";


        body += "<main>";

        if (!string.IsNullOrWhiteSpace(GeneralOpinion.text))
            body += "<article><h3>General Opinion:</h3><br><div><pre>" +
                GeneralOpinion.text +
                "</pre></div></article>";

        if (!string.IsNullOrWhiteSpace(Errors.text))
            body += "<article><h3>Problems found:</h3><br><div><pre>" +
                Errors.text +
                "</pre></div></article>";

        if (!string.IsNullOrWhiteSpace(Suggestions.text))
            body += "<article><h3>Suggestions:</h3><br><div><pre>" +
                Suggestions.text +
                "</pre></div></article>";

        body += "<hr></main>" +
            "<b>Opinion form send: </b>" + DateTime.Now.ToString("dd-MM-yyyy") + "<br>" +
            "<hr></p></body></html>";

        Debug.Log(
            message: name + " Opinion form ready to send.");

        bool isMailSend = NetHelper.SendMail(
            subject: subject,
            body: body,
            HTMLBody: true);

        if (!isMailSend)
        {
            Debug.Log(
                message: name + " E-Mail couldn't be send, saving for future re-try.");

            string savePath = this.savePath +
                SystemInfo.deviceUniqueIdentifier + "_" +
                DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") +
                ".json";

            NetHelper.EmailPrototype prototype = new NetHelper.EmailPrototype(
                subject: subject,
                body: body,
                isHTML: true);

            AppHelper.SaveToFile(
                filePath: savePath,
                content: JsonUtility.ToJson(prototype));
        }
        else
        {
            Debug.Log(
                message: name + "Form sended.");
        }
        GameObject.Find("GameManager")
            .GetComponent<GameManager>()
            .LoadSceneAsync("MainMenu", true);
    }
    public void Cancel()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        GameObject.Find("GameManager")
            .GetComponent<GameManager>()
            .LoadSceneAsync("MainMenu", true);
    }

    private void OnTextSelect(string text)
    {
        keyboard = TouchScreenKeyboard.Open(text);
    }

    private void OnTextEditEnd(string text)
    {
        bool hasText = !string.IsNullOrWhiteSpace(GeneralOpinion.text)
            || !string.IsNullOrWhiteSpace(Errors.text)
            || !string.IsNullOrWhiteSpace(Suggestions.text);

        keyboard = null;

        SendButton.interactable = hasText;

        //if (!string.IsNullOrWhiteSpace(text))
        //{
        //    TotalTextFields++;
        //}
        //else
        //{
        //    if (TotalTextFields > 0)
        //        TotalTextFields--;
        //}

        //SendButton.interactable = TotalTextFields > 0;

    }

    public void OnEmailEditEnd(string email)
    {
        // Email validation regex by jago link: https://regex101.com/library/sI6yF5
        Regex regex = new Regex(@"^([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x22([^\x0d\x22\x5c\x80-\xff]|\x5c[\x00-\x7f])*\x22)(\x2e([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x22([^\x0d\x22\x5c\x80-\xff]|\x5c[\x00-\x7f])*\x22))*\x40([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x5b([^\x0d\x5b-\x5d\x80-\xff]|\x5c[\x00-\x7f])*\x5d)(\x2e([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x5b([^\x0d\x5b-\x5d\x80-\xff]|\x5c[\x00-\x7f])*\x5d))*$");

        if (!regex.IsMatch(email))
        {
            Email.text = null;

            ErrorMessage.text = "E-Mail is not valid";
            ErrorMessage.transform
                .parent
                .gameObject
                .SetActive(true);
        }
    }

    private IEnumerator SendSavedMail(IEnumerable<string> FilePaths)
    {
        int retry = 0;
        do
        {
            try
            {
                foreach (string filePath in FilePaths)
                {

                    NetHelper.EmailPrototype prototype = JsonUtility.FromJson<NetHelper.EmailPrototype>(
                        json: File.ReadAllText(filePath));

                    bool isMailSend = NetHelper.SendMail(
                        subject: prototype.subject,
                        body: prototype.body,
                        HTMLBody: prototype.isHTML);

                    if (isMailSend)
                    {
                        File.Delete(filePath);
                        List<string> _FilePaths = FilePaths.ToList();
                        _FilePaths.Remove(filePath);

                        FilePaths = _FilePaths.AsEnumerable();
                        Debug.Log(
                            message: name + " saved form sended \'" + Path.GetFileName(filePath) + "\'");
                    }
                }
                if (FilePaths.Count() > 0)
                    retry++;

                Debug.Log(
                    message: name + "Retry number: " + retry.ToString());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            yield return new WaitForSecondsRealtime(30);
        }
        while (retry < 3 && FilePaths.Count() > 0);
    }
}