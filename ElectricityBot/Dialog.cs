using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ElectricityBot
{
    [Serializable]
    public class SimpleDialog : IDialog<object>
    {
        // 会話を始めた時刻
        protected DateTime PrevTime;
        protected bool ResetTime = false;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Message> argument)
        {
            if (ResetTime)
            {
                this.PrevTime = DateTime.Now;
                this.ResetTime = false;
            }
            var message = await argument;
            var usage = GetElectricityUsage();
            PromptDialog.Confirm(
                context,
                ShowUsageAsync,
                "東京電力の電力使用状況を取得します。よろしいですか？",
                "わかりませんでした。「はい」か「いいえ」でお答えください。",
                promptStyle: PromptStyle.None);
        }

        public async Task ShowUsageAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                var usage = GetElectricityUsage();
                await context.PostAsync(usage != null ? string.Format("{0}kWです", usage) : "取得できませんでした。。");
            }
            else
            {
                await context.PostAsync(string.Format("では、やめておきますね。。余談ですが、あなた{0}秒間迷っていましたよ。", (int)(DateTime.Now - this.PrevTime).TotalSeconds));
            }
            context.PerUserInConversationData.SetValue("state", BotState.Default);
            this.ResetTime = true;
            context.Wait(MessageReceivedAsync);
        }

        private int? GetElectricityUsage()
        {
            int? result = null;
            try
            {
                var requestUrl = @"http://setsuden.yahooapis.jp/v1/Setsuden/latestPowerUsage?appid=<appid>&area=tokyo&output=json";
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
    }
}