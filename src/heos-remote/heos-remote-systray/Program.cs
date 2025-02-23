using CommandLine;
using heos_remote_lib;

namespace heos_remote_systray
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task<int> Main(string[] args)
        {
            Task.Yield();

            // parse options (do not care about errors)
            var result = Parser.Default.ParseArguments<HeosAppOptions>(args);
            if (result.Value == null)
                return -1;

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