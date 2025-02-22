namespace heos_remote_ctl
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var device = (await HeosDiscovery.DiscoverItems(firstFriedlyName: "Schlafzimmer", debugLevel: 2)).FirstOrDefault();
            ;
        }
    }
}
