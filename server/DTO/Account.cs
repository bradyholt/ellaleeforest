using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    [PetaPoco.TableName("accounts")]
    public class Account : DataModel
    {
        public int user_id { get; set; }
        public string name { get; set; }
        public string bank_id { get; set; }
        public string account_type { get; set; }
        public string ofx_account_number { get; set; }
        public string ofx_bank_id { get; set; }
        public string ofx_username { get; set; }
        public string ofx_password { get; set; }
        public string ofx_security_code { get; set; }
        public decimal? ofx_balance { get; set; }
        public DateTime? ofx_balance_date { get; set; }
        public DateTime? ofx_last_updated { get; set; }
        public bool ofx_enabled { get; set; }
        public bool ofx_error_last_update { get; set; }
        public string ofx_error_last_update_message { get; set; }

        [PetaPoco.Ignore]
        public Bank Bank { get; set; }

        [PetaPoco.Ignore]
        public List<Transaction> Transactions { get; set; }

        [PetaPoco.Ignore]
        public AccountTypeEnum Type
        {
            get { return (AccountTypeEnum)Enum.Parse(typeof(AccountTypeEnum), this.account_type); }
            set { this.account_type = value.ToString(); }
        }
    }
}
