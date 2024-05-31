using System.Drawing;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;

namespace GithubTrendsScraper.Common
{

    public  class TranslationChinese
    {
        //APP ID
        public static string appId = "20210420000791175";
        //密钥
        public static string secretKey = "xhRL6tzMl5acjsZHaapC";

        public string GetChineseTranslation(string descText)
        {
            int n = new Random().Next(1000, 3500);
            Task.Delay(n).Wait();
            var unescapeRetString = GetUnescapeRetString(descText, "en", "zh");
            Console.WriteLine($"{unescapeRetString}");
            if (string.IsNullOrWhiteSpace(unescapeRetString)) 
            {
                return "";
            }
            var amode = JsonConvert.DeserializeObject<TranslationResult>(unescapeRetString);
            return amode?.trans_result.FirstOrDefault()?.dst ?? "";
        }

        /// <summary>
        /// 翻译 中文(zh) 英语(en) 繁体中文(cht)
        /// </summary>
        /// <param name="text">原文</param>
        /// <param name="from">源语言</param>
        /// <param name="to">目标语言</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string GetUnescapeRetString(string text, string from, string to)
        {
            try
            {
                Random rd = new Random();
                string salt = rd.Next(100000).ToString();

                string sign = EncryptString(appId + text + salt + secretKey);
                string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
                url += "q=" + HttpUtility.UrlEncode(text);
                url += "&from=" + from;
                url += "&to=" + to;
                url += "&appid=" + appId;
                url += "&salt=" + salt;
                url += "&sign=" + sign;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "text/html";
                request.UserAgent = null;
                request.Timeout = 6000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream);
                string retString = myStreamReader.ReadToEnd();
                var UnescapeRetString = Regex.Unescape(retString);
                myStreamReader.Close();
                myResponseStream.Close();
                return UnescapeRetString;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        // 计算MD5值
        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }

        /// <summary>
        /// 接收百度翻译API结果的实体类
        /// </summary>

        public class TranslationResult
        {
            public string from { get; set; }
            public string to { get; set; }
            public Trans_Result[] trans_result { get; set; }
        }

        public class Trans_Result
        {
            public string src { get; set; }
            public string dst { get; set; }
        }

        public class TranslationErrorResult
        {
            public string error_code { get; set; }
            public string error_msg { get; set; }
        }
    }

}
