using FtJohn.Raft.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Raft.Peers
{
    public interface IPeerManager
    {
        void SystemInit();
        PeerInfo GetCurrentPeerInfo();
        List<PeerInfo> GetAllPeers();
        CurentState GetCurrentState();
        void SaveCurrentState(CurentState state);
    }
}
