using System;
using System.Collections;
using System.IO;
using UnityEngine;

public static class AppHelper
{
#if UNITY_WEBPLAYER
     public static string webplayerQuitURL = "http://google.com";
#endif
    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        GameObject.Find("Logger")
            .GetComponent<Logger>()
            .OnApplicationQuit();
#elif UNITY_WEBPLAYER
         Application.OpenURL(webplayerQuitURL);
#else
         Application.Quit();
#endif
    }
    /// <summary>
    /// Writes text on a file, if the file defined on filePath doesn't exists is created.
    /// </summary>
    /// <param name="filePath">Full path and file name and extension</param>
    /// <param name="content">Full content of the file</param>
    public static void SaveToFile(string filePath, string content)
    {
        try
        {
            //if (!File.Exists(filePath))
            //{
            //    File.Create(filePath);
            //}
            //StreamWriter streamWriter = new StreamWriter(savePath, true);
            StreamWriter streamWriter = new StreamWriter(filePath, false);
            streamWriter.Write(content);
            streamWriter.Close();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}