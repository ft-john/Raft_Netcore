using FtJohn.Business.Data;
using FtJohn.Business.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FtJohn.Business
{
    public class TrustedClientManager
    {
        public void Add(TrustedClient client)
        {
            new TrustedClientDac().Insert(client);
        }

        public TrustedClient GetByAddress(string address)
        {
            return new TrustedClientDac().SelectByAddress(address);
        }

        public List<TrustedClient> GetAll()
        {
            return new TrustedClientDac().SelectAll();
        }
    }
}
