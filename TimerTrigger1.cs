using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function
{
    public class TimerTrigger1
    {
        private readonly ILogger _logger;

        public TimerTrigger1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerTrigger1>();
        }

        [Function("TimerTrigger1")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            _logger.LogInformation($"SLACK_WEB_HOOK: {System.Environment.GetEnvironmentVariable("SLACK_WEB_HOOK", EnvironmentVariableTarget.Process)}");

            var jsonString = await MakeStackOverflowRequest();

            var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonString);

            var newQuestionCount = jsonObject?.items.Count;

            await MakeSlackRequest($"You have {newQuestionCount} question(s) from Stackoverflow");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }

        public static async Task<string> MakeStackOverflowRequest()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (var client = new HttpClient(handler))
            {
                var response = await client.GetAsync($"https://api.stackexchange.com/2.3/search?fromdate=1709251200&order=desc&sort=activity&intitle=rcs&site=stackoverflow");

                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }

        public static async Task<string> MakeSlackRequest(string message)
        {
            using (var client = new HttpClient())
            {
                var slackWebHook = System.Environment.GetEnvironmentVariable("SLACK_WEB_HOOK", EnvironmentVariableTarget.Process);

                var requestData = new StringContent("{'text':'" + message + "'}", Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{slackWebHook}", requestData);

                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
    }
}
