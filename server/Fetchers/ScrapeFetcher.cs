using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace bude.server
{
    public class ScrapeFetcher : IFetchOfx
    {
        private static NLog.Logger s_logger = NLog.LogManager.GetLogger("*");

        private Account m_Account;
        public ScrapeFetcher(Account acct)
        {
            m_Account = acct;
        }

        public string Request { get; protected set; }
        public string Response { get; protected set; }

        public OfxAccount FetchOfx(DateTime fromDateUtc, DateTime toDateUtc)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string casperJsPath = Path.Combine(assemblyPath, "casperjs");
            string scriptsPath = Path.Combine(assemblyPath, "casperjs/scripts");
            string outputPath = Path.Combine(assemblyPath, "casperjs/output");
            string workingOutputPath = Path.Combine(outputPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(workingOutputPath);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = ConfigurationManager.AppSettings["phantomJsPath"];
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.WorkingDirectory = casperJsPath;
            startInfo.Arguments = Path.Combine(scriptsPath, string.Format("{0}.js", m_Account.Bank.ofx_uri));
            startInfo.Arguments += " --username=" + m_Account.ofx_username;
            startInfo.Arguments += " --password=" + m_Account.ofx_password;
            startInfo.Arguments += " --accountnumber=" + m_Account.ofx_account_number;
            startInfo.Arguments += " --fromdate=" + fromDateUtc.ToShortDateString();
            startInfo.Arguments += " --todate=" + toDateUtc.ToShortDateString();
            startInfo.Arguments += " --securityquestionanswer1=" + string.Empty;
            startInfo.Arguments += " --securityquestionanswer2=" + string.Empty;
            startInfo.Arguments += " --securityquestionanswer3=" + string.Empty;
            startInfo.Arguments += " --securitycode=" + m_Account.ofx_security_code;
            startInfo.Arguments += " --outputpath=" + workingOutputPath;

            if (ConfigurationManager.AppSettings["phantomJsDebugMode"] == "true")
            {
                startInfo.Arguments += " --debug=true";

                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
            }

            this.Request = string.Format("{0} {1}", startInfo.FileName, startInfo.Arguments);

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = new Process())
                {
                    exeProcess.StartInfo = startInfo;
                    exeProcess.OutputDataReceived += new DataReceivedEventHandler(exeProcess_OutputDataReceived);
                    exeProcess.ErrorDataReceived += new DataReceivedEventHandler(exeProcess_ErrorDataReceived);

                    s_logger.Info("Starting phantomjs process: {0} {1}", startInfo.FileName, startInfo.Arguments);
                    exeProcess.Start();

                    exeProcess.BeginOutputReadLine();
                    exeProcess.BeginErrorReadLine();
                    exeProcess.WaitForExit();
                }

                /* FILE TYPES:
                 *      transactions.[ofx|qif|json|xml]
                 *      balance-last-statement.txt - Used as a baseline for computing the Ledger balance
                 */

                string[] outputFiles = Directory.GetFiles(workingOutputPath);
                Dictionary<string, string> outputFilesByName = outputFiles.ToDictionary(f => Path.GetFileNameWithoutExtension(f), f => f);

                OfxAccount ofxAcct = null;

                if (outputFilesByName.ContainsKey("error-security-code-needed"))
                {
                    this.Response = File.ReadAllText(outputFilesByName["error-security-code-needed"]);
                    throw new FetchException(this.Response);
                }
                else if (outputFilesByName.ContainsKey("error-invalid-security-code"))
                {
                    this.Response = File.ReadAllText(outputFilesByName["error-invalid-security-code"]);
                    throw new FetchException(this.Response);
                }
                else if (outputFilesByName.ContainsKey("error-account-not-found"))
                {
                    this.Response = File.ReadAllText(outputFilesByName["error-account-not-found"]);
                    throw new FetchException(this.Response);
                }
                else
                {
                    if (outputFilesByName.ContainsKey("transactions"))
                    {
                        string transactionsFile = outputFilesByName["transactions"];
                        string transactionFileExtension = Path.GetExtension(transactionsFile);
                        switch (transactionFileExtension)
                        {
                            case ".ofx":
                                this.Response = File.ReadAllText(transactionsFile);
                                OfxParser parser = new OfxParser();
                                ofxAcct = parser.LoadAccount(this.Response);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }

                    if (outputFilesByName.ContainsKey("last-statement-balance"))
                    {
                        //if last-statement-balance file exists, we will use enclosed
                        //ledger balance as baseline and then recalculate the LedgerBalanceAmount
                        //with fetched transactions.  this is necessary for some banks (like Chase Credit) where 
                        //the LedgerBalanceAmount is unreliable.

                        string balanceFile = outputFilesByName["last-statement-balance"];
                        string lastStatementBalanceText = File.ReadAllText(balanceFile);
                        decimal lastStatementBalance = decimal.Parse(lastStatementBalanceText);
                        decimal transactionTotal = ofxAcct.Transactions.Sum(t => t.Amount);
                        ofxAcct.LedgerBalanceAmount = lastStatementBalance + transactionTotal;
                        ofxAcct.LedgerBalanceAsOfDate = toDateUtc;
                    }
                }

                List<OfxTransaction> filteredTransactions = new List<OfxTransaction>();
                foreach (OfxTransaction transaction in ofxAcct.Transactions)
                {
                    if (transaction.Date.Date >= fromDateUtc.Date && transaction.Date <= toDateUtc.Date)
                    {
                        filteredTransactions.Add(transaction);
                    }
                }

                ofxAcct.Transactions = filteredTransactions;

                return ofxAcct;

            }
            finally
            {
                if (ConfigurationManager.AppSettings["phantomJsDebugMode"] != "true")
                {
                    Directory.Delete(workingOutputPath);
                }
            }
        }

        void exeProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            s_logger.Error(e.Data);
        }

        void exeProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            s_logger.Info(e.Data);
        }
    }
}
