using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ElectricityBot
{
    public enum BotState { Default, Usage };

    public class ElectricityUsage
    {
        public int usage { get; set; }
    }

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        internal static IDialog<ElectricityUsageQuery> MakeUsageDialog()
        {
            return Chain.From(() => FormDialog.FromForm(ElectricityUsageQuery.BuildForm));
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
                if (message.Text == "電力使用状況は？" || message.GetBotPerUserInConversationData<BotState>("state") == BotState.Usage)
                {
                    message.SetBotPerUserInConversationData("state", BotState.Usage);

                    // FormFlow を呼び出して、電力会社について情報を集める
                    return await Conversation.SendAsync(message, MakeUsageDialog);
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