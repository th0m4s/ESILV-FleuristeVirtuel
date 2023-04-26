using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Types
{
    public class TMagasin : DbRecord
    {
        [PrimaryKey]
        [DbColumn]
        public uint id_magasin { get; private set; }

        [DbColumn]
        public string? nom_magasin { get; set; }

        [DbColumn("id_adresse")]
        public uint id_adresse_localisation { get; set; }

        [ForeignReference("adresse", "id_adresse", "id_adresse_localisation")]
        public TAdresse? adresse_localisation { get; set; }

        public override string ToString()
        {
            return $"{nom_magasin} (#{id_magasin})";
        }

        public new void InsertInto(string tableName, DbConnection connection, bool insertPrimaryKey = false)
        {
            base.InsertInto(tableName, connection, insertPrimaryKey);
            RemplirProduits(connection);
        }

        public void RemplirProduits(DbConnection conn, List<TProduit>? produits = null)
        {
            if (produits == null)
                produits = conn.SelectMultipleRecords<TProduit>("SELECT * FROM produit WHERE produit.id_produit NOT IN " +
                    "(SELECT s.id_produit FROM stock AS s WHERE s.id_magasin = @magasin)", new DbParam("@magasin", id_magasin));

            foreach(TProduit produit in produits)
            {
                TStock stock = DbRecord.CreateEmptyOrGetInstance<TStock>(produit.id_produit, id_magasin);
                stock.quantite_stock = 0;
                stock.InsertInto("stock", conn, true);
            }
        }
    }
}