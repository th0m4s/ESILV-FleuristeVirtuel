using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Types
{
    public class TCommande : DbRecord
    {
        [PrimaryKey]
        [DbColumn]
        public uint id_commande { get; private set; }

        [DbColumn]
        public string? message_accompagnement { get; set; }

        [DbColumn]
        public string? commentaire_commande { get; set; }

        [DbColumn]
        public uint id_magasin { get; set; }

        [DbColumn]
        public float? prix_maximum { get; set; }

        [DbColumn]
        public DateTime? date_commande { get; set; }

        [DbColumn]
        public DateTime? date_livraison_souhaitee { get; set; }

        [DbColumn]
        public string? statut { get; set; }

        [DbColumn]
        public float? prix_avant_reduc { get; set; }

        [DbColumn]
        public uint pourc_reduc_prix { get; set; }

        [DbColumn]
        public uint id_adresse { get; set; }

        [DbColumn]
        public uint id_client { get; set; }

        [ForeignReference("magasin", "id_magasin", "id_magasin")]
        public TMagasin? magasin { get; set; }

        [ForeignReference("adresse", "id_adresse", "id_adresse")]
        public TAdresse? adresse_livraison { get; set; }

        [ForeignReference("client", "id_client", "id_client")]
        public TClient? client { get; set; }

    }
}
