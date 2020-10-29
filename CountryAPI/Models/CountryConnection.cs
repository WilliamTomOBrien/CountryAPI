using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CountryAPI.Models
{
    public class CountryConnection
    {
        // Entity that allows us to create connections between
        // each country object

        public ulong CountryOneID { get; set; }
        public ulong CountryTwoID { get; set; }

    }
}
