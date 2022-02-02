using System.Threading.Tasks;
using NUnit.Framework;

namespace RqliteDotnet.Test;

public class RqliteClientTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task BasicQuery_Works()
    {
        var client = HttpClientMock.Get();

        var rqClient = new RqliteClient("http://localhost:6000", client);
        var queryresult = await rqClient.Query("select * from foo");
        
        Assert.AreEqual(1, queryresult.Results.Count);
        Assert.AreEqual(2, queryresult.Results[0].Columns.Count);
        Assert.AreEqual("id", queryresult.Results[0].Columns[0]);
        Assert.AreEqual("name", queryresult.Results[0].Columns[1]);
        Assert.AreEqual(2, queryresult.Results[0].Types.Count);
        Assert.AreEqual(1, queryresult.Results[0].Values.Count);
    }
}