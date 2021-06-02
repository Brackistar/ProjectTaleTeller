using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Text;
using System.Linq;

public class Logger : MonoBehaviour
{

    private string savePath;
    public bool allowSave = true;
    /// <summary>
    /// Time period in minutes collected on the logfile.
    /// </summary>
    public float time = 1f;
    public string FileName;
    public string FileType = "json";
    public int Retry = 3;

    private LogInfo logInfo = new LogInfo();

    private void Start()
    {
        DontDestroyOnLoad(this);

        FileName = "logFile_" + SystemInfo.deviceUniqueIdentifier;
        savePath = GameManager.savePath + "/logs";

        Directory.CreateDirectory(savePath);

        savePath += "/" + FileName + "." + FileType;

        Debug.Log(
            message: name + " log file path: \'" + savePath + "\'");

        if (File.Exists(savePath))
            StartCoroutine(SendbyMail());
        //ResetLogFile();
        //StartCoroutine(SaveLogs());
    }

    private IEnumerator SendbyMail()
    {
        int currentTry = 0;
        do
        {
            try
            {
                string subject = "Log file device: " +
                "\'" + SystemInfo.deviceUniqueIdentifier + "\'" +
                "date: " + DateTime.Now.ToString("g");

                bool result = false;
                //using (FileStream stream = File.Open(savePath, FileMode.Open))
                //{
                //    result = NetHelper.SendMail(
                //        subject: subject,
                //        body: "",
                //        attachments: new Attachment[] { new Attachment(stream, Path.GetFileName(savePath)) });
                //}

                using (FileStream stream = File.Open(savePath, FileMode.Open))
                {
                    using (MemoryStream memoryStream = new MemoryStream(AppHelper.GetCompressedFile(
                        content: new AppHelper.VirtualFile[] {
                            new AppHelper.VirtualFile(
                                Name: Path.GetFileName(savePath),
                                Content: stream)
                            })
                        ))
                    {
                        result = NetHelper.SendMail(
                            subject: subject,
                            body: "Log file attached.",
                            attachments: new Attachment[] {
                                new Attachment(
                                    contentStream:memoryStream,
                                    name: Path.GetFileName(savePath)
                                        .Replace(FileType,"zip"))
                            });
                    }
                }

                //bool result = NetHelper.SendMail(
                //    subject: subject,
                //    body: "",
                //    attachments: new string[] { savePath });

                if (result)
                {
                    File.Delete(savePath);
                    break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            //string subject = "Log file device: " +
            //    "\'" + SystemInfo.deviceUniqueIdentifier + "\'" +
            //    "date: " + DateTime.Now.ToString("g");

            //bool result = NetHelper.SendMail(
            //    subject: subject,
            //    body: "",
            //    attachments: new string[] { savePath });

            //if (result)
            //{
            //    File.Delete(savePath);
            //    break;
            //}

            currentTry++;

            yield return new WaitForSeconds(30);
        }
        while (currentTry < Retry);
    }
    private void OnEnable()
    {
        Application.logMessageReceived += LogCallback;
    }
    //Called when there is an exception
    void LogCallback(string condition, string stackTrace, LogType type)
    {
        //Create new Log
        Log log = new Log(condition, stackTrace, type);

        //Add it to the List
        logInfo.logInfoList.Add(log);
    }

    public void OnApplicationQuit()
    {
        allowSave = false;
        Application.logMessageReceived -= LogCallback;
        //StopCoroutine(SaveLogs());
        if (SaveLastLogs())
            StartCoroutine(SendbyMail());
    }

    private bool SaveLastLogs()
    {
        bool result = false;
        LogInfo finalLog = new LogInfo();
        DateTime t = DateTime.Now.AddMinutes(-time);

        finalLog.logInfoList.AddRange(
            logInfo.logInfoList
                .Where(log => Convert.ToDateTime(log.dateTime).CompareTo(t) >= 0)
            );

        if (finalLog.logInfoList.Any(log => log.type == LogType.Error || log.type == LogType.Exception || log.type == LogType.Assert))
        {
            AppHelper.SaveToFile(
                filePath: savePath,
                content: finalLog.ToString());
            result = true;
        }

        return result;
    }
    [Serializable]
    public struct Log
    {
        public string condition;
        public string stackTrace;
        public LogType type;

        public string dateTime;

        //public Log(string condition, string stackTrace, LogType type, string dateTime)
        //{
        //    this.condition = condition;
        //    this.stackTrace = stackTrace;
        //    this.type = type;
        //    this.dateTime = dateTime;
        //}

        public Log(string condition, string stackTrace, LogType type)
        {
            this.condition = condition;
            this.stackTrace = stackTrace;
            this.type = type;
            this.dateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }

        public override string ToString()
        {
            //return base.ToString();
            string result = JsonUtility.ToJson(this);
            return result;
        }
    }

    [Serializable]
    public class LogInfo
    {
        public List<Log> logInfoList = new List<Log>();

        public override string ToString()
        {
            string result = JsonUtility.ToJson(this);
            return result;
        }
    }
}
