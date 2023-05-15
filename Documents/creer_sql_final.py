import random
import datetime


# COUNTERS
count_addresse = 0
count_client = 0
count_magasin = 0
count_commande = 0


# DATA
addr_villes = [
    (75000, "Paris"), (69000, "Lyon"), (13000, "Marseille"), (31000, "Toulouse"),
    (92400, "Courbevoie"), (92600, "Asnières-sur-Seine")
]
addr_rue_types = ["rue", "avenue", "impasse", "boulevard"]
addr_rue_noms = [
    "de la Paix", "de la Liberté", "de la République", "de la Gare", "de la Mairie",
    "de la Poste", "de la Plage", "de la Croix", "de la Chapelle", "de la Fontaine",
    "de la Source", "des Parisiens", "Léonard de Vinci", "Général de Gaulle", "du Lézard"
]

client_prenom = ["William", "Emma", "Kévin", "Manon", "Léo", "Louis", "Joyce", "Julien", "Oriane",
                 "John", "Lucas", "Clément", "Camille", "Anaïs", "Tom", "Michel", "Gauthier", "Paul"]
client_nom = ["DUPONT", "MARTIN", "BERNARD", "BENOIT", "ROBERT", "DURAND", "PETIT", "ROUX", "MOREAU", "LEFEBVRE"]
client_email_domain = ["gmail.com", "hotmail.fr", "yahoo.fr", "outlook.fr", "laposte.net", "orange.fr", "sfr.fr", "free.fr"]

magasin_nom = ["Pouvoir des fleurs", "Fleur de vie", "Fine fleur", "Vert tige", "La Fabrique à Fleurs"]
random.shuffle(magasin_nom)

fleurs = [
    ("Gerbera", 5), ("Ginger", 4), ("Glaïeul", 2), ("Marguerite", 2.5), ("Rose rouge", 3),
    ("Rose blanche", 3), ("Rose jaune", 3), ("Muguet", 2), ("Jacinthe bleue", 3.25), ("Oiseau du paradis", 6),
    ("Lys", 3.75), ("Jacinthe jaune", 4), ("Genet", 3.25), ("Pensée bleue", 2.5),
    ("Pensée rouge", 2.75), ("Orchidée", 3), ("Tulipe rouge", 3.5), ("Tulipe blanche", 3.5)
]

accessoires = [
    ("Vase", 10), ("Pot", 5), ("Panier", 7), ("Bougie", 3), ("Boîte", 5), ("Verdure", 1), ("Feuille", 0.5)
]

bouquets = [
    ("Gros Merci", 45, "Toute occasion", "Marguerites et verdure, parfait pour remercier un ami", [(4, 6), (3, 3), (19, 1), (13, 5), (24, 4)]),
    ("L'Amoureux", 65, "Saint-Valentin", "Des roses pour l'élu(e) de votre coeur", [(5, 7), (6, 7), (11, 3), (19, 1), (23, 1), (24, 4), (22, 2)]),
    ("L'Exotique", 40, "Toute occasion", "Arrangement de fleurs exotiques dont des oiseaux du paradis", [(2, 4), (10, 5), (19, 1)]),
    ("Maman", 35, "Fête des mères", "Pour remercier votre maman ou un membre de votre famille", [(1, 6), (6, 7), (11, 2), (21, 1)]),
    ("Vive la mariée", 85, "Mariage", "Parfait pour féliciter un couple", [(11, 6), (16, 8), (20, 1), (24, 5), (25, 8), (14, 8)]),
    ("L'Élégant", 50, "Toute occasion", "Bouquet spécialement conçu pour les partenaires commerciaux", [(14, 5), (17, 4), (18, 4), (19, 1), (24, 5), (4, 2)])
]

message_accompagnement = [
    "Bonne fête !", "Joyeux anniversaire !", "Bon rétablissement", "Félicitations !", "Bon courage !"
]


def creer_adresse():
    global count_addresse
    count_addresse += 1
    ville = random.choice(addr_villes)
    return f"({count_addresse}, {random.randint(1, 99)}, '{random.choice(addr_rue_types)} {random.choice(addr_rue_noms)}', {ville[0]}, '{ville[1]}')"


def creer_carte_de_credit():
    return random.choice(["5132", "4985"]) + "".join([str(random.randint(0, 9)) for _ in range(12)])


def creer_telephone():
    return "0" + str(random.randint(1, 9)) + "".join([str(random.randint(0, 9)) for _ in range(8)])


def creer_mot_de_passe():
    return "".join([str(random.randint(0, 9)) for _ in range(8)])


def enlever_accents(txt):
    # il n'y a que ces 2 accents dans les prénoms
    return txt.replace("é", "e").replace("ï", "i")


emails_existants = []


def creer_client():
    global count_client
    count_client += 1

    email = ""
    while email == "" or email in emails_existants:
        nom = random.choice(client_nom)
        prenom = random.choice(client_prenom)
        email = f"{enlever_accents(prenom.lower())}.{enlever_accents(nom.lower())}@{random.choice(client_email_domain)}"

    mdp = creer_mot_de_passe()
    return f"({count_client}, '{nom}', '{prenom}', '{email}', '{creer_telephone()}', '{mdp}', '{creer_carte_de_credit()}', {count_client})"


def creer_magasin():
    global count_magasin
    count_magasin += 1
    return f"({count_magasin}, '{magasin_nom.pop()}', {17 + count_magasin})"


hier = datetime.date.today() - datetime.timedelta(days=1)
debut = hier - datetime.timedelta(days=30 * 7)


ft_date = "%Y-%m-%d %H:%M:%S"


def creer_date():
    delta = hier - debut
    int_delta = (delta.days * 24 * 60 * 60) + delta.seconds
    random_second = random.randrange(int_delta)
    return debut + datetime.timedelta(seconds=random_second)


contient = []


def creer_commande(id_client):
    global count_commande
    count_commande += 1

    type_commande = random.choice(["standard", "perso_soimeme", "perso_fleuriste"])
    date_commande = creer_date()
    date_livraison = date_commande + datetime.timedelta(days=random.randint(1, 7))
    if date_livraison > hier - datetime.timedelta(days=21):
        statut = random.choice(["VINV", "CC", "CAL"])
    else:
        statut = random.choice(["VINV", "CPAV", "CL", "CC"])
    acc = random.choice(message_accompagnement + [""] * len(message_accompagnement * 2))
    id_magasin = random.randint(1, 3)
    reduc = random.choice([0] * 4 + [5] * 2 + [15])
    if type_commande == "standard" or True:
        bouquet_id = random.randint(1, 6)
        bouquet_price = bouquets[bouquet_id - 1][1]
        contient.append((count_commande, bouquets[bouquet_id - 1][4]))
        return f"(\"{statut}\", \"{acc}\", '', {id_magasin}, NULL, '{datetime.datetime.strftime(date_commande, ft_date)}', '{datetime.datetime.strftime(date_livraison, ft_date)}', {20 + count_commande}, {id_client}, {bouquet_price}, {reduc}, {bouquet_id})"


with open("_CreationTables.sql", "r", encoding="utf-8") as f:
    creation = "".join(f.readlines())

with open("../BelleFleur.sql", "w", encoding="utf-8") as file:
    file.write("-- CREATION\n")
    file.write(creation)

    file.write("\n-- REMPLISSAGE\n")
    file.write("-- remplissage automatiquement généré par le script 'creer_sql_final.py'\n")
    file.write("INSERT INTO adresse (id_adresse, numero_rue, nom_rue, code_postal, ville) VALUES")
    # 17 clients, 3 magasins et 40 commandes
    # (5 clients avec 1 commande, 5 clients avec 2 commandes, 5 clients avec 3 commandes, 1 client avec 10 commandes, 1 client sans commande)
    for i in range(60):
        file.write(f"\n  {creer_adresse()}")
        if i != 59:
            file.write(",")
    file.write(";\n\n")

    file.write("INSERT INTO client (id_client, nom_client, prenom_client, email_client, telephone_client, mot_de_passe, carte_de_credit, id_adresse) VALUES")
    for i in range(16):
        file.write(f"\n  {creer_client()}")
        file.write(",")
    file.write(f"\n  (17, 'LEDOS', 'Thomas', 'thomas.ledos@example.org', '0102030405', 'toto', '5132000000000000', 20);\n\n")

    file.write("INSERT INTO magasin (id_magasin, nom_magasin, id_adresse) VALUES")
    for i in range(3):
        file.write(f"\n  {creer_magasin()}")
        if i != 2:
            file.write(",")
    file.write(";\n\n")

    file.write("INSERT INTO produit (nom_produit, prix_produit, categorie_produit) VALUES")
    for i in range(len(fleurs)):
        file.write(f"\n  ('{fleurs[i][0]}', {fleurs[i][1]}, 'fleur')")
        file.write(",")
    for i in range(len(accessoires)):
        file.write(f"\n  ('{accessoires[i][0]}', {accessoires[i][1]}, 'accessoire')")
        if i != len(accessoires) - 1:
            file.write(",")
    file.write(";\n\n")

    file.write("INSERT INTO stock (id_magasin, id_produit, quantite_stock) VALUES")
    for id_mag in range(1, 3 + 1):
        for id_prod in range(1, len(fleurs) + len(accessoires) + 1):
            file.write(f"\n  ({id_mag}, {id_prod}, {random.randint(0, 100)})")
            if id_mag != 3 or id_prod != len(fleurs) + len(accessoires):
                file.write(",")
    file.write(";\n\n")

    file.write("INSERT INTO bouquet (nom_bouquet, prix_bouquet, desc_bouquet, categorie_bouquet) VALUES")
    for i in range(len(bouquets)):
        file.write(f"\n  (\"{bouquets[i][0]}\", {bouquets[i][1]}, \"{bouquets[i][2]}\", \"{bouquets[i][3]}\")")
        if i != len(bouquets) - 1:
            file.write(",")
    file.write(";\n\n")

    file.write("INSERT INTO compose (id_bouquet, id_produit, quantite_compose) VALUES")
    for id_bouquet in range(1, len(bouquets) + 1):
        for i in range(1, len(bouquets[id_bouquet - 1][4]) + 1):
            composition = bouquets[id_bouquet - 1][4][i - 1]
            file.write(f"\n  ({id_bouquet}, {composition[0]}, {composition[1]})")
            if id_bouquet != len(bouquets) or i != len(bouquets[id_bouquet - 1][4]):
                file.write(",")
    file.write(";\n\n")

    clients_orders = [2, 3, 4, 5, 6] + list(range(7, 12)) * 2 + list(range(12, 17)) * 3 + [17] * 10
    random.shuffle(clients_orders)

    file.write("INSERT INTO commande (statut, message_accompagnement, commentaire_commande, id_magasin, prix_maximum, date_commande, date_livraison_souhaitee, id_adresse, id_client, prix_avant_reduc, pourc_reduc_prix, id_bouquet_base) VALUES")
    for i in range(len(clients_orders)):
        file.write(f"\n  {creer_commande(clients_orders[i])}")
        if i != len(clients_orders) - 1:
            file.write(",")
    file.write(";\n\n")

    file.write("INSERT INTO contient (id_commande, id_produit, quantite_contient) VALUES")
    for i in range(len(contient)):
        for j in range(len(contient[i][1])):
            file.write(f"\n  ({contient[i][0]}, {contient[i][1][j][0]}, {contient[i][1][j][1]})")
            if i != len(contient) - 1 or j != len(contient[i][1]) - 1:
                file.write(",")
    file.write(";\n")
