using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    [PetaPoco.TableName("transaction_imports")]
    public class TransactionImport : DataModel
    {
        public TransactionImport()
        {
            this.created_at = DateTime.UtcNow;
            this.transaction_imports_status_id = (int)TransactionImportsStatusEnum.NOT_IMPORTED;
        }

        public int account_id { get; set; }
        public int user_id { get; set; }
        public string ofx_request { get; set; }
        public string ofx_response { get; set; }
        public int transaction_imports_status_id { get; set; }
        public string status_detail { get; set; }
    }
}
