using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EdjCase.JsonRpc.Router;
using EdjCase.JsonRpc.Router.Abstractions;
using FtJohn.Business;
using FtJohn.Business.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FtJohn.Services.Controllers
{
    public class ExplorerController : RpcController
    {
        public IRpcMethodResult GetSinceLogs(string hash, int count = 100)
        {
            try
            {
                var logManager = new LogManager();
                var items = logManager.GetSinceLogs(hash, 100);

                return Ok(items);
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetLastLog()
        {
            try
            {
                var logManager = new LogManager();
                var log = logManager.GetLastCommitedLog();
                return Ok(log);
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetLogByHash(string hash)
        {
            try
            {
                var logManager = new LogManager();

                var item = logManager.GetLogByHash(hash);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetLogByIndex(long index)
        {
            try
            {
                var logManager = new LogManager();
                var item = logManager.GetLogByIndex(index);

                return Ok(item);
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message, ex);
            }
        }

        public IRpcMethodResult GetLogsByTerm(long term)
        {
            try
            {
                var logManager = new LogManager();
                var logs = logManager.GetLogsByTerm(term);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return Error(ex.HResult, ex.Message, ex);
            }
        }
    }
}