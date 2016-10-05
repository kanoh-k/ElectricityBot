using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;


namespace ElectricityBot
{
    [LuisModel("YourModelId", "YourSubscriptionKey")]
    [Serializable]
    public class ElectricityDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            // 意図がわからなかった場合はとりあえず「てへぺろ」しておく
            string message = "てへぺろ！";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Electricity")]
        public async Task Electricity(IDialogContext context, LuisResult result)
        {
            // Companyエンティティを探す
            string area = null;
            foreach (EntityRecommendation entity in result.Entities)
            {
                if (entity.Type == "Company")
                {
                    // "東京 電力"などのCompanyエンティテを"tokyo"というAPIに渡す引数に変換
                    area = ElectricityUsageAPI.GetAreaFromCompanyEntity(entity.Entity.Replace(" ", ""));
                }
            }

            string message;
            if (area != null)
            {
                var usage = ElectricityUsageAPI.GetElectricityUsage(area);
                message = (usage != null) ? string.Format("{0}kWです。", usage) : "取得できませんでした。。";
            }
            else
            {
                // 電力会社がわからなかったら、ユーザーに聞いてBotの状態をRequestCompanyに移行する
                message = "どの電力会社について知りたいですか？";
                context.ConversationData.SetValue<ElectricityBotState>("state", ElectricityBotState.RequestCompany);
            }
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Photo")]
        public async Task Photo(IDialogContext context, LuisResult result)
        {
            string keyword = null;
            foreach (EntityRecommendation entity in result.Entities)
            {
                if (entity.Type == "PhotoKeyword")
                {
                    keyword = entity.Entity.Replace(" ", "");
                }
            }

            if (keyword != null)
            {
                var uri = PhotoSearchAPI.GetPhotoByKeyword(keyword);

                if (uri == null)
                {
                    await context.PostAsync("写真を取得できませんでした。。");
                }
                else
                {
                    // 画像はIMessageActivity.Attachmentsに情報を追加することで送信できる
                    var photoMessage = context.MakeMessage();
                    photoMessage.Text = keyword + "の写真だよ！";
                    photoMessage.Attachments.Add(new Attachment()
                    {
                        ContentType = "image/jpg",
                        ContentUrl = uri,
                        Name = "photo.jpg"
                    });

                    await context.PostAsync(photoMessage);
                }
            }
            else
            {
                context.ConversationData.SetValue<ElectricityBotState>("state", ElectricityBotState.RequestPhotoKeyword);
                await context.PostAsync("見たい写真のキーワードを教えてください。");
            }
            context.Wait(MessageReceived);
        }

        public ElectricityDialog()
        {
        }
        public ElectricityDialog(ILuisService service)
            : base(service)
        {
        }
    }
}