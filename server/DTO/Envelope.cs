using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    [PetaPoco.TableName("envelopes")]
    public class Envelope : DataModel
    {
        public int user_id { get; set; }
        public int envelope_group_id { get; set; }
        public string name { get; set; }
    }
}
