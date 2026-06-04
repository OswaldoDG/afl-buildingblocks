namespace ClientPOC;

using ClientPOC.Models;
using ClientPOC.services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

internal class Program
{

    private static string sessionId = string.Empty;

    public static async Task RunSignalR(string url, string token)
    {
        // Build the connection to your SignalR hub
        var connection = new HubConnectionBuilder()
            .WithUrl(url, options =>
            {
                // Attach JWT token to the request
                options.AccessTokenProvider = () => Task.FromResult(token);
            }) // Replace with your hub URL
            .WithAutomaticReconnect()
            .Build();

        try
        {
            // Start the connection
            await connection.StartAsync();
            Console.WriteLine("Connected to SignalR hub.");

            // Handle incoming messages from the hub
            connection.On<string>("SessionEstablished", (message) =>
            {
                sessionId = message;
            });


            // Handle incoming messages from the hub
            connection.On<string>("BatchCompleted", async (message) =>
            {
                await BatchCompleted(message);
            });

            while (true)
            {
                await Task.Delay(250);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting: {ex.Message}");
            Console.ReadLine();
        }
    }

    private static async Task BatchCompleted(string? serializedBatch)
    {
        Console.WriteLine("Batch completed message received");
        Console.WriteLine($"{serializedBatch}");
    }

    public static async Task Main(string[] args)
    {
        // Buidl config from appsettings
        var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory()) // needed for console apps
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddUserSecrets<Program>()
        .AddEnvironmentVariables()
        .Build();

        DateTime now = DateTime.Now;

        // read config values
        var path = configuration["AppSettings:Path"];
        string descopeUser = configuration["AppSettings:DescopeUser"];
        string descopePassword = configuration["AppSettings:DescopePassword"];
        string sorsUrl = configuration["AppSettings:SorsUrl"];


        CloudService cloudService = new CloudService(configuration, new HttpClient());

        var login = new { loginId = descopeUser, password = descopePassword };
        var jwt = cloudService.DescopeLogin("https://api.descope.com/v1/auth/password/signin", login).Result;


        Console.WriteLine("Press enter when API is up and running ...");
        Console.ReadLine();

        Task.Run(() =>  RunSignalR(configuration["AppSettings:SignalREndpoint"], jwt));

        while (string.IsNullOrEmpty(sessionId))
        {
            await Task.Delay(250);
            Console.Write(".");
        }

        Console.WriteLine("\r\nSessionid = " + sessionId);

        while (true)
        {
            await Task.Delay(250);
        }

        //var files = FileService.GetFilesRecursive(path);

        //FileService.LogTimeDiff("Reading files", now);
        //now = DateTime.Now;

        //int id = 0;
        //BatchRequestDto rq = new BatchRequestDto
        //{
        //    Name = "Test Batch + " + DateTime.Now.ToString("yyyyMMddHHmmss"),
        //    DestinationFolder = "/reports/batch-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
        //    Items = []
        //};

        //foreach (var file in files)
        //{
        //    id++;
        //    rq.Items.Add(new BatchItemRequestDto
        //    {
        //        ItemId = id.ToString(),
        //        FileName = Path.GetRelativePath(path, file.FullName).Replace('\\', '/'),
        //        Size = file.Length
        //    });
        //}


        //if (File.Exists("batchResponse.json")) 
        //{ 
        //    File.Delete("batchResponse.json");
        //}

        //Console.WriteLine($"Found files in '{path}': {files.Count}\r\n");
        //Console.Write("Presss a key to continue\r\n");
        //Console.ReadKey();

        //FileService.LogTimeDiff("Create reqeuest", now);
        //now = DateTime.Now;



        //FileService.LogTimeDiff("Descope login", now);
        //now = DateTime.Now;


        //Console.WriteLine($"Descope Login Response success: {!string.IsNullOrEmpty(jwt)}\r\n");
        //BatchResponseDto batchResponse;


        //Console.Write("Creating batch\r\n");
        //if (!File.Exists("batchResponse.json"))
        //{
        //    batchResponse = cloudService.CreateBatch($"{sorsUrl}/batch", jwt, rq).Result;

        //    FileService.LogTimeDiff("Batch creation", now);
        //    now = DateTime.Now;


        //    File.WriteAllText("batchResponse.json", Newtonsoft.Json.JsonConvert.SerializeObject(batchResponse));
        //}
        //else
        //{
        //    batchResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<BatchResponseDto>(File.ReadAllText("batchResponse.json"));
        //}

        //Console.Write($"Batch created {batchResponse.Id}\r\n");
        //Console.Write("Presss a key to continue\r\n");
        //Console.ReadKey();


        //now = DateTime.Now;
        //foreach (var item in rq.Items)
        //{
        //    item.FileName = Path.Combine(path, item.FileName.Replace("/", "\\"));
        //}

        //Console.Write("Uploading files\r\n");
        //await cloudService.UploadToGoogle(batchResponse, rq, 10);

        //FileService.LogTimeDiff("Cloud upload", now);

        //BatchCompleteRequestDto batchComplete = new BatchCompleteRequestDto
        //{
        //    Id = batchResponse.Id,
        //    Items = batchResponse.Items.Select(i => new BatchCompletedEntityDto { Id = i.Id }).ToList()
        //};

        //Console.Write("Completing batch\r\n");
        //await cloudService.CompleteBatch($"{sorsUrl}/batch/complete/{batchResponse.Id}", batchComplete, jwt);
        //Console.ReadKey();
    }
}