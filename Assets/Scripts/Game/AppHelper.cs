using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

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
            //StreamWriter streamWriter = new StreamWriter(filePath, false);
            //streamWriter.Write(content);
            //streamWriter.Close();

            using (StreamWriter streamWriter = new StreamWriter(filePath, false))
            {
                streamWriter.Write(content);
                streamWriter.Close();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public static byte[] GetCompressedFile(VirtualFile[] content)
    {

        using (MemoryStream outStream = new MemoryStream())
        {
            using (ZipArchive archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
            {
                foreach (VirtualFile file in content)
                {
                    var fileInArchive = archive.CreateEntry(file.Name, CompressionLevel.Optimal);
                    using (Stream entryStream = fileInArchive.Open())
                    {
                        using (MemoryStream fileToCompressStream = new MemoryStream(file.Content))
                        {
                            fileToCompressStream.CopyTo(entryStream);
                        }
                    }
                }
            }
            return outStream.ToArray();
        }
    }

    public static bool Approximately(float a, float b, float margin = 30)
    {
        return Mathf.Abs(a - b) <= margin;
    }

    public struct VirtualFile
    {
        /// <summary>
        /// Full file name with extension
        /// </summary>
        public string Name { private set; get; }
        /// <summary>
        /// Content of the file
        /// </summary>
        public byte[] Content { private set; get; }

        //public VirtualFile(string Name, MemoryStream Content)
        //{
        //    this.Name = Name
        //        .Replace(" ", "_");
        //    this.Content = Content;
        //}
        //public VirtualFile(string Name, FileStream Content)
        //{
        //    this.Name = Name
        //        .Replace(" ", "_");

        //    using (MemoryStream newContent = new MemoryStream())
        //    {
        //        Content.CopyTo(newContent);
        //        this.Content = newContent;
        //    }
        //}

        public VirtualFile(string Name, Stream Content)
        {
            this.Name = Name
                .Replace(" ", "_");

            using (MemoryStream _ = new MemoryStream())
            {
                Content.CopyTo(_);
                this.Content = _.ToArray();
            }
        }
    }
}