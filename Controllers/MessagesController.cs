using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace ElectricityBot
{
    public enum ElectricityBotState
    {
        Default,
        RequestCompany,
        RequestPhotoKeyword
    };

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                // Botの現在の状態を取得
                StateClient stateClient = activity.GetStateClient();
                BotData conversationData = stateClient.BotState.GetConversationData(activity.ChannelId, activity.Conversation.Id);
                var state = conversationData.GetProperty<ElectricityBotState>("state");

                if (state == ElectricityBotState.Default)
                {
                    // Default状態の場合はLuisDialogを呼ぶ
                    await Conversation.SendAsync(activity, () => new ElectricityDialog());
                }
                else if (state == ElectricityBotState.RequestCompany)
                {
                    // RequestConpamy状態の場合は電力会社が入力されているので、
                    // activity.Textの内容をもとに電力使用状況を取得する
                    string message;
                    if (activity.Text != null)
                    {
                        var usage = ElectricityUsageAPI.GetElectricityUsage(activity.Text);
                        message = (usage != null) ? string.Format("{0}kWです。", usage) : "取得できませんでした。。";
                    }
                    else
                    {
                        message = "電力会社が見つかりませんでした";
                    }

                    // 結果をユーザーに送信
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    Activity reply = activity.CreateReply(message);
                    await connector.Conversations.ReplyToActivityAsync(reply);

                    // 会話の区切りがついたので、状態をデフォルトに戻す
                    conversationData.SetProperty<ElectricityBotState>("state", ElectricityBotState.Default);
                    await stateClient.BotState.SetConversationDataAsync(activity.ChannelId, activity.Conversation.Id, conversationData);
                }
                else if (state == ElectricityBotState.RequestPhotoKeyword)
                {
                    var keyword = activity.Text;
                    var uri = PhotoSearchAPI.GetPhotoByKeyword(keyword);
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                    if (uri == null)
                    {
                        Activity reply = activity.CreateReply("写真を取得できませんでした。。");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                    else
                    {
                        Activity photoMessage = activity.CreateReply(keyword + "の写真だよ！");
                        photoMessage.Attachments.Add(new Attachment()
                        {
                            ContentType = "image/jpg",
                            ContentUrl = uri,
                            Name = "photo.jpg"
                        });

                        await connector.Conversations.ReplyToActivityAsync(photoMessage);
                    }

                    conversationData.SetProperty<ElectricityBotState>("state", ElectricityBotState.Default);
                    await stateClient.BotState.SetConversationDataAsync(activity.ChannelId, activity.Conversation.Id, conversationData);
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}