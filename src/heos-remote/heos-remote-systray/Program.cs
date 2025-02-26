using CommandLine;
using CommandLine.Text;
using heos_remote_lib;
using System.Reflection;

namespace heos_remote_systray
{
    internal static class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        public static void RegisterGUIConsoleWriter()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task<int> Main(string[] args)
        {
            await Task.Yield();

            RegisterGUIConsoleWriter();

            // parse options (do not care about errors)
            var parser = new Parser(config => {
                config.HelpWriter = null;
            });
            var result = parser.ParseArguments<HeosAppOptions>(args);                            

            if (result.Value == null)
            {
                var ver = System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "(unknown)";
                Console.WriteLine(HelpText.AutoBuild(result, h => {
                    h.AdditionalNewLineAfterOption = false;
                    h.Heading = "heos-remote-systray" + ver; //change header
                    h.Copyright = "(C) 2025 by Michael Hoffmeister. MIT license."; //change copyright text
                    return h;
                }));
                return -1;
            }

            OptionsSingleton.Curr = result.Value;

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            // Application.Run(new Form1());
            Application.Run(new HeosCustomApplicationContext());

            return 0;
        }
    }
}