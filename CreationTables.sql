DROP DATABASE IF EXISTS fleurs;
CREATE DATABASE fleurs;
USE fleurs;

CREATE TABLE adresse (
    id_adresse INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT,
    numero_rue INT UNSIGNED NOT NULL,
    nom_rue VARCHAR(255) NOT NULL,
    code_postal INT UNSIGNED NOT NULL,
    ville VARCHAR(255) NOT NULL
);

CREATE TABLE client (
    id_client INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT,
    nom_client VARCHAR(255) NOT NULL,
    prenom_client VARCHAR(255) NOT NULL,
    email_client VARCHAR(255) NOT NULL,
    telephone_client VARCHAR(20) NOT NULL,
    mot_de_passe VARCHAR(255) NOT NULL,
    carte_de_credit VARCHAR(255) NOT NULL,
    id_adresse INT UNSIGNED NOT NULL,
    UNIQUE KEY `email_client` (`email_client`),
    CONSTRAINT fk_client_adresse FOREIGN KEY (id_adresse) REFERENCES adresse(id_adresse)
);

CREATE TABLE produit (
    id_produit INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT,
    nom_produit VARCHAR(255) NOT NULL,
    prix_produit FLOAT NOT NULL,
    categorie_produit ENUM("fleur", "accessoire") NOT NULL
);

CREATE TABLE magasin (
    id_magasin INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT,
    nom_magasin VARCHAR(255) NOT NULL,
    id_adresse INT UNSIGNED NOT NULL,
    CONSTRAINT fk_magasin_adresse FOREIGN KEY (id_adresse) REFERENCES adresse(id_adresse)
);

CREATE TABLE stock (
    id_produit INT UNSIGNED NOT NULL,
    id_magasin INT UNSIGNED NOT NULL,
    quantite_stock INT UNSIGNED NOT NULL,
    CONSTRAINT fk_stock_produit FOREIGN KEY (id_produit) REFERENCES produit(id_produit),
    CONSTRAINT fk_stock_magasin FOREIGN KEY (id_magasin) REFERENCES magasin(id_magasin),
    PRIMARY KEY (id_produit, id_magasin)
);

CREATE TABLE bouquet (
    id_bouquet INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT,
    nom_bouquet VARCHAR(255) NOT NULL,
    prix_bouquet FLOAT NOT NULL,
    desc_bouquet TEXT NOT NULL,
    categorie_bouquet VARCHAR(255) NOT NULL
);

CREATE TABLE compose (
    id_bouquet INT UNSIGNED NOT NULL,
    id_produit INT UNSIGNED NOT NULL,
    quantite_compose INT UNSIGNED NOT NULL,
    CONSTRAINT fk_compose_bouquet FOREIGN KEY (id_bouquet) REFERENCES bouquet(id_bouquet) ON DELETE CASCADE,
    CONSTRAINT fk_compose_produit FOREIGN KEY (id_produit) REFERENCES produit(id_produit) ON DELETE CASCADE,
    PRIMARY KEY (id_bouquet, id_produit)
);

CREATE TABLE commande (
    id_commande INT UNSIGNED NOT NULL PRIMARY KEY AUTO_INCREMENT,
    statut ENUM("VINV", "CC", "CPAV", "CAL", "CL") NOT NULL,
    message_accompagnement TEXT NOT NULL,
    commentaire_commande TEXT NOT NULL,
    id_magasin INT UNSIGNED NOT NULL,
    prix_maximum FLOAT NOT NULL,
    date_commande DATETIME NOT NULL,
    id_adresse INT UNSIGNED NOT NULL,
    id_client INT UNSIGNED NOT NULL,
    prix_avant_reduc FLOAT NOT NULL,
    # si le client choisit un bouquet standard, il paiera peut-�tre moins cher que tous les produits � l'unit�
	# donc on stocke le prix par commande (cela �vite en plus de refaire la somme � chaque fois)
    pourc_reduc_prix INT UNSIGNED NOT NULL,
    CONSTRAINT fk_commande_magasin FOREIGN KEY (id_magasin) REFERENCES magasin(id_magasin),
    CONSTRAINT fk_commande_adresse FOREIGN KEY (id_adresse) REFERENCES adresse(id_adresse),
    CONSTRAINT fk_commande_client FOREIGN KEY (id_client) REFERENCES client(id_client)
);

CREATE TABLE contient (
    id_commande INT UNSIGNED NOT NULL,
    id_produit INT UNSIGNED NOT NULL,
    quantite_contient INT UNSIGNED NOT NULL,
    CONSTRAINT fk_contient_commande FOREIGN KEY (id_commande) REFERENCES commande(id_commande),
    CONSTRAINT fk_contient_produit FOREIGN KEY (id_produit) REFERENCES produit(id_produit),
    PRIMARY KEY (id_commande, id_produit)
);

DROP USER IF EXISTS bozo@localhost;
CREATE USER bozo@localhost IDENTIFIED BY 'bozo';
GRANT SELECT ON fleurs.* TO bozo@localhost;
