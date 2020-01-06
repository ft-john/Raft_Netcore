using FtJohn.Raft.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FtJohn.Raft.Peers
{
    public class P2pClient
    {
        private UdpClient client;
        private bool isRunning = false;
        private int bufferSize = 1024; //1KB
        private Queue<CommandQueueItem> sendCommandQueue;
        private Dictionary<string, List<byte>> receivedMessageBuffer;

        public Action<P2PState> DataReceived;

        public P2pClient()
        {
            this.sendCommandQueue = new Queue<CommandQueueItem>();
            this.receivedMessageBuffer = new Dictionary<string, List<byte>>();
        }

        public void Start(string ipString, int port)
        {
            this.isRunning = true;
            IPAddress address;

            if (string.IsNullOrWhiteSpace(ipString) || !IPAddress.TryParse(ipString, out address))
            {
                address = IPAddress.Any;
            }


            IPEndPoint ep = new IPEndPoint(address, port);

            this.client = new UdpClient(ep);
            this.client.Client.SendBufferSize = bufferSize;
            this.client.Client.ReceiveBufferSize = bufferSize;
            this.client.BeginReceive(receiveDataAsync, null);

            this.startSendCommand();
        }

        public void Stop()
        {
            this.isRunning = false;
            this.client.Close();
        }

        public void SendCommand(CommandQueueItem item)
        {
            this.sendCommandQueue.Enqueue(item);
        }

        private void receiveDataAsync(IAsyncResult ar)
        {
            IPEndPoint remote = null;
            byte[] buffer = null;

            try
            {
                buffer = client.EndReceive(ar, ref remote);

                var prefix = new byte[4];
                var suffix = new byte[4];
                bool isBufferEnd = false;
                var key = remote.Address + ":" + remote.Port;

                if (buffer.Length > 4)
                {
                    Array.Copy(buffer, 0, prefix, 0, 4);
                    Array.Copy(buffer, buffer.Length - 4, suffix, 0, 4);

                    if (!this.receivedMessageBuffer.ContainsKey(key))
                    {
                        this.receivedMessageBuffer.Add(key, new List<byte>());
                    }

                    //first data package
                    if (P2pCommand.BytesEquals(P2pCommand.DefaultPrefixBytes, prefix))
                    {
                        this.receivedMessageBuffer[key] = new List<byte>();
                        this.receivedMessageBuffer[key].AddRange(buffer);

                        //last data package
                        if (P2pCommand.BytesEquals(P2pCommand.DefaultSuffixBytes, suffix))
                        {
                            isBufferEnd = true;
                        }
                        else
                        {

                        }
                    }
                    else if (P2pCommand.BytesEquals(P2pCommand.DefaultSuffixBytes, suffix))
                    {
                        this.receivedMessageBuffer[key].AddRange(buffer);
                        isBufferEnd = true;
                    }
                    //other data package
                    else
                    {
                        this.receivedMessageBuffer[key].AddRange(buffer);
                    }
                }
                else
                {
                    this.receivedMessageBuffer[key].AddRange(buffer);
                    isBufferEnd = true;
                }

                if (isBufferEnd)
                {
                    var command = P2pCommand.ConvertBytesToMessage(this.receivedMessageBuffer[key].ToArray());
                    P2PState state = new P2PState();
                    state.IP = remote.Address.ToString();
                    state.Port = remote.Port;
                    state.Command = command;

                    if (command != null)
                    {
                        raiseDataReceived(state);
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Error(ex.Message, ex);
                //raiseOtherException(null);
                LogHelper.Info(ex.ToString());
            }
            finally
            {
                if (this.isRunning && this.client != null)
                {
                    client.BeginReceive(receiveDataAsync, null);
                }
            }
        }
        private void startSendCommand()
        {
            while (this.client != null)
            {
                if (this.sendCommandQueue.Count > 0)
                {
                    var item = this.sendCommandQueue.Dequeue();

                    if (item != null)
                    {
                        try
                        {
                            var index = 0;
                            var data = item.Command.GetBytes();

                            while (index < data.Length)
                            {
                                byte[] buffer;

                                if (data.Length > index + this.bufferSize)
                                {
                                    buffer = new byte[this.bufferSize];
                                }
                                else
                                {
                                    buffer = new byte[data.Length - index];
                                }

                                Array.Copy(data, index, buffer, 0, buffer.Length);
                                this.client.BeginSend(buffer, buffer.Length, item.IP, item.Port, this.sendCallback, null);
                                index += buffer.Length;
                                //Thread.Sleep(100);
                            }
                        }
                        catch (Exception)
                        {
                            //raiseOtherException(null);
                        }
                    }

                }
                else
                {
                    //Thread.Sleep(300);
                }

                if (!this.isRunning)
                {
                    break;
                }
            }
        }
        private void sendCallback(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                try
                {
                    this.client.EndSend(ar);
                }
                catch (Exception)
                {
                }
            }

        }
        private void raiseDataReceived(P2PState state)
        {
            if (DataReceived != null)
            {
                DataReceived(state);
            }
        }
    }
}
