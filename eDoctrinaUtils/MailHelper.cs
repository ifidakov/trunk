using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace eDoctrinaUtils
{
    public class MailHelper
    {
        Log log = new Log();

        public void SendMail()
        {
            SendMail(null, null);
        }
        //-------------------------------------------------------------------------
        public void SendMail(string caption)
        {
            SendMail(caption, null);
        }
        //-------------------------------------------------------------------------
        public void SendMail(string caption, string message)
        {
            if (!CanSendCommonMail) return;
            if (AppConfig.email.host.StartsWith("/")) return;
            string mail = AppConfig.email.emails;
            string[] mails = Regex.Split(mail, "\\s+");
            foreach (string item in mails)
            {
                if (!String.IsNullOrEmpty(item) && !item.StartsWith("/"))
                {
                    Send(caption, message, item);
                }
            }
        }
        //-------------------------------------------------------------------------
        private Batches AppConfig;
        //-------------------------------------------------------------------------
        private Batches GetAppConfig()
        {
            Exception ex;
            var serializer = new SerializerHelper();
            return serializer.GetBatchesFromFile(OcrAppConfig.AppConfigFileName, out ex);
        }
        //-------------------------------------------------------------------------
        private bool CanSendCommonMail
        {
            get
            {
                AppConfig = GetAppConfig();
                if (AppConfig == null) return false;
                if (AppConfig.email == null) return false;
                if (String.IsNullOrEmpty(AppConfig.email.host)) return false;
                return true;
            }
        }
        //-------------------------------------------------------------------------
        public void SendMailToAdminWithError(string caption, string message)
        {
            if (!CanSendCommonMail)
            {
                OcrAppConfig def = new OcrAppConfig();
                AppConfig = def.CreateDefaultAppConfig();
                if (AppConfig == null) return;
            }
            if (AppConfig.email.host.StartsWith("/"))
                AppConfig.email.host = AppConfig.email.host.Replace("//", "");
            string adminMail = @"mkukharuk@itera-research.com";//adminMailForError TestPeriod
            Send(caption, message, adminMail);
        }
        //-------------------------------------------------------------------------
        public void SendMailToSupport(string message, string mail, string caption = "[OCR] Error report")
        {
            if (!CanSendCommonMail)
            {
                OcrAppConfig def = new OcrAppConfig();
                AppConfig = def.CreateDefaultAppConfig();
                if (AppConfig == null) return;
            }
            if (AppConfig.email.host.StartsWith("/"))
                AppConfig.email.host = AppConfig.email.host.Replace("//", "");
            //var caption = "[OCR] Error report";
            string[] mails = Regex.Split(mail, ";");
            foreach (string item in mails)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    Send(caption, message, item);
                }
            }
        }
        //-------------------------------------------------------------------------
        private void Send(string caption, string message, string mail)
        {
            caption = (caption == null) ? AppConfig.email.caption : caption;
            message = (message == null) ? AppConfig.email.message : message;
            try
            {
                var port = Convert.ToInt32(AppConfig.email.port);
                SendMail(AppConfig.email.host, AppConfig.email.from, AppConfig.email.password, mail, caption, message, port);
            }
            catch (Exception)
            {
                SendMail(AppConfig.email.host, AppConfig.email.from, AppConfig.email.password, mail, caption, message);
            }
        }
        //-------------------------------------------------------------------------
        public List<string> Attachments;
        //-------------------------------------------------------------------------
        /// <summary>
        /// Отправка письма на почтовый ящик C# mail send
        /// </summary>
        /// <param name="smtpServer">Имя SMTP-сервера</param>
        /// <param name="from">Адрес отправителя</param>
        /// <param name="password">пароль к почтовому ящику отправителя</param>
        /// <param name="mailto">Адрес получателя</param>
        /// <param name="caption">Тема письма</param>
        /// <param name="message">Сообщение</param>
        /// <param name="attachFile">Присоединенный файл</param>
        private void SendMail(string smtpServer, string from, string password, string mailto, string caption, string message, int port = 587, string attachFile = null)
        {
            try
            {
                MailMessage mail = new MailMessage(from, mailto, caption, message);
                if (!String.IsNullOrWhiteSpace(attachFile))
                    mail.Attachments.Add(new Attachment(attachFile));
                if (this.Attachments != null && this.Attachments.Count > 0)
                {
                    foreach (var file in this.Attachments)
                    {
                        mail.Attachments.Add(new Attachment(file));
                    }
                }
                SmtpClient client = new SmtpClient();
                client.Host = smtpServer;
                client.Port = port;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(from.Split('@')[0], password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mail);
                mail.Dispose();
            }
            catch (Exception ex)
            {
                log.LogMessage("Mail.Send: ", ex);
            }
        }
    }
}
