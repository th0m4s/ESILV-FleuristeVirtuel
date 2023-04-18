using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Utils
{
    internal class NameSecurity
    {
        public static bool CheckMySqlName(string str)
        {
            return str.ToLower().All(x => (x >= 'a' && x <= 'z') || "-_".Contains(x));
        }
    }
}
