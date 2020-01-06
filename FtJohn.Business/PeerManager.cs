using FtJohn.Raft.Peers;
using FtJohn.Raft.State;
using FtJohn.Business.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Business
{
    public class PeerManager : IPeerManager
    {
        public void SystemInit()
        {
            DBManager.Initialization();
        }

        public List<PeerInfo> GetAllPeers()
        {
            var dac = new PeerDac();
            return dac.SelectAll();
        }

        public PeerInfo GetCurrentPeerInfo()
        {
            var dac = new PeerDac();
            return dac.SelectLocalPeer();
        }

        public CurentState GetCurrentState()
        {
            var dac = new StateDac();
            var state = dac.SelectCurrentState();

            if(state == null)
            {
                var peerDac = new PeerDac();
                var peerInfo = peerDac.SelectLocalPeer();

                if (peerInfo == null)
                {
                    throw new Exception("Not found local peer information");
                }

                state = new CurentState();
                state.Id = peerInfo.Id;
                state.State = EnumState.Follower;
                state.CurrentTerm = -1;
                state.VotedFor = null;
                state.VotesCount = 0;
                state.CommitIndex = -1;
                state.LastLogIndex = -1;
                state.ElectedTime = null;
            }

            state.State = EnumState.Follower;
            return state;
        }

        public void SaveCurrentState(CurentState state)
        {
            var dac = new StateDac();
            dac.SaveState(state);
        }
    }
}
