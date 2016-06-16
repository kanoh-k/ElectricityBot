using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ElectricityBot
{
    public class ElectricityUsage
    {
        public int usage { get; set; }
    }

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private int? GetElectricityUsage()
        {
            int? result = null;
            try
            {
                var requestUrl = @"http://setsuden.yahooapis.jp/v1/Setsuden/latestPowerUsage?appid=＜アプリケーションID＞&area=tokyo&output=json";
                var request = WebRequest.Create(requestUrl);
                var response = request.GetResponse();
                var rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var json = JObject.Parse(rawJson);
                result = (int)json["ElectricPowerUsage"]["Usage"]["$"];
            }
            catch
            {
                // Do nothing.
            }
            return result;
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {

                Message reply = null;
                if (message.Text == "電力使用状況は？")
                {
                    //reply = message.CreateReplyMessage("？？？kWです");
                    var usage = GetElectricityUsage();
                    reply = message.CreateReplyMessage(
                        usage != null ? string.Format("{0}kWです", usage) : "取得できませんでした。。"
                        );
                }
                else
                {
                    reply = message.CreateReplyMessage("(｡･ ω<)ゞてへぺろ♡");
                }
                return reply;
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}