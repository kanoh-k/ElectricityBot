using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace ElectricityBot
{
    public class ElectricityUsageAPI
    {
        // 2016/9/30日をもって電力使用状況APIの提供が終了しました。
        // 残念ながら、現在はこの関数を実行しても常に null を返します。。。
        public static int? GetElectricityUsage(string Area)
        {
            int? result = null;
            try
            {
                var requestUrl = @"http://setsuden.yahooapis.jp/v1/Setsuden/latestPowerUsage?appid=<YourAppID>&area=" + Area + @"&output=json";
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

        public static string GetAreaFromCompanyEntity(string company)
        {
            if (company.Contains("北海道") || company.Contains("北電"))
            {
                return "hokkaido";
            }
            else if (company.Contains("東北"))
            {
                return "tohoku";
            }
            else if (company.Contains("東京") || company.Contains("東電"))
            {
                return "tokyo";
            }
            else if (company.Contains("中部") || company.Contains("中電"))
            {
                return "chubu";
            }
            else if (company.Contains("関西") || company.Contains("関電"))
            {
                return "kansai";
            }
            else if (company.Contains("九州") || company.Contains("九電"))
            {
                return "kyushu";
            }
            return null;
        }
    }

    public class PhotoSearchAPI
    {
        public static string GetPhotoByKeyword(string keyword)
        {
            string result = null;
            try
            {
                var requestUrl = @"http://shinsai.yahooapis.jp/v1/Archive/search?appid=<YourAppID>&area=" + keyword + @"&output=json&results=1";
                System.Console.WriteLine(requestUrl);
                var request = WebRequest.Create(requestUrl);
                var response = request.GetResponse();
                var rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var json = JObject.Parse(rawJson);
                result = (string)json["ArchiveData"]["Result"][0]["PhotoData"]["OriginalUrl"];
            }
            catch
            {
                // Do nothing.
            }
            return result;
        }
    }
}
