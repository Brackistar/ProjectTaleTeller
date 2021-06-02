using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using UnityEngine;


public class NetHelper : MonoBehaviour
{
    public static string From = "logs.brackistargames@outlook.com";
    public static string Password = "b4nc0*666";
    public static string To = "brackistar@hotmail.com";
    public static string smtp = "smtp-mail.outlook.com";
    public static int port = 587;

    private const bool allowCarrierDataNetwork = false;
    private const string pingAddress = "8.8.8.8"; // Google Public DNS server
    private const float waitingTime = 2.0f;

    private Ping ping;
    private float pingStartTime;
    private static bool InternetAvaible = false;

    public void Start()
    {
        DontDestroyOnLoad(this);
        bool internetPossiblyAvailable;
        switch (Application.internetReachability)
        {
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                internetPossiblyAvailable = true;
                Debug.Log(
                    message: name + "Connected via wi-fi.");
                break;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                internetPossiblyAvailable = allowCarrierDataNetwork;
                Debug.Log(
                    message: name + "Connected via carrier data network.");
                break;
            default:
                internetPossiblyAvailable = false;
                Debug.Log(
                    message: name + "Non connected.");
                break;
        }
        if (!internetPossiblyAvailable)
        {
            //InternetIsNotAvailable();
            InternetAvaible = false;
            return;
        }
        ping = new Ping(pingAddress);
        pingStartTime = Time.time;
    }

    private void Update()
    {
        if (ping != null)
        {
            bool stopCheck = true;
            if (ping.isDone)
            {
                if (ping.time >= 0)
                    //InternetAvailable();
                    InternetAvaible = true;
                else
                    //InternetIsNotAvailable();
                    InternetAvaible = false;
            }
            else if (Time.time - pingStartTime < waitingTime)
                stopCheck = false;
            else
                //InternetIsNotAvailable();
                InternetAvaible = false;
            if (stopCheck)
                ping = null;
        }
        else
        {
            ping = new Ping(pingAddress);
            pingStartTime = Time.time;
        }
    }

    public static bool SendMail(string subject, string body, IEnumerable<Attachment> attachments = null, bool HTMLBody = false)
    {
        bool result = false;
        if (InternetAvaible)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    //MailMessage mail = new MailMessage(
                    //from: new MailAddress(From),
                    //to: new MailAddress(To))
                    //{
                    //    Subject = subject,
                    //    Body = body
                    //};
                    mail.From = new MailAddress(From);
                    mail.To.Add(new MailAddress(To));
                    mail.Subject = subject;
                    mail.Body = body;

                    if (attachments != null)
                        foreach (Attachment attachment in attachments)
                            mail.Attachments.Add(attachment);

                    mail.IsBodyHtml = HTMLBody;

                    using (SmtpClient smtpClient = new SmtpClient())
                    {
                        smtpClient.Host = smtp;
                        smtpClient.Port = port;
                        smtpClient.Credentials = new NetworkCredential(
                            userName: From,
                            password: Password);
                        smtpClient.EnableSsl = true;
                        smtpClient.Send(mail);
                    }
                    result = true;
                }
                //MailMessage mail = new MailMessage(
                //    from: new MailAddress(From),
                //    to: new MailAddress(To))
                //{
                //    Subject = subject,
                //    Body = body
                //};

                //if (attachments != null)
                //    foreach (string savePath in attachments)
                //        mail.Attachments.Add(
                //            new Attachment(savePath));

                //mail.IsBodyHtml = HTMLBody;

                ////SmtpClient smtpClient = new SmtpClient(
                ////    host: smtp,
                ////    port: port);
                ////smtpClient.Credentials = new NetworkCredential(
                ////    userName: From,
                ////    password: Password);
                ////smtpClient.EnableSsl = true;

                ////smtpClient.Send(mail);
                ////result = true;

                //using (SmtpClient smtpClient = new SmtpClient())
                //{
                //    smtpClient.Host = smtp;
                //    smtpClient.Port = port;
                //    smtpClient.Credentials = new NetworkCredential(
                //        userName: From,
                //        password: Password);
                //    smtpClient.EnableSsl = true;
                //    smtpClient.Send(mail);
                //}
                //result = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        else
        {
            Debug.Log(
                message: "Mail can't be send. Internet not avaible.");
        }
        return result;
    }

    public static bool SendMail(IEnumerable<string> ToList, string subject, string body, IEnumerable<string> attachments = null, bool HTMLBody = false)
    {
        bool result = false;
        if (InternetAvaible)
        {
            try
            {
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(From),
                    Subject = subject,
                    Body = body
                };

                foreach (string To in ToList)
                    mail.To.Add(new MailAddress(To));

                if (attachments != null)
                    foreach (string savePath in attachments)
                        mail.Attachments.Add(
                            new Attachment(savePath));

                mail.IsBodyHtml = HTMLBody;

                SmtpClient smtpClient = new SmtpClient(
                    host: smtp,
                    port: port);
                smtpClient.Credentials = new NetworkCredential(
                    userName: From,
                    password: Password);
                smtpClient.EnableSsl = true;

                if (InternetAvaible)
                {
                    smtpClient.Send(mail);
                    result = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        else
        {
            Debug.Log(
                message: "Mail can't be send. Internet not avaible.");
        }
        return result;
    }

    [Serializable]
    public struct EmailPrototype
    {
        public string subject;
        public string body;
        public bool isHTML;

        public EmailPrototype(string subject, string body, bool isHTML = false)
        {
            this.subject = subject;
            this.body = body;
            this.isHTML = isHTML;
        }
    }
}