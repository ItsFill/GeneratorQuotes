    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.Unicode;
    using Serilog;
    using Serilog.Core;

    class Program
    {
        static List<string> quotes = new List<string>()
        {
            "There are only two hard things in Computer Science: cache invalidation and naming things.",
            "First, solve the problem. Then, write the code.",
            "Experience is the name everyone gives to their mistakes.",
            "In order to be irreplaceable, one must always be different.",
            "Java is to JavaScript what car is to carpet."
        };
        
        static Random random = new Random();

        static async Task HandleClientAsync(TcpClient client)
        {
            await using var stream = client.GetStream();
            string clientIp = client.Client.RemoteEndPoint.ToString();

            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)  break;
                
                string text = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                if (text == "exit")
                {
                    stream.Close();
                    Log.Information($"Client {clientIp} was disconnected");
                    break;
                }
                else
                {
                    int count = random.Next(0, 5);
                    text = quotes[count];
                    buffer = Encoding.ASCII.GetBytes(text);
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    Log.Information($"Client {clientIp} received \"{text}\"");
                }
            }
        }
        
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/server_log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 8888);
            TcpListener Listener = new TcpListener(ipEndPoint);
            try
            {
                Listener.Start();
                while (true)
                {
                    TcpClient client = await Listener.AcceptTcpClientAsync();
                    
                    string clientIp = client.Client.RemoteEndPoint.ToString();
                    
                    Log.Information($"Client connected: {clientIp}");
                    
                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OOPS! Something went terribly wrong:" + ex.Message);
            }
            finally
            {
                Listener.Stop();
            }
        }
    }