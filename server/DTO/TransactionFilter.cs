using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    [PetaPoco.TableName("transaction_filters")]
    public class TransactionFilter : DataModel
    {
        public int user_id { get; set; }
        public string search_text { get; set; }
        public decimal? amount { get; set; }
        public int envelope_id { get; set; }
    }
}
