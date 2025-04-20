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

        public async Task<dynamic> SendCommandAsync(
            string command,
            bool doNotReceive = false,
            bool readToAnyDelim = false,
            bool readAnyText = false,
            int readTimeout = 20000)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_host, _port);
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.UTF8);
            stream.ReadTimeout = readTimeout;

            // Send the command
            await writer.WriteLineAsync(command);

            // special case?
            if (doNotReceive)
                return "";

            // may loop
            while (true)
            {
                // Read response
                var sb = new StringBuilder();
                int braces = 0;
                try
                {
                    if (readToAnyDelim)
                    {
                        while (!reader.EndOfStream)
                        {
                            var ch = reader.Read();
                            if (ch < 0 || ch == '\r' || ch == '\n')
                                break;
                            sb.Append((char)ch);
                        }
                    }
                    else
                    {
                        while (true || !reader.EndOfStream)
                        {
                            // get a line
                            var line = reader.ReadLine();
                            if (line == null)
                                break;
                            if (line.Trim() == "")
                                continue;
                            sb.AppendLine(line);

                            // investigate the level of opening and closing braces
                            // Note: I was not able to figure out another (better) indication, when
                            // the datagramm was fully received.
                            foreach (var c in line)
                            {
                                if (c == '{') braces++;
                                if (c == '}') braces--;
                            }

                            // this means: the first non-empty line has to contain a '{', or the
                            // algo will directly return!!
                            if (braces == 0)
                                break;
                        }
                    }
                }
                catch (Exception ex) {
                    return new TelnetClientErrorInfo() { ErrorMessage = ex.Message };
                }

                var response = sb.ToString();

                // return with any text?
                if (readAnyText)
                {
                    return response;
                }

                // NO, expecting JSON
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