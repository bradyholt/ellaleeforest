using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    [PetaPoco.TableName("users")]
    public class User : DataModel
    {
        public string encrypted_password { get; set; }
        public string email { get; set; }
    }
}
