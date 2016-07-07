using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ElectricityBot
{
    public class ElectricityUsageAPI
    {
        public static int? GetElectricityUsage(string Area)
        {
            int? result = null;
            try
            {
                var requestUrl = @"http://setsuden.yahooapis.jp/v1/Setsuden/latestPowerUsage?appid=<appid>&area=" + Area + @"&output=json";
                System.Console.WriteLine(requestUrl);
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
            var usage = ElectricityUsageAPI.GetElectricityUsage("tokyo");
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
                var usage = ElectricityUsageAPI.GetElectricityUsage("tokyo");
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
    }


    // 本当は、AreaOptionは英語で定義してリソースファイルを追加して日本語にローカライズするのが
    // 正しいはずだが、今回はサボって楽をしています。。
    public enum AreaOption
    {
        北海道電力, 東北電力, 東京電力, 中部電力, 関西電力, 九州電力
    }

    public static class AreaOptionExt
    {
        private static string[] EnglishString = { "hokkaido", "tohoku", "tokyo", "chubu", "kansai", "kyushu" };

        public static string ToEnglishString(this AreaOption Area)
        {
            return EnglishString[(int)Area];
        }
    }

    [Serializable]
    public class ElectricityUsageQuery
    {
        [Describe("電力会社")]
        [Prompt("どの電力会社について知りたいですか？{||}")]
        public AreaOption? Area;

        public static IForm<ElectricityUsageQuery> BuildForm()
        {
            return new FormBuilder<ElectricityUsageQuery>()
                .Field(nameof(Area))
                .Confirm("{Area}の電力使用状況を取得します。よろしいですか？")
                .OnCompletion(ReportUsage)
                .Build();
        }

        private static async Task ReportUsage(IDialogContext context, ElectricityUsageQuery query)
        {
            var usage = ElectricityUsageAPI.GetElectricityUsage(query.Area.GetValueOrDefault().ToEnglishString());
            await context.PostAsync(usage != null ? string.Format("{0}kWです", usage) : "取得できませんでした。。");
            context.PerUserInConversationData.SetValue("state", BotState.Default);
        }
    }
}