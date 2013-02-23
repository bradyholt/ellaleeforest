using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PetaPoco;
using bude.server;
using System.Configuration;

namespace TransactionsFetcher.Test
{
    [TestClass]
    public class AccountTest
    {
        [TestMethod]
        public void TestFetch()
        {
            var db = new PetaPoco.Database(ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ConnectionString, ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ProviderName);
            Account acct = db.SingleOrDefault<Account>(1);
            Assert.IsNotNull(acct);
            List<Account> accts = db.Fetch<Account, Bank>(string.Empty);
        }
    }
}
