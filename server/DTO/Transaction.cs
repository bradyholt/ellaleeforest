using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    [PetaPoco.TableName("transactions")]
    public class Transaction : DataModel
    {
        public Transaction()
        {
            this.created_at = DateTime.UtcNow;
        }

        public int? account_id { get; set; }
        public int user_id { get; set; }
        public int? envelope_id { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public decimal amount { get; set; }
        public DateTime date { get; set; }
        public int? parent_transaction_id { get; set; }
        public int? transaction_imports_id { get; set; }
        public string ofx_transaction_id { get; set; }
    }
}
