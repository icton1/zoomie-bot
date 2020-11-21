using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace GimmeTheZoomBot
{
    static public class GmailServiceWorker
    {
        static string[] Scopes = { GmailService.Scope.GmailReadonly };

        static string ApplicationName = "Gmail API .NET Quickstart";
        static UserCredential credential;

        static public bool IsAuth(long chatId)
        {
            var files = Directory.GetFiles("token.json");

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);
                if (ext.Contains(chatId.ToString()))
                    return true;
            }

            return false;
        }

        static public UserCredential Init(long chatId)
        {

            using (var stream =
                 new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    chatId.ToString(),
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);

                return credential;
            }

        }


        static public void ReInit(long chatId)
        {
            using (var stream =
                 new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";

                if(credential == null)
                {
                    credential = Init(chatId);
                }
                else
                {
                    try
                    {
                        GoogleWebAuthorizationBroker.ReauthorizeAsync(
                            credential,
                            CancellationToken.None);
                        Console.WriteLine("Credential file saved to: " + credPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return;
                    }
                }

            }

        }

        static public GmailService GetService()
        {
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;
        }

        static public List<Message> GetGmailMessage(string from = "process@isu.ifmo.ru")
        {
            List<Message> messsages = new List<Message>();

            var service = GetService();

            var request = service.Users.Messages.List("me");
            request.Q = $"from:{from}";
            request.LabelIds = "INBOX";
            request.IncludeSpamTrash = false;

            try
            {
                var response = request.Execute();
                
                if (response.Messages != null)
                {
                    int count = 0;
                    foreach (var mail in response.Messages)
                    {
                        count++;
                        var mailId = mail.Id;
                        var threadId = mail.ThreadId;

                        var messageRequest = service.Users.Messages.Get("me", mailId);
                        messageRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
                        var message = messageRequest.Execute();

                        var payload = message.Payload;
                        var parts = payload.Parts;

                        foreach(var part in parts)
                        {
                            var body = part.Body.Data;

                            if (body != null)
                            {
                                var decodeBody = Base64UrlEncoder.Decode(body);
                                Console.WriteLine(decodeBody);
                            }
                        }

                    }

                    Console.WriteLine(count);
                }
                else
                {
                    Console.WriteLine("Nothing was found!");
                }

                return messsages;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return messsages;
            }
        }

        static public string GetGmailName(long chatId)
        {
            var service = GetService();

            var mail = service.Users.GetProfile("me").Execute();

            Console.WriteLine(mail.EmailAddress);

            return mail.EmailAddress;
        }

        //static public void DeleteGmail(long chatId)
        //{
        //    var files = Directory.GetFiles("token.json");

        //    foreach (var file in files)
        //    {
        //        if (file.Contains(chatId.ToString()))
        //        {
        //            File.Delete(file);
        //            break;
        //        }

        //    }
        //}

    }
}
