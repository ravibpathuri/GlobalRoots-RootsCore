// RootsCore/Data/Neo4jContext.cs
using Neo4j.Driver;

namespace RootsCore.Data
{
    public class Neo4jContext
    {
        private readonly IDriver _driver;
        public bool IsConnected { get; private set; }

        public Neo4jContext(IConfiguration config)
        {
            var uri = config["Neo4j:ConnectionString"];
            var user = config["Neo4j:Username"];
            var pass = config["Neo4j:Password"];
            int retries = 30, delayMs = 12000;

            while (retries > 0)
            {
                try
                {
                    _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, pass));
                    using var session = _driver.AsyncSession();
                    var result = session.RunAsync("RETURN 1").Result;
                    IsConnected = true;
                    Console.WriteLine("Neo4j connected");
                    return;
                }
                catch (Exception ex)
                {
                    retries--;
                    Console.WriteLine($"Neo4j connect failed: {ex.Message}. Retries: {retries}");
                    if (retries == 0) break;
                    Thread.Sleep(delayMs);
                }
            }
            IsConnected = false;
            Console.WriteLine("Neo4j connection failed. Proceeding without DB.");
        }

        public IAsyncSession Session => IsConnected ? _driver.AsyncSession() : null;
    }
}