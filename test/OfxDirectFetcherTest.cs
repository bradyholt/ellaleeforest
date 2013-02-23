using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using bude.server;

namespace TransactionsFetcher.Test
{
    [TestClass]
    public class OfxDirectFetcherTest
    {
        Account m_acct;

        public static bool CertificateValidator(object sender,
       System.Security.Cryptography.X509Certificates.X509Certificate certificate,
       System.Security.Cryptography.X509Certificates.X509Chain chain,
       System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            //trust all certificates
            return true;
        }

        #region Additional test attributes
        [ClassInitialize()]
        public static void Initialize(TestContext testContext) {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = CertificateValidator;

        }


        [ClassCleanup()]
        public static void Cleanup() { }

        [TestInitialize()]
        public void TestInitialize() {
            m_acct= new Account();
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
        public void TestTransactionRequestChecking()
        {
            m_acct.Type = AccountTypeEnum.CHECKING;
            DirectFetcher fetcher = new DirectFetcher(m_acct);
            OfxRequest request = fetcher.GenerateOFXRequest(DateTime.Now.AddMonths(-1), DateTime.UtcNow);
            string result = fetcher.GenerateRequestBody(request);
            System.Diagnostics.Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("BANKMSGSRQV1"));
        }

        [TestMethod]
        public void TestTransactionRequestCreditCard()
        {
            m_acct.Type = AccountTypeEnum.CREDITCARD;
            DirectFetcher fetcher = new DirectFetcher(m_acct);
            OfxRequest request = fetcher.GenerateOFXRequest(DateTime.Now.AddMonths(-1), DateTime.UtcNow);
            string result = fetcher.GenerateRequestBody(request);
            System.Diagnostics.Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("CREDITCARDMSGSRQV1"));
        }

        [TestMethod]
        public void TestBOACheckingFetchOfx()
        {
            Account acct = new Account();
            acct.ofx_username = "brady.holt";
            acct.ofx_password = "9796BmH9796";
            acct.ofx_account_number = "004778226280";
            acct.account_type = "CHECKING";
            acct.ofx_bank_id = "111000025";
            acct.Bank = new Bank();
            acct.Bank.ofx_fid = "6812";
            acct.Bank.ofx_org = "HAN";
            acct.Bank.ofx_uri = "https://ofx.bankofamerica.com/cgi-forte/fortecgi?servicename=ofx_2-3&pagename=ofx";

            DirectFetcher fetcher = new DirectFetcher(acct);
            OfxRequest request = fetcher.GenerateOFXRequest(DateTime.Now.AddMonths(-1), DateTime.UtcNow);
            System.Diagnostics.Debug.WriteLine(fetcher.GenerateRequestBody(request));
            string result = fetcher.FetchOfx(request);
            System.Diagnostics.Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("<BALAMT>"));
        }

        [TestMethod]
        public void TestChaseCheckingFetchOfx()
        {
            Account acct = new Account();
            acct.ofx_username = "bradyholt1980";
            acct.ofx_password = "Brady4Katie";
            acct.ofx_account_number = "816689624";
            acct.account_type = "CHECKING";
            acct.ofx_bank_id = "111000614";
            acct.Bank = new Bank();
            acct.Bank.ofx_fid = "10898";
            acct.Bank.ofx_org = "B1";
            acct.Bank.ofx_uri = "https://ofx.chase.com";

            DirectFetcher fetcher = new DirectFetcher(acct);
            OfxRequest request = fetcher.GenerateOFXRequest(DateTime.Now.AddMonths(-1), DateTime.UtcNow);
            string result = fetcher.FetchOfx(request);
            System.Diagnostics.Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("<BALAMT>"));
        }

        [TestMethod]
        public void TestVanguardCehckingFetchOfx()
        {
            Account acct = new Account();
            acct.ofx_username = "bradyholt";
            acct.ofx_password = "9796BmH9796";
            acct.ofx_account_number = "76060303";
            acct.account_type = "MONEYMRKT";
           // acct.ofx_bank_id = "111000614";
            acct.Bank = new Bank();
            acct.Bank.ofx_fid = "1358";
            acct.Bank.ofx_org = "The Vanguard Group";
            acct.Bank.ofx_uri = "https://vesnc.vanguard.com/us/OfxDirectConnectServlet";

            DirectFetcher fetcher = new DirectFetcher(acct);
            OfxRequest request = fetcher.GenerateOFXRequest(DateTime.Now.AddMonths(-1), DateTime.UtcNow);
            System.Diagnostics.Debug.Write(fetcher.GenerateRequestBody(request));
            string result = fetcher.FetchOfx(request);
            System.Diagnostics.Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("<BALAMT>"));
        }

        [TestMethod]
        public void TestChaseCreditCardFetchOfx()
        {
            Account acct = new Account();
            acct.ofx_username = "bradyholt1980";
            acct.ofx_password = "Brady4Katie";
            acct.ofx_account_number = "5466574001947985";
            acct.account_type = "CREDITCARD";
            acct.ofx_bank_id = "111000614";
            acct.Bank = new Bank();
            acct.Bank.ofx_fid = "10898";
            acct.Bank.ofx_org = "B1";
            acct.Bank.ofx_uri = "https://ofx.chase.com";

            DirectFetcher fetcher = new DirectFetcher(acct);
            OfxRequest request = fetcher.GenerateOFXRequest(DateTime.Now.AddMonths(-1), DateTime.UtcNow);
            //System.Diagnostics.Debug.WriteLine(fetcher.GenerateRequestBody(m_request));
            string result = fetcher.FetchOfx(request);
            Assert.IsTrue(result.Contains("<BALAMT>"));
        }

        [TestMethod]
        public void TestINGSavingsFetchOfx()
        {
            Account acct = new Account();
            acct.ofx_username = "118791647";
            acct.ofx_password = "Z4KC6XVW9P98";
            acct.ofx_account_number = "149703040";
            acct.account_type = "SAVINGS";
            acct.ofx_bank_id = "031176110";
            acct.Bank = new Bank();
            acct.Bank.ofx_fid = "031176110";
            acct.Bank.ofx_org = "ING DIRECT";
            acct.Bank.ofx_uri = "https://ofx.ingdirect.com/OFX/ofx.html";

            DirectFetcher fetcher = new DirectFetcher(acct);
            OfxRequest request = fetcher.GenerateOFXRequest(DateTime.Now.AddMonths(-1), DateTime.UtcNow);
            string result = fetcher.FetchOfx(request);
            System.Diagnostics.Debug.WriteLine(result);
            Assert.IsTrue(result.Contains("<BALAMT>"));
        }
    }
}
