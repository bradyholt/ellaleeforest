using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using bude.server;

namespace TransactionsFetcher.Test
{
    [TestClass]
    public class OfxParserTest
    {
        [TestMethod]
        public void TestParseCreditCardAccount()
        {
            string ofx = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "../../../../TransactionsFetcher.Test/ofx/credit-card.ofx");
            OfxParser parser = new OfxParser();
            OfxAccount acct = parser.LoadAccount(ofx);
            Assert.IsNotNull(acct);
        }
    }
}
