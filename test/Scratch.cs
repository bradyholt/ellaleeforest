using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using bude.server;
using System.Configuration;
using PetaPoco;
using System.IO;

namespace TransactionsFetcher.Test
{
    [TestClass]
    public class Scratch
    {
        [TestMethod]
        public void TestFetchChaseCredit()
        {
            Database s_db = new PetaPoco.Database(
           ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ConnectionString.Replace("localhost", "www.veritasclass.com"),
           ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ProviderName);

            TransactionFetcher tf = new TransactionFetcher(s_db);
            tf.FetchAndSaveNewTransactions(6);
        }

        [TestMethod]
        public void TestImportUploadedOfx()
        {
            Database s_db = new PetaPoco.Database(
             ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ConnectionString.Replace("localhost", "www.veritasclass.com"),
             ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ProviderName);

            TransactionImporter importer = new TransactionImporter(s_db);
            importer.ImportAllPending();
        }

        [TestMethod]
        public void ImportOfxFile()
        {
            string ofx = File.ReadAllText(@"E:\Downloads\Activity.ofx");

            TransactionImport ofxImport = new TransactionImport();
            ofxImport.account_id = 6;
            ofxImport.ofx_response = ofx;
            ofxImport.transaction_imports_status_id = (int)TransactionImportsStatusEnum.NOT_IMPORTED;

            Database s_db = new PetaPoco.Database(
              ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ConnectionString.Replace("localhost", "www.geekytidbits.com"),
              ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ProviderName);

            s_db.Save(ofxImport);

            TransactionImporter importer = new TransactionImporter(s_db);
            importer.ImportAllPending();
        }

        [TestMethod]
        public void CheckForMissingChaseTransaction()
        {
            string ofx = File.ReadAllText(@"E:\Downloads\Activity.ofx");
            OfxParser parser = new OfxParser();
            OfxAccount ofxAcct = parser.LoadAccount(ofx);

            Database s_db = new PetaPoco.Database(
               ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ConnectionString.Replace("localhost", "www.geekytidbits.com"),
               ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ProviderName);

            List<bude.server.Transaction> transactions = s_db.Fetch<bude.server.Transaction>("where account_id = 6");

            using (StreamWriter sw = new StreamWriter(@"C:\Users\bholt\Desktop\chase_missing.txt"))
            {
                foreach (var ofxTrans in ofxAcct.Transactions)
                {
                    if (!transactions.Exists(t => t.ofx_transaction_id == ofxTrans.TransactionID))
                    {
                        sw.WriteLine(string.Format("id: {0} | date: {1} | amount: {2} | name: {3}",
                            ofxTrans.TransactionID, ofxTrans.Date.ToShortDateString(), ofxTrans.Amount, ofxTrans.Name));
                    }
                }
            }
        }

        [TestMethod]
        public void ImportEnvelopes()
        {
            var db = new PetaPoco.Database(ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ConnectionString, ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ProviderName);

            List<Envelope> existingEnvelopes = db.FetchAll<Envelope>();
            if (existingEnvelopes != null)
            {
                foreach (Envelope env in existingEnvelopes)
                {
                    db.Delete(env);
                }
            }

            List<EnvelopeGroup> existingEnvelopeGroups = db.FetchAll<EnvelopeGroup>();
            if (existingEnvelopeGroups != null)
            {
                foreach (EnvelopeGroup env in existingEnvelopeGroups)
                {
                    db.Delete(env);
                }
            }

            Dictionary<string, int> groupMappings = new Dictionary<string, int>();
            string groupsFile = AppDomain.CurrentDomain.BaseDirectory + "../../../../TransactionsFetcher.Test/import/envelope-groups.xml";
            XElement xeGrp = XElement.Load(groupsFile);
            var groups = from grp in xeGrp.Elements("envelopegroup")
                         select grp;

            foreach (var grp in groups)
            {
                EnvelopeGroup newGroup = new EnvelopeGroup();
                newGroup.user_id = 1;
                newGroup.name = grp.Element("groupName").Value;
                db.Save(newGroup);
                groupMappings.Add(grp.Attribute("id").Value, newGroup.id);
            }

            EnvelopeGroup otherGroup = new EnvelopeGroup();
            otherGroup.user_id = 1;
            otherGroup.name = "SYSTEM GROUPS";
            db.Save(otherGroup);

            string envelopesFile = AppDomain.CurrentDomain.BaseDirectory + "../../../../TransactionsFetcher.Test/import/envelopes.xml";
            XElement xeEnv = XElement.Load(envelopesFile);
            var envelopes = from envelope in xeEnv.Elements("envelope")
                            select envelope;

            foreach (var envelope in envelopes)
            {
                Envelope newEnvelope = new Envelope();
                if (envelope.Element("groupId") != null && groupMappings.ContainsKey(envelope.Element("groupId").Value))
                {
                    newEnvelope.envelope_group_id = groupMappings[envelope.Element("groupId").Value];
                }
                else
                {
                    newEnvelope.envelope_group_id = otherGroup.id;
                }

                newEnvelope.name = envelope.Element("name").Value;
                db.Save(newEnvelope);
            }
        }

        [TestMethod]
        public void ImportFundingProfiles()
        {
            var db = new PetaPoco.Database(ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ConnectionString, ConfigurationManager.ConnectionStrings["BUDE_PRODUCTION"].ProviderName);
            string file = AppDomain.CurrentDomain.BaseDirectory + "../../../../TransactionsFetcher.Test/import/funding-profile-export.xml";

            List<EnvelopeTemplate> existingTemplates = db.FetchAll<EnvelopeTemplate>();
            if (existingTemplates != null)
            {
                foreach (EnvelopeTemplate tp in existingTemplates)
                {
                    db.Delete(tp);
                }
            }

            EnvelopeTemplate newTemplate = new EnvelopeTemplate();
            newTemplate.user_id = 1;
            newTemplate.name = "Funding Profile";
            db.Save(newTemplate);


            List<Envelope> envelopes = db.FetchAll<Envelope>();
            XElement xe = XElement.Load(file);
            var profileItems = from items in xe.Descendants("fundingprofileenvelope")
                               select items;

            foreach (var item in profileItems)
            {
                EnvelopeTemplateItem newItem = new EnvelopeTemplateItem();
                newItem.envelope_template_id = newTemplate.id;
                newItem.envelope_id = (from env in envelopes
                                       // where env.TEMP_ID == item.Attribute("envelope").Value
                                       select env.id).FirstOrDefault();
                newItem.amount = Convert.ToDecimal(item.Attribute("amount").Value);
                db.Save(newItem);
            }
        }
    }
}
