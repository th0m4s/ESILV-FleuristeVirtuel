using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Types
{
    public class TAdresse : DbRecord
    {
        [PrimaryKey]
        [DbColumn]
        public uint id_adresse { get; private set; }

        [DbColumn]
        public uint numero_rue { get; set; }

        [DbColumn]
        public string? nom_rue { get; set; }

        [DbColumn]
        public uint code_postal { get; set; }

        [DbColumn]
        public string? ville { get; set; }

        public override string ToString()
        {
            return $"{numero_rue} {nom_rue}, {code_postal} {ville}";
        }
    }
}
