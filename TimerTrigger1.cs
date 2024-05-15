using System;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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
            
            await MakeSlackRequest("Hello from Azure Function");

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }

        public static async Task<string> MakeSlackRequest(string message)
        {
            using (var client = new HttpClient())
            {
                var requestData = new StringContent("{'text':'" + message + "'}", Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"https://hooks.slack.com/services/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", requestData);

                var result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }
    }
}
