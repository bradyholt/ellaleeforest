using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    [PetaPoco.TableName("banks")]
    public class Bank : DataModel
    {
        public string name { get; set; }
        public int transaction_fetch_method_id { get; set; }
        public string ofx_fid { get; set; }
        public string ofx_org { get; set; }
        public string ofx_uri { get; set; }
    }
}
