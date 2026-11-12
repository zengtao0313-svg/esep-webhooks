using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogLine($"Received: {input}");

        dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
        string issueUrl = (string)json.issue.html_url;
        string payload = JsonConvert.SerializeObject(new
        {
            text = $"Issue Created: {issueUrl}"
        });

        using var client = new HttpClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            Environment.GetEnvironmentVariable("SLACK_URL")
        );
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = client.Send(request);
        using var reader = new System.IO.StreamReader(response.Content.ReadAsStream());
        return reader.ReadToEnd();
    }
}