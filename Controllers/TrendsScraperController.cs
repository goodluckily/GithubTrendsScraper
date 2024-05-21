using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using GithubTrendsScraper.Common;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GithubTrendsScraper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrendsScraperController : ControllerBase
    {
        private string baseUrl = "https://github.com/trending";
        public TrendsScraperController()
        {
            baseUrl = "https://github.com/trending";
        }

        [HttpGet("GetTrendingRepositories")]
        public async Task<string> GetTrendingRepositories([FromQuery]string lange= "any", [FromQuery] string daterange= "weekly")
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(lange) && lange != "any")
                {
                    var langeStr = HttpUtility.UrlEncode(lange);
                    baseUrl += $"/{langeStr}";
                }
                if (!string.IsNullOrWhiteSpace(daterange))
                {
                    var daterangeStr = HttpUtility.UrlEncode(daterange);
                    baseUrl += $"?since={daterangeStr}";
                }
                UriBuilder uriBuilder = new UriBuilder(baseUrl);
                string url = uriBuilder.ToString();
                var trendingRepositories = await GetTrendingRepositoriesStr(url);
                var strBuilder = new StringBuilder();

                for (int i = 0; i < trendingRepositories.Count; i++)
                {
                    var item = trendingRepositories[i];
                    var index = i + 1;
                    var str = $"""
                    {index}. **{item.Name}**:
                        - Url：`{item.RepositoryUrl}`。
                        - 描述(En)：{item.Description_CN}。
                        - 描述(Cn)：{item.Description}。
                        - Stars⭐：{item.Stars}。    
                        - Forks🍴：{item.Forks}。
                        - Language💻：{item.Language}。
                    ----------------------------------------
                    """;
                    strBuilder.AppendLine(str);
                }

                var thisDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                SendToEmail.SendEmail("15652338313@163.com", $"GitHub，{thisDate}、趋势信息", strBuilder.ToString());
                //邮件发送
                return await Task.FromResult(strBuilder.ToString());

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<List<Repository>> GetTrendingRepositoriesStr(string url)
        {
            var repositories = new List<Repository>();
            var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(response);

            var repoNodes = htmlDocument.DocumentNode.SelectNodes("//article[@class='Box-row']");

            foreach (var repoNode in repoNodes)
            {
                var repo = new Repository();

                var nodeName = repoNode.ChildNodes.Where(x => x.Name == "h2").FirstOrDefault()?.InnerText?.Trim().Replace("\n", "").Replace(" ", "") ?? "";
                repo.Name = nodeName;
                repo.Description = repoNode.SelectSingleNode("./p")?.InnerText.Trim();
                if (!string.IsNullOrWhiteSpace(repo.Description))
                {
                    var trans = new TranslationChinese();
                    //需要翻译中文 百度翻译
                    repo.Description_CN = trans.GetChineseTranslation(repo.Description);
                }

                var afsafd = repoNode.SelectNodes("h2[@class='h3 lh-condensed']/a");
                var Url = afsafd.FirstOrDefault()?.GetAttributeValue("href", "") ?? string.Empty;
                repo.RepositoryUrl = !string.IsNullOrWhiteSpace(Url) ? "github.com" + Url : "";

                var starsNode = repoNode.SelectSingleNode(".//a[contains(@href, '/stargazers')]");
                repo.Stars = starsNode != null ? starsNode.InnerText.Trim() : "0";

                var forkNode = repoNode.SelectSingleNode(".//a[contains(@href, '/forks')]");
                repo.Forks = forkNode?.InnerText?.Trim().Replace(",", "") ?? "0";

                var languageNode = repoNode.SelectSingleNode(".//span[@itemprop='programmingLanguage']");
                repo.Language = languageNode != null ? languageNode.InnerText.Trim() : "Unknown";
                repositories.Add(repo);
            }

            return repositories;
        }

        public class Repository
        {
            public string Name { get; set; }
            public string RepositoryUrl { get; set; }
            public string Description { get; set; }
            public string Description_CN { get; set; }
            public string Stars { get; set; }
            public string Language { get; set; }
            public string Forks { get; set; }
        }
    }
}
