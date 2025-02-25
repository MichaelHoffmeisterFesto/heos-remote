// generated with ChatGPT
// extended 

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace heos_remote_lib
{

    public class TelnetClientErrorInfo
    {
        public string ErrorMessage = "";
    }

    public class HeosTelnet
    {
        public const int DefaultPort = 1255;

        private readonly string _host;
        private readonly int _port;

        public HeosTelnet(string host, int port = DefaultPort)
        {
            _host = host;
            _port = port;
        }

        public async Task<dynamic> SendCommandAsync(string command)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_host, _port);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.ASCII);
            stream.ReadTimeout = 20000;

            // Send the command
            await writer.WriteLineAsync(command);

            // may loop
            while (true)
            {

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
                }
                catch { }
                var response = sb.ToString();

                if (!response.StartsWith("{"))
                {
                    return new TelnetClientErrorInfo() { ErrorMessage = response };
                }

                if (response.Contains("command under process"))
                {
                    continue;
                }

                // assume done

                try
                {
                    return JsonConvert.DeserializeObject(response) ?? new object(); 
                }
                catch (Newtonsoft.Json.JsonException)
                {
                    Console.WriteLine("Invalid JSON received.");
                    return new object();
                }
            }
        }

        public static bool IsSuccessCode(dynamic? o)
        {
            return Utils.PropertyExists(o, "heos") && o?.heos.result.ToString() == "success";
        }

        public void Close()
        {
        }
    }
}