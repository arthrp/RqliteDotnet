using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace RqliteDotnet.Test;

public static class HttpClientMock
{
    private const string BASE_URL = "http://localhost:6000";
    public static HttpClient GetQueryMock()
    {
        var fileContent = GetContents();
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(s => s.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fileContent)
            });
        var client = new HttpClient(handlerMock.Object){ BaseAddress = new Uri(BASE_URL) };

        return client;
    }

    private static string GetContents()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "RqliteDotnet.Test.DbQueryResponse.json";
        
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            string result = reader.ReadToEnd();
            return result;
        }
    }
}