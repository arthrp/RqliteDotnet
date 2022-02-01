using RqliteDotnet;

namespace RqliteDotnetExample;

public static class RqliteDotnetExample
{
    public static void Main(string[] args)
    {
        var x = new RqliteClient("http://localhost:4001");
        Console.WriteLine(x.Ping().Result);

        var r = x.Query("select * from foo").Result;
        var r1 = x.Query<FooDto>("select * from foo").Result;
    }
}

public class FooDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}