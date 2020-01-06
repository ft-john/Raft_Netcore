using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdjCase.JsonRpc.Client;
using EdjCase.JsonRpc.Core;
using EdjCase.JsonRpc.Router;
using EdjCase.JsonRpc.Router.Abstractions;
using FtJohn.Raft;
using FtJohn.Raft.Framework;
using FtJohn.Raft.State;
using FtJohn.Business;
using FtJohn.Business.Entities;
using FtJohn.Business.Verify;
using FtJohn.Services.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FtJohn.Services.Controllers
{
    public class DefaultController : RpcController
    {
        public async Task<IRpcMethodResult> SubmitNewLog(LogIM logIM)
        {
            try
            {
                if (Startup.CurrentPeer.CurrentState.State != EnumState.Leader)
                {
                    if (Startup.CurrentPeer.CurrentState.VotedFor != null)
                    {
                        var leaderPeer = Startup.CurrentPeer.Peers.Where(p => p.Id == Startup.CurrentPeer.CurrentState.VotedFor).FirstOrDefault();

                        if (leaderPeer == null || leaderPeer.Id == Startup.CurrentPeer.CurrentState.Id)
                        {
                            return Error(500, "Server error");
                        }
                        else
                        {
                            //return Error(302, "Please send request to LEADER node", leaderPeer.ApiUri);
                            var result = this.SendHttpPostRequest(logIM, "SubmitNewLog", leaderPeer.ApiUri);

                            if (!result.HasError)
                            {
                                return Ok(JsonConvert.DeserializeObject(result.Result.ToString()));
                            }
                            else
                            {
                                return Error(result.Error.Code, result.Error.Message);
                            }
                        }
                    }

                    return Error(404, "Can not found leader node");
                }
                else
                {
                    var soruce = $"{logIM.Category},{logIM.SenderAddress},{logIM.ReceiverAddress},{logIM.LegalCurrency},{logIM.LegalCurrencyAmount},{logIM.CryptoCurrency},{logIM.CryptoCurrencyAmount},{logIM.ExchangeRate},{logIM.TxTimestamp},{logIM.TxHash}";
                    var address = FiiiCoinAddress.CreateAccountAddress(Base16.Decode(logIM.PublicKey));
                    if(new TrustedClientManager().GetByAddress(address) == null)
                    {
                        throw new Exception("The public key is not trusted by server");
                    }
                    else if(!Signature.Verify(logIM.PublicKey, logIM.Signature, soruce))
                    {
                        throw new Exception("Signature is invalid");
                    }

                    var logManager = new LogManager();

                    var log = new Log();
                    log.Category = logIM.Category;
                    log.SenderAddress = Convert.ToString(logIM.SenderAddress);
                    log.ReceiverAddress = Convert.ToString(logIM.ReceiverAddress);
                    log.LegalCurrency = Convert.ToString(logIM.LegalCurrency);
                    log.LegalCurrencyAmount = logIM.LegalCurrencyAmount;
                    log.CryptoCurrency = Convert.ToString(logIM.CryptoCurrency);
                    log.CryptoCurrencyAmount = logIM.CryptoCurrencyAmount;
                    log.ExchangeRate = logIM.ExchangeRate;
                    log.TxTimestamp = logIM.TxTimestamp;
                    log.TxHash = Convert.ToString(logIM.TxHash);

                    log.Id = 0;
                    log.Index = -1;
                    log.Hash = Base16.Encode(HashHelper.EmptyHash()); 
                    log.PrevHash = Base16.Encode(HashHelper.EmptyHash());
                    log.Term = -1;
                    log.Commited = false;
                    log.PublicKey = logIM.PublicKey;
                    log.SubmitterAddress = address;
                    log.LeaderId = Startup.CurrentPeer.CurrentState.Id;

                    if (string.IsNullOrWhiteSpace(logIM.Signature))
                    {
                        log.Signature = "";
                        log.SigSize = 0;
                    }
                    else
                    {
                        log.Signature = logIM.Signature;
                        log.SigSize = logIM.Signature.Length / 2;
                    }

                    log.Timestamp = Time.EpochTime;

                    //foreach(var input in log.Inputs)
                    //{
                    //    input.LogHash = log.Hash;
                    //}

                    var hash = Startup.CurrentPeer.SubmitNewLog(log);
                    EnumProcessResult processResult = EnumProcessResult.Wait;
                    var cts = new CancellationTokenSource(5000);

                    await Task.Factory.StartNew(() => {
                        while(!cts.Token.IsCancellationRequested && processResult != EnumProcessResult.Commited && processResult != EnumProcessResult.Failed)
                        {
                                processResult = Startup.CurrentPeer.GetProcessResult(hash);
                        }
                    },cts.Token);

                    var result = new
                    {
                        Hash = hash,
                        Result = processResult
                    };

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message);
            }
        }

        public IRpcMethodResult QueryLogStatus(string logHash)
        {
            try
            {
                if (Startup.CurrentPeer.CurrentState.State != EnumState.Leader)
                {
                    if (Startup.CurrentPeer.CurrentState.VotedFor != null)
                    {
                        var leaderPeer = Startup.CurrentPeer.Peers.Where(p => p.Id == Startup.CurrentPeer.CurrentState.VotedFor).FirstOrDefault();

                        if (leaderPeer == null || leaderPeer.Id == Startup.CurrentPeer.CurrentState.Id)
                        {
                            return Error(500, "Server error");
                        }
                        else
                        {
                            //return Error(302, "Please send request to LEADER node", leaderPeer.ApiUri + "/SubmitNewLog");
                            var result = this.SendHttpPostRequest(logHash, "QueryLogStatus", leaderPeer.ApiUri);

                            if(!result.HasError)
                            {
                                return Ok(JsonConvert.DeserializeObject(result.Result.ToString()));
                            }
                            else
                            {
                                return Error(result.Error.Code, result.Error.Message);
                            }
                        }
                    }

                    throw new Exception("Can not found leader node");
                }
                else
                {
                    var result = Startup.CurrentPeer.GetProcessResult(logHash);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message);
            }
        }

        private RpcResponse SendHttpPostRequest(object data, string methodName, string url)
        {
            var parameters = new object[] { data };
            RpcClient client = new RpcClient(new Uri(url), null);
            RpcRequest request = RpcRequest.WithParameterList(methodName, parameters, "Id1");
            RpcResponse result = client.SendRequestAsync(request).Result;

            return result;
        }
    }
}