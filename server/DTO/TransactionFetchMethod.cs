using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    [PetaPoco.TableName("transaction_fetch_methods")]
    public class TransactionFetchMethod : DataModel
    {
        public string method { get; set; }
    }
}
