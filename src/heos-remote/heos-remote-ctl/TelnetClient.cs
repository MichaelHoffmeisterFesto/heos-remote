// generated with ChatGPT
// extended 

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class TelnetClientErrorInfo
{
    public string ErrorMessage = "";
}

public class TelnetClient
{
    private readonly string _host;
    private readonly int _port;

    public TelnetClient(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public async Task<dynamic?> SendCommandAsync(string command)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_host, _port);
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
        using var reader = new StreamReader(stream, Encoding.ASCII);
        stream.ReadTimeout = 2000;

        // Send the command
        await writer.WriteLineAsync(command);

        // Read response
        var sb = new StringBuilder();
        try
        {
            while (true || !reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;
                sb.AppendLine(line);
                if (line?.TrimEnd() == "}")
                    break;
            }
        } catch { }
        var response = sb.ToString();

        if (!response.StartsWith("{"))
        {
            return new TelnetClientErrorInfo() { ErrorMessage = response };
        }

        try
        {
            return JsonConvert.DeserializeObject(response);
        }
        catch (Newtonsoft.Json.JsonException)
        {
            Console.WriteLine("Invalid JSON received.");
            return null;
        }
    }
}
