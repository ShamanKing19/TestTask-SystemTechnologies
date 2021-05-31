using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossesClass
{
    class ManageLink
    {
        public int Chief_id;
        public int Subordinate_id;

        public ManageLink() { }
        public ManageLink(int Chief_id, int Subordinate_id) 
        {
            this.Chief_id = Chief_id;
            this.Subordinate_id = Subordinate_id;
        }
    }
}
