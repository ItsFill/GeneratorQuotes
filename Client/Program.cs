using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);
            var client = new TcpClient();
            await client.ConnectAsync(ipEndPoint);
            using var stream = client.GetStream();

            bool isTrue = true;
            
            while (isTrue)
            {
                string isRequest;
        
                Console.WriteLine("Do you want to get quote?\nType 'exit' if you don't want to");
                isRequest = Console.ReadLine();
                
        
                byte[] buffer = Encoding.UTF8.GetBytes(isRequest);

                try
                {
                    await stream.WriteAsync(buffer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("OOPS! Something went terribly wrong:" + ex.Message);
                }
                
                if (isRequest == "exit")
                {
                    isTrue = false;
                    break;
                }
            
                buffer = new byte[1024];
                int byteRead = await stream.ReadAsync(buffer, 0 , buffer.Length);
                string quote = Encoding.UTF8.GetString(buffer, 0, byteRead);
                
                Console.WriteLine("\n" + quote + "\n");
                await Task.Delay(1000);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Cannot connect to the server");
            throw;
        }
    }
}