using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    [PetaPoco.TableName("envelope_templates")]
    public class EnvelopeTemplate : DataModel
    {
        public int user_id { get; set; }
        public string name { get; set; }
    }
}
