# ShopEase : site e-commerce en ASP.NET Core MVC

Projet du module « Développement d'applications web et multimédia (J2EE, .NET) », LSIM3, 2026-2027.

Application web de commerce électronique qui permet de consulter le catalogue, de remplir un panier et de passer commande.

## Livrables

| Livrable demandé | Fichier |
|---|---|
| Présentation des interfaces | [`docs/Presentation-ShopEase.pptx`](docs/Presentation-ShopEase.pptx) (21 diapositives) et [`docs/captures/`](docs/captures) |
| Diagramme de classes | [`docs/diagramme-classes.png`](docs/diagramme-classes.png) (modèle métier) et [`docs/diagramme-architecture.png`](docs/diagramme-architecture.png) (contrôleurs et services) |
| Schéma de la base de données | [`docs/schema-base-de-donnees.png`](docs/schema-base-de-donnees.png) et [`docs/script-base-de-donnees.sql`](docs/script-base-de-donnees.sql) |
| Code source | [`ShopEase/`](ShopEase) |

Les sources Mermaid des diagrammes (`.mmd`) sont dans `docs/`. Elles s'ouvrent sur https://mermaid.live.

## Fonctionnalités

**Catalogue de produits**
- Liste paginée de 9 produits par page.
- Filtres : recherche texte, catégorie, prix minimum et maximum, « en stock uniquement ».
- Tri par popularité (nombre de ventes), nouveautés, note, prix ou nom.
- Page de détails avec description, prix, stock, galerie de 3 images et produits de la même catégorie.

**Panier d'achat**
- Ajout au panier, modification des quantités (boutons − / + ou saisie), suppression d'un article, vidage du panier.
- Sous-total, frais de livraison (8 DT, offerts dès 300 DT) et total.
- Le panier est enregistré en base. Un visiteur non connecté est repéré par un cookie, et son panier est fusionné avec celui de son compte à la connexion.

**Commande et paiement**
- Page de validation avec les informations d'expédition, pré-remplies depuis le profil.
- Paiement par carte, simulé : le format et la clé de Luhn sont vérifiés, mais rien n'est enregistré. Le paiement à la livraison est aussi proposé.
- Confirmation avec un numéro de commande généré au format `CMD-AAAAMMJJ-XXXXXX`.
- La commande est créée dans une transaction : contrôle du stock, décrément du stock, mise à jour de la popularité, puis vidage du panier.

**Gestion des utilisateurs** (ASP.NET Core Identity)
- Inscription, connexion et déconnexion.
- Page de profil pour modifier ses informations personnelles et son mot de passe.
- Historique des commandes et détail de chaque commande.

**En plus : espace d'administration** (rôle `Admin`)
- Tableau de bord : chiffre d'affaires, nombre de commandes, produits en rupture, meilleures ventes.
- Gestion des produits : ajout, modification avec envoi d'image, suppression.
- Suivi des commandes : filtre par statut et changement de statut. Une commande annulée remet ses articles en stock.

## Technologies

- **ASP.NET Core 8 MVC** : contrôleurs, vues Razor, ViewModels, View Component et Area `Admin`.
- **Entity Framework Core 8** en Code First, avec migrations et base **SQLite**.
- **ASP.NET Core Identity** : mots de passe hachés, rôles `Admin` et `Client`.
- Bootstrap 5 et validation jQuery Unobtrusive.

## Lancer le projet

Prérequis : [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
cd ShopEase
dotnet run
```

Ouvrez ensuite l'URL affichée dans la console, par exemple http://localhost:5xxx. Vous pouvez aussi ouvrir `ShopEase.sln` dans Visual Studio et appuyer sur F5.

Au premier démarrage, la base `shopease.db` est créée par les migrations. Elle est ensuite remplie avec 6 catégories, 30 produits, deux comptes et deux commandes d'exemple.

| Compte | Email | Mot de passe |
|---|---|---|
| Client | client@shopease.tn | Client123 |
| Administrateur | admin@shopease.tn | Admin123 |

Carte de test : `4242 4242 4242 4242`, avec une date d'expiration future (par exemple `12/29`) et n'importe quel CVV à 3 chiffres.

Pour repartir d'une base vierge, supprimez `ShopEase/shopease.db` puis relancez le projet.

### Utiliser SQL Server au lieu de SQLite (facultatif)

1. `dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11`
2. Dans `Program.cs`, remplacez `UseSqlite(...)` par `UseSqlServer(...)`.
3. Dans `appsettings.json`, utilisez par exemple : `Server=(localdb)\\mssqllocaldb;Database=ShopEase;Trusted_Connection=True`
4. Supprimez le dossier `Data/Migrations`, puis lancez `dotnet ef migrations add InitialCreate -o Data/Migrations`.

## Organisation du code

```
ShopEase/
├── Areas/Admin/          Espace administrateur (Dashboard, Products, Orders)
├── Controllers/          Home, Products, Cart, Orders, Account
├── Data/                 ApplicationDbContext, DbSeeder, données de démo, Migrations
├── Extensions/           Formatage (DT, dates), libellés des énumérations
├── Models/               Category, Product, ProductImage, CartItem, Order, OrderItem, ApplicationUser
├── Services/             CartService, OrderService (logique métier injectée)
├── ViewComponents/       Badge du panier dans la barre de navigation
├── ViewModels/           Filtres, panier, commande, comptes
├── Views/                Vues Razor
└── wwwroot/              CSS, JS, images des produits (SVG)
tools/                    Scripts Python : images, données de démo, présentation
docs/                     Livrables (présentation, diagrammes, schéma, captures)
```
