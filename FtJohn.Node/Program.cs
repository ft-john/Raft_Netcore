using FtJohn.Raft;
using FtJohn.Business;
using FtJohn.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FtJohn.Node
{
    class Program
    {
        static void Main(string[] args)
        {
            int peerPort = int.Parse(Resource.PeerMainnetPort);
            int apiPort = int.Parse(Resource.ApiMainnetPort);
            string ip = "";

            try
            {
                ConfigurationTool tool = new ConfigurationTool();
                Setting setting = tool.GetAppSettings<Setting>("Setting");

                if (setting != null)
                {
                    ip = setting.AdapterIP;
                }
            }
            catch
            {

            }



            if (args.Length > 0 && args[0].ToLower() == "-testnet")
            {
                GlobalParameters.IsTestnet = true;
                peerPort = int.Parse(Resource.PeerTestnetPort);
                apiPort = int.Parse(Resource.ApiTestnetPort);
            }

            Peer peer = new Peer();
            peer.Init(new PeerManager(), new LogManager());
            peer.Start(ip, peerPort);

            Task.Run(() => {
                //Startup.P2PStartAction = BlockchainJob.Current.P2PJob.Start;
                //Startup.P2PStopAction = BlockchainJob.Current.P2PJob.Stop;
                //Startup.P2PBroadcastBlockHeaderAction = BlockchainJob.Current.P2PJob.BroadcastNewBlockMessage;
                //Startup.P2PBroadcastTransactionAction = BlockchainJob.Current.P2PJob.BroadcastNewTransactionMessage;
                //Startup.GetLatestBlockChainInfoFunc = BlockchainJob.Current.P2PJob.GetLatestBlockChainInfo;
                //Startup.EngineStopAction = BlockchainJob.Current.Stop;
                //Startup.GetEngineJobStatusFunc = BlockchainJob.Current.GetJobStatus;
                Startup.CurrentPeer = peer;
                var host = WebHost
                  .CreateDefaultBuilder()
                  //.UseKestrel().ConfigureServices(cssc => cssc.AddMemoryCache())
                  .UseStartup<Startup>()
                  .UseUrls("http://*:" + apiPort)
                  .Build();
                host.Start();
            });
        }
    }
}
