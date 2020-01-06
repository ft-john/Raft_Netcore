using FtJohn.Raft.Framework;
using FtJohn.Raft.Log;
using FtJohn.Raft.Messages;
using FtJohn.Raft.State;
using FtJohn.Raft.Peers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Threading;

namespace FtJohn.Raft
{
    public class Peer
    {
        public CurentState CurrentState { get; set; }
        public List<PeerInfo> Peers;

        private P2pClient client;
        private System.Timers.Timer electionTimer;
        private System.Timers.Timer heartbeatTimer;
        private IPeerManager peerManager;
        private ILogManager logManager;
        private Random random;
        private int minInterval = 3000;
        private int maxInterval = 10000;
        private int heartbeatInterval = 1000;

        private bool isRunning = false;
        private bool electioneering = false;
        private bool logRepicating = false;
        private ILog currentLog = null;
        private bool startReceiveInstallSnapshot = false;
        
        private List<string> votedPeers = new List<string>();
        private List<string> logConfirmedPeers;
        private Dictionary<string, EnumProcessResult> logProcessResult;

        private static object objlock = new object();
        public Peer()
        {
            random = new Random();
            this.Peers = new List<PeerInfo>();
            this.logConfirmedPeers = new List<string>();
            this.logProcessResult = new Dictionary<string, EnumProcessResult>();

            heartbeatTimer = new System.Timers.Timer();
            heartbeatTimer.Interval = heartbeatInterval;
            heartbeatTimer.AutoReset = true;
            heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;

            electionTimer = new System.Timers.Timer();
            electionTimer.AutoReset = true;
            electionTimer.Elapsed += ElectionTimer_Elapsed;

            this.client = new P2pClient();
            this.client.DataReceived = this.dataReceived;
        }

        public void Init(IPeerManager peerMgr, ILogManager logMgr)
        {
            LogHelper.Info("Peer Initialize");
            //Load All peers
            this.peerManager = peerMgr;
            this.logManager = logMgr;

            this.peerManager.SystemInit();
            //init peers and currentState
            this.Peers = this.peerManager.GetAllPeers();
            this.CurrentState = this.peerManager.GetCurrentState();

            if(this.CurrentState.CommitIndex == -1)
            {
                this.CurrentState.CommitIndex = this.logManager.GetLastCommitIndex();
                this.CurrentState.LastLogIndex = this.logManager.GetLastLogIndex();
                this.peerManager.SaveCurrentState(this.CurrentState);
            }
        }

        private void ElectionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.CurrentState.State != EnumState.Leader && !startReceiveInstallSnapshot)
            {
                LogHelper.Info("Election timeout, start to request votes");
                this.ChangeToCandidate();
            }
        }

        private void HeartbeatTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (this.CurrentState.State == EnumState.Leader)
                {
                    if (this.currentLog != null)
                    {
                        if (this.logRepicating)
                        {
                            this.sendAppendEntries(this.currentLog);

                            if (this.logProcessResult.ContainsKey(this.currentLog.Hash))
                            {
                                this.logProcessResult[this.currentLog.Hash] = EnumProcessResult.Replicating;
                            }
                        }
                    }
                    else
                    {
                        this.sendAppendEntries(null);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }

        public void Start(string ip, int port)
        {
            LogHelper.Info("Peer Start");

            this.isRunning = true;

            Thread thread = new Thread(new ThreadStart(() =>
            {
                this.client.Start(ip, port);
            }));

            thread.Start();
            this.newElectionTimeout();
            this.heartbeatTimer.Start();
        }

        public void Stop()
        {
            LogHelper.Info("Peer Stop");

            this.electionTimer.Stop();
            this.heartbeatTimer.Stop();
            this.client.Stop();
        }

        public void ChangeToFollower(long term)
        {
            LogHelper.Info("Peer state changed to FOLLOWER");

            if (term >= this.CurrentState.CurrentTerm)
            {
                this.CurrentState.State = EnumState.Follower;
                this.CurrentState.CurrentTerm = term;
                this.CurrentState.VotedFor = null;
                this.CurrentState.VotesCount = 0;
                this.CurrentState.ElectedTime = null;
                this.electioneering = false;
                this.votedPeers.Clear();

                this.peerManager.SaveCurrentState(this.CurrentState);
            }

            this.newElectionTimeout();
        }

        public void ChangeToCandidate()
        {
            this.CurrentState.State = EnumState.Candidate;
            this.CurrentState.CurrentTerm = this.CurrentState.CurrentTerm + 1;
            this.CurrentState.VotedFor = this.CurrentState.Id;
            this.CurrentState.VotesCount = 1;

            LogHelper.Info(string.Format("Peer state changed to Candidate, start term {0} election", this.CurrentState.CurrentTerm));
            this.electioneering = true;
            this.votedPeers.Clear();
            this.votedPeers.Add(this.CurrentState.Id);

            this.sendRequestVote();
            this.peerManager.SaveCurrentState(this.CurrentState);
            this.newElectionTimeout();
        }

        public void ChangeToLeader()
        {
            LogHelper.Info(string.Format("Peer state changed to LEADER in term {0}, received {1} votes", this.CurrentState.CurrentTerm, this.CurrentState.VotesCount));
            this.electionTimer.Stop();

            this.CurrentState.State = EnumState.Leader;
            this.CurrentState.ElectedTime = Time.EpochTime;
            this.electioneering = false;

            this.peerManager.SaveCurrentState(this.CurrentState);
        }

        public void SetTerm(long term)
        {
            this.CurrentState.CurrentTerm = term;
            this.peerManager.SaveCurrentState(this.CurrentState);
        }

        public string SubmitNewLog(ILog log)
        {
            log.Hash = this.logManager.GetLogHash(log);

            if (logManager.ContainsLogHash(log.Hash))
            {
                throw new Exception("Log has been existed");
            }

            var lastCommitedLog = this.logManager.GetLastCommitedLog();
            log.Index = this.logManager.GetLastCommitIndex() + 1;
            log.Term = this.CurrentState.CurrentTerm;

            if(lastCommitedLog != null)
            {
                log.PrevHash = lastCommitedLog.Hash;
            }

            this.logManager.HandleLog(log);
            this.CurrentState.LastLogIndex = log.Index;

            logRepicating = true;
            this.currentLog = log;
            this.logConfirmedPeers.Clear();
            this.logConfirmedPeers.Add(this.CurrentState.Id);
            this.CurrentState.LastLogIndex = log.Index;

            this.logProcessResult[log.Hash] = EnumProcessResult.Wait;
            return log.Hash;
        }

        public EnumProcessResult GetProcessResult(string logHash)
        {
            if(!this.logProcessResult.ContainsKey(logHash))
            {
                var item = logManager.GetLogByHash(logHash);

                if(item != null)
                {
                    return EnumProcessResult.Commited;
                }
                else
                {
                    return EnumProcessResult.NotFound;
                }
            }
            else
            {
                var result = this.logProcessResult[logHash];

                if(result == EnumProcessResult.Commited || result == EnumProcessResult.Failed)
                {
                    this.logProcessResult.Remove(logHash);
                }

                return result;
            }
        }

        private void newElectionTimeout()
        {
            this.electionTimer.Stop();
            this.electionTimer.Interval = random.Next(minInterval, maxInterval);
            this.electionTimer.Start();
        }

        private void commitCurrentLog()
        {
            logManager.Commit(this.currentLog);

            this.CurrentState.LastLogIndex = this.currentLog.Index;
            this.CurrentState.CommitIndex = this.currentLog.Index;
            this.peerManager.SaveCurrentState(this.CurrentState);

            if (this.logProcessResult.ContainsKey(this.currentLog.Hash))
            {
                this.logProcessResult[this.currentLog.Hash] = EnumProcessResult.Commited;
            }

            LogHelper.Info(this.currentLog.Index + " has been confirmed");
            this.logRepicating = false;
            this.currentLog = null;
            this.logConfirmedPeers.Clear();
        }

        #region data received
        private void dataReceived(P2PState state)
        {
            if(!this.isRunning)
            {
                return;
            }

            var peerInfo = this.Peers.Where(p => p.IP == state.IP && p.Port == state.Port).FirstOrDefault();

            if(peerInfo == null)
            {

                LogHelper.Info(string.Format("Received data from invalid ip address {0}", state.IP));
                return;
            }

            IMessage msg;

            switch (state.Command.CommandName)
            {
                case CommandNames.RequestVote:
                    msg = new RequestVote();
                    msg.Deserialize(state.Command.Payload);
                    this.receivedReqeustVote(peerInfo, (RequestVote)msg);
                    break;
                case CommandNames.VoteResponse:
                    msg = new VoteResponse();
                    msg.Deserialize(state.Command.Payload);
                    this.receivedVoteResponse(peerInfo, (VoteResponse)msg);
                    break;
                case CommandNames.AppendEntries:
                    msg = new AppendEntries();
                    msg.Deserialize(state.Command.Payload);
                    this.receivedAppendEntries(peerInfo, (AppendEntries)msg);
                    break;
                case CommandNames.AppendEntriesResponse:
                    msg = new AppendEntriesResponse();
                    msg.Deserialize(state.Command.Payload);
                    this.receivedAppendEntriesResponse(peerInfo, (AppendEntriesResponse)msg);
                    break;
                case CommandNames.InstallSnapshot:
                    msg = new InstallSnapshot();
                    msg.Deserialize(state.Command.Payload);
                    this.receivedInstallSnapshot(peerInfo, (InstallSnapshot)msg);
                    break;
                case CommandNames.InstallSnapshotResponse:
                    msg = new InstallSnapshotResponse();
                    msg.Deserialize(state.Command.Payload);
                    this.receivedInstallSnapshotResponse(peerInfo, (InstallSnapshotResponse)msg);
                    break;
                default:
                    break;
            }
        }

        private void receivedReqeustVote(PeerInfo peer, RequestVote msg)
        {
            LogHelper.Info(string.Format("Received vote request from {0} Term {1}", peer.IP, msg.Term));
            lock (objlock)
            {
                var response = new VoteResponse();
                response.VoteGranted = false;

                LogHelper.Info($"msg.Term={msg.Term},CurrentTerm={CurrentState.CurrentTerm};VotedFor={CurrentState.VotedFor}");
                if (msg.Term > this.CurrentState.CurrentTerm || (msg.Term == this.CurrentState.CurrentTerm && this.CurrentState.VotedFor == null))
                {
                    if (msg.LastCommitedIndex >= this.logManager.GetLastCommitIndex())
                    {
                        this.CurrentState.CurrentTerm = msg.Term;
                        this.CurrentState.VotedFor = msg.CandidateId;
                        this.ChangeToFollower(msg.Term);

                        response.VoteGranted = true;
                    }
                }

                response.Term = this.CurrentState.CurrentTerm;
                this.sendVoteResponse(peer, response);
            }
        }

        private void receivedVoteResponse(PeerInfo peer, VoteResponse msg)
        {
            LogHelper.Info(string.Format("Received vote response from {0}, vote granted is {1}", peer.IP, msg.VoteGranted));

            if (this.CurrentState.State == EnumState.Candidate || this.CurrentState.State == EnumState.Leader)
            {
                if(msg.VoteGranted && msg.Term == this.CurrentState.CurrentTerm && !this.votedPeers.Contains(peer.Id))
                {
                    this.CurrentState.VotesCount++;
                    this.votedPeers.Add(peer.Id);

                    if(this.electioneering && this.CurrentState.VotesCount > (this.Peers.Count / 2))
                    {
                        this.electioneering = false;
                        this.ChangeToLeader();
                    }
                }
            }
        }

        private void receivedAppendEntries(PeerInfo peer, AppendEntries msg)
        {
            LogHelper.Info(string.Format("Received append entries from {0}", peer.IP));

            var response = new AppendEntriesResponse();
            response.Term = this.CurrentState.CurrentTerm;
            response.LastLogInex = this.logManager.GetLastLogIndex();
            response.CommitIndex = this.logManager.GetLastCommitIndex();
            response.Success = false;

            if (this.CurrentState.CurrentTerm <= msg.Term)
            {
                if (this.CurrentState.State != EnumState.Follower)
                {
                    this.ChangeToFollower(msg.Term);
                }
                else if(this.CurrentState.CurrentTerm < msg.Term)
                {
                    this.SetTerm(msg.Term);
                }
                else
                {
                    this.newElectionTimeout();
                }

                if(this.CurrentState.VotedFor != msg.LeaderId)
                {
                    this.CurrentState.VotedFor = msg.LeaderId;
                }

                if (msg.PrevCommitedIndex < 0 || (this.logManager.ContainsLogIndex(msg.PrevCommitedIndex) && this.logManager.ContainsLogTerm(msg.PrevCommitedTerm)))
                {
                    response.Success = true;

                    if(msg.LogData != null)
                    {
                        int index = 0;
                        var log = logManager.LogDeserialize(msg.LogData, ref index);
                        var item = logManager.GetLogByHash(log.Hash);

                        if (item != null)
                        {
                            logManager.DeleteLogByHash(log.Hash);
                        }

                        this.logManager.HandleLog(log);
                        this.currentLog = log;
                        this.CurrentState.LastLogIndex = log.Index;
                        response.LastLogInex = log.Index;

                        LogHelper.Info("Received a new log");
                    }
                }

                if(this.logManager.GetLastCommitIndex() < msg.LeaderCommit && this.currentLog != null)
                {
                    this.commitCurrentLog();
                    response.CommitIndex = msg.LeaderCommit;
                }
            }

            this.sendAppendEntriesResponse(peer, response);
        }

        private void receivedAppendEntriesResponse(PeerInfo peer, AppendEntriesResponse msg)
        {
            LogHelper.Info(string.Format("Received append entries response from {0}", peer.IP));
            if (this.CurrentState.State == EnumState.Leader)
            {
                if (this.logRepicating && this.currentLog != null)
                {
                    //LogHelper.Info(string.Format("msg.Success: {0}, msg.LastLogInex: {1}, currentLog.Index: {2}, logConfirmedPeers.Contains(peer.Id):{3}", 
                    //    msg.Success, msg.LastLogInex, currentLog.Index, logConfirmedPeers.Contains(peer.Id)));
                    if (msg.Success && msg.LastLogInex == this.currentLog.Index && !this.logConfirmedPeers.Contains(peer.Id))
                    {
                        this.logConfirmedPeers.Add(peer.Id);

                        //LogHelper.Info(string.Format("Received log confirm from {0}, total confirms is {1}", peer.IP, this.logConfirmedPeers.Count));
                        if (this.logConfirmedPeers.Count > (this.Peers.Count / 2))
                        {
                            this.commitCurrentLog();
                            return;
                        }
                    }
                }

                if(msg.Term < logManager.GetLastCommitedTerm() || msg.CommitIndex < this.CurrentState.CommitIndex)
                {
                    LogHelper.Info("Start send install snapshot");
                    this.sendInstallSnapshot(peer, msg.Term, msg.CommitIndex, msg.LastLogInex);
                }
            }
        }

        private void receivedInstallSnapshot(PeerInfo peer, InstallSnapshot snapshot)
        {
            LogHelper.Info("Receive Install Snapshot From " + peer.IP + ":" + peer.Port + ", CurrentState is " + CurrentState.State);
            if(this.CurrentState.State != EnumState.Leader)
            {
                if(!this.startReceiveInstallSnapshot)
                {
                    this.startReceiveInstallSnapshot = true;
                }

                var index = 0;

                while(index < snapshot.Data.Length)
                {
                    var log = logManager.LogDeserialize(snapshot.Data, ref index);

                    if(!logManager.ContainsLogIndex(log.Index))
                    {
                        LogHelper.Info(string.Format("Start to commit log {0}", log.Index));
                        logManager.Commit(log);
                        this.CurrentState.CommitIndex = log.Index;
                        this.CurrentState.LastLogIndex = log.Index;
                    }
                }

                if(snapshot.Done)
                {
                    this.startReceiveInstallSnapshot = false;
                }

                var response = new InstallSnapshotResponse();
                response.Term = this.CurrentState.CurrentTerm;
                response.CommitIndex = this.CurrentState.CommitIndex;
                response.LastLogInex = this.CurrentState.LastLogIndex;

                this.sendInstallSnapshotResponse(peer, response);
            }
        }

        private void receivedInstallSnapshotResponse(PeerInfo peer, InstallSnapshotResponse msg)
        {
            if(this.CurrentState.State == EnumState.Leader)
            {
                if(msg.Term < this.CurrentState.CurrentTerm || msg.CommitIndex < this.logManager.GetLastCommitIndex())
                {
                    this.sendInstallSnapshot(peer, msg.Term, msg.CommitIndex, msg.LastLogInex);
                }
            }
        }
        #endregion

        #region data send
        private void sendRequestVote()
        {
            var msg = new RequestVote();
            msg.Term = this.CurrentState.CurrentTerm;
            msg.CandidateId = this.CurrentState.Id;
            msg.LastCommitedIndex = this.logManager.GetLastCommitIndex();
            msg.LastLogTerm = this.logManager.GetLastCommitedTerm();

            var cmd = P2pCommand.CreateCommand(CommandNames.RequestVote, msg);

            foreach(var peer in this.Peers)
            {
                if(peer.Id != this.CurrentState.Id)
                {
                    var item = new CommandQueueItem(peer.IP, peer.Port, cmd);
                    this.client.SendCommand(item);
                }
            }
        }

        private void sendVoteResponse(PeerInfo peer, VoteResponse msg)
        {
            var cmd = P2pCommand.CreateCommand(CommandNames.VoteResponse, msg);
            var item = new CommandQueueItem(peer.IP, peer.Port, cmd);
            this.client.SendCommand(item);
        }

        private void sendAppendEntries()
        {
            //logRepicating = false;
            //this.currentLog = null;
            //this.logConfirmedPeers.Clear();

            var msg = new AppendEntries();
            msg.Term = this.CurrentState.CurrentTerm;
            msg.LeaderId = this.CurrentState.Id;
            msg.PrevCommitedIndex = this.logManager.GetLastCommitIndex();
            msg.PrevCommitedTerm = this.logManager.GetLastCommitedTerm();
            msg.Offset = 0;
            msg.LogData = null;
            msg.LeaderCommit = this.logManager.GetLastCommitIndex();

            var cmd = P2pCommand.CreateCommand(CommandNames.AppendEntries, msg);

            foreach (var peer in this.Peers)
            {
                if (peer.Id != this.CurrentState.Id)
                {
                    var item = new CommandQueueItem(peer.IP, peer.Port, cmd);
                    this.client.SendCommand(item);

                    LogHelper.Info(string.Format("Send append entries to {0}", peer.IP));
                }
            }
        }

        private void sendAppendEntries(ILog log)
        {
            //this.logConfirmedPeers.Clear();
            //this.logManager.HandleLog(log);
            //this.CurrentState.LastLogIndex = log.Index;

            var msg = new AppendEntries();
            msg.Term = this.CurrentState.CurrentTerm;
            msg.LeaderId = this.CurrentState.Id;
            msg.PrevCommitedIndex = this.logManager.GetLastCommitIndex();
            msg.PrevCommitedTerm = this.logManager.GetLastCommitedTerm();

            if(log != null)
            {
                msg.LogData = logManager.LogSerialize(log);
                msg.Offset = msg.LogData.Length;
            }
            else
            {
                msg.LogData = null;
                msg.Offset = 0;
            }

            msg.LeaderCommit = this.logManager.GetLastCommitIndex();

            var cmd = P2pCommand.CreateCommand(CommandNames.AppendEntries, msg);

            foreach (var peer in this.Peers)
            {
                if (peer.Id != this.CurrentState.Id)
                {
                    var item = new CommandQueueItem(peer.IP, peer.Port, cmd);
                    this.client.SendCommand(item);
                }
            }

            if(log != null)
            {
                LogHelper.Info("Sent a log data to all peers");
            }
        }

        private void sendAppendEntriesResponse(PeerInfo peer, AppendEntriesResponse msg)
        {
            var cmd = P2pCommand.CreateCommand(CommandNames.AppendEntriesResponse, msg);
            var item = new CommandQueueItem(peer.IP, peer.Port, cmd);
            this.client.SendCommand(item);
        }

        private void sendInstallSnapshot(PeerInfo peer, long peerTerm, long peerCommitIndex, long peerLastLogIndex)
        {
            bool done = false;

            if (peerCommitIndex < 0)
            {
                peerCommitIndex = 0;
            }

            LogHelper.Info("peerCommitIndex=" + peerCommitIndex);
            var log = logManager.GetLogByIndex(peerCommitIndex);

            if(log == null)
            {
                return;
            }

            LogHelper.Info("log.Hash=" + log.Hash);
            try
            {
                var items = logManager.GetSinceLogs(log.Hash, 10);

                if (items.Length == 0)
                {
                    return;
                }
                else
                {
                    done = (items[items.Length - 1].Index >= this.logManager.GetLastCommitIndex());
                }

                LogHelper.Info("logs count=" + items.Length);
                var data = new List<byte>();

                foreach (var item in items)
                {
                    data.AddRange(logManager.LogSerialize(item));
                }

                var snapshot = new InstallSnapshot();
                snapshot.Term = this.CurrentState.CurrentTerm;
                snapshot.LeaderId = this.CurrentState.Id;
                snapshot.LastIncludedIndex = items.Length > 0 ? items[0].Index : -1;
                snapshot.LastIncludedTerm = items.Length > 0 ? items[0].Term : -1;
                snapshot.Offset = data.Count;
                snapshot.Data = data.ToArray();
                snapshot.Done = done;

                LogHelper.Info("Send Install Snapshot to " + peer.IP + ":" + peer.Port);
                var cmd = P2pCommand.CreateCommand(CommandNames.InstallSnapshot, snapshot);
                var queueItem = new CommandQueueItem(peer.IP, peer.Port, cmd);
                this.client.SendCommand(queueItem);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }
        }

        private void sendInstallSnapshotResponse(PeerInfo peer, InstallSnapshotResponse msg)
        {
            var cmd = P2pCommand.CreateCommand(CommandNames.InstallSnapshotResponse, msg);
            var item = new CommandQueueItem(peer.IP, peer.Port, cmd);
            this.client.SendCommand(item);
        }
        #endregion
    }
}
