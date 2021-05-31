using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossesClass
{
    class ManageLink
    {
        public string Chief_id { get; set; }
        public string Subordinate_id { get; set; }

        public ManageLink() { }
        public ManageLink(string Chief_id, string Subordinate_id) 
        {
            this.Chief_id = Chief_id;
            this.Subordinate_id = Subordinate_id;
        }
    }
}
