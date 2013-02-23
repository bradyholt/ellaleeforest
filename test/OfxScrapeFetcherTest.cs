using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using bude.server;

namespace TransactionsFetcher.Test
{
    [TestClass]
    public class OfxScrapeFetcherTest
    {
        Account m_acct;

        #region Additional test attributes
        [ClassInitialize()]
        public static void Initialize(TestContext testContext) { }

        [ClassCleanup()]
        public static void Cleanup() { }

        [TestInitialize()]
        public void TestInitialize()
        {
            m_acct = new Account();
            m_acct.ofx_username = "SDfcdf";
            m_acct.ofx_password = "34343DSD";
            m_acct.ofx_account_number = "004778226280";
            m_acct.account_type = "CHECKING";
            m_acct.ofx_bank_id = "111000025";
            m_acct.Bank = new Bank();
            m_acct.Bank.ofx_fid = "6812";
            m_acct.Bank.ofx_org = "HAN";
            m_acct.Bank.ofx_uri = "https://ofx.bankofamerica.com/cgi-forte/fortecgi?servicename=ofx_2-3&pagename=ofx";
        }

        [TestCleanup()]
        public void TestCleanup() { }

        #endregion

        [TestMethod]
        public void TestOne()
        {
        }
    }
}
