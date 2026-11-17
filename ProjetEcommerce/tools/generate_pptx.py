"""Génère docs/Presentation-ShopEase.pptx à partir des captures et diagrammes."""
import os
from pptx import Presentation
from pptx.util import Inches, Pt, Emu
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_SHAPE
from pptx.enum.text import PP_ALIGN
from PIL import Image

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
DOCS = os.path.join(ROOT, "docs")
CAP = os.path.join(DOCS, "captures")
BRAND, DARK, INK, MUTED, SOFT = RGBColor(0x4F, 0x46, 0xE5), RGBColor(0x37, 0x30, 0xA3), RGBColor(0x1F, 0x29, 0x37), RGBColor(0x6B, 0x72, 0x80), RGBColor(0xEE, 0xF2, 0xFF)

prs = Presentation()
prs.slide_width, prs.slide_height = Inches(13.333), Inches(7.5)
SW, SH = prs.slide_width, prs.slide_height
BLANK = prs.slide_layouts[6]

def rect(s, x, y, w, h, color):
    r = s.shapes.add_shape(MSO_SHAPE.RECTANGLE, x, y, w, h)
    r.fill.solid(); r.fill.fore_color.rgb = color; r.line.fill.background()
    return r

def text(s, x, y, w, h, t, size=18, bold=False, color=INK, align=PP_ALIGN.LEFT):
    tb = s.shapes.add_textbox(x, y, w, h); tf = tb.text_frame; tf.word_wrap = True
    lines = t if isinstance(t, list) else [t]
    for i, line in enumerate(lines):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.alignment = align
        r = p.add_run(); r.text = line
        r.font.size, r.font.bold, r.font.color.rgb, r.font.name = Pt(size), bold, color, "Calibri"
        p.space_after = Pt(6)
    return tb

def bullets(s, x, y, w, h, items, size=18):
    tb = s.shapes.add_textbox(x, y, w, h); tf = tb.text_frame; tf.word_wrap = True
    for i, it in enumerate(items):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        sub = it.startswith("  ")
        r = p.add_run(); r.text = ("– " if sub else "• ") + it.strip()
        r.font.size = Pt(size - 3 if sub else size); r.font.color.rgb = MUTED if sub else INK; r.font.name = "Calibri"
        p.level = 1 if sub else 0; p.space_after = Pt(4 if sub else 8)
    return tb

def header(s, title, num):
    rect(s, 0, 0, SW, Inches(1.0), BRAND)
    text(s, Inches(0.5), Inches(0.18), Inches(11.5), Inches(0.7), title, 28, True, RGBColor(255, 255, 255))
    text(s, Inches(12.0), Inches(7.0), Inches(1.1), Inches(0.4), str(num), 12, False, MUTED, PP_ALIGN.RIGHT)
    text(s, Inches(0.5), Inches(7.0), Inches(6), Inches(0.4), "ShopEase — Projet ASP.NET Core MVC", 12, False, MUTED)

def picture(s, path, x, y, maxw, maxh, border=True):
    w, h = Image.open(path).size
    scale = min(maxw / w, maxh / h)
    pw, ph = int(w * scale), int(h * scale)
    px, py = x + (maxw - pw) // 2, y + (maxh - ph) // 2
    pic = s.shapes.add_picture(path, px, py, pw, ph)
    if border:
        pic.line.color.rgb = RGBColor(0xD1, 0xD5, 0xDB); pic.line.width = Pt(1)
    return pic

n = 0
def new_slide():
    global n; n += 1
    return prs.slides.add_slide(BLANK)

# 1. Titre
s = new_slide()
rect(s, 0, 0, SW, SH, DARK)
rect(s, 0, Inches(4.9), SW, Inches(2.6), BRAND)
text(s, Inches(0.8), Inches(1.2), Inches(11), Inches(0.6), "Projet — Développement d'applications web et multimédia", 20, False, RGBColor(0xC7, 0xD2, 0xFE))
text(s, Inches(0.8), Inches(1.9), Inches(11), Inches(1.4), "ShopEase : site e-commerce en ASP.NET Core MVC", 44, True, RGBColor(255, 255, 255))
text(s, Inches(0.8), Inches(3.5), Inches(11), Inches(0.8), "Catalogue · Panier · Commande et paiement · Gestion des utilisateurs · Administration", 20, False, RGBColor(0xE0, 0xE7, 0xFF))
text(s, Inches(0.8), Inches(5.3), Inches(7), Inches(1.6), ["Réalisé par : Ali Ezzet Yahyaoui — LSIM3", "Enseignante du cours : Nesrine Akrout", "Enseignante du TP : Asma Alakrout"], 18, False, RGBColor(255, 255, 255))
text(s, Inches(8.5), Inches(5.3), Inches(4.2), Inches(1.0), "Année universitaire 2026-2027", 18, True, RGBColor(255, 255, 255), PP_ALIGN.RIGHT)

# 2. Objectif
s = new_slide(); header(s, "Objectif et fonctionnalités", n)
bullets(s, Inches(0.6), Inches(1.3), Inches(6.0), Inches(5.5), [
    "Catalogue de produits",
    "  Liste paginée (9 produits par page)",
    "  Filtres : recherche, catégorie, prix min/max, en stock",
    "  Tri : popularité, nouveautés, note, prix, nom",
    "  Page détails : description, prix, galerie d'images",
    "Panier d'achat",
    "  Ajout, modification des quantités, suppression",
    "  Total, frais de livraison (offerte dès 300 DT)",
], 20)
bullets(s, Inches(6.9), Inches(1.3), Inches(6.0), Inches(5.5), [
    "Commande et paiement",
    "  Saisie des informations d'expédition",
    "  Paiement par carte (simulé) ou à la livraison",
    "  Numéro de commande généré : CMD-AAAAMMJJ-XXXXXX",
    "Gestion des utilisateurs",
    "  Inscription, connexion, déconnexion",
    "  Profil modifiable + historique des commandes",
    "Bonus : espace administrateur (produits, commandes)",
], 20)

# 3. Technologies
s = new_slide(); header(s, "Technologies et architecture", n)
bullets(s, Inches(0.6), Inches(1.3), Inches(5.2), Inches(5.6), [
    "ASP.NET Core 8 MVC",
    "  Contrôleurs + vues Razor + ViewModels",
    "  Area « Admin » protégée par rôle",
    "Entity Framework Core 8",
    "  Code First + migrations, SQLite",
    "ASP.NET Core Identity",
    "  Mots de passe hachés, rôles Admin / Client",
    "Services injectés (DI)",
    "  CartService, OrderService",
    "Bootstrap 5, validation jQuery",
], 18)
picture(s, os.path.join(DOCS, "diagramme-architecture.png"), Inches(6.2), Inches(1.1), Inches(6.9), Inches(5.85), False)

# 4. Diagramme de classes
s = new_slide(); header(s, "Diagramme de classes (modèle métier)", n)
picture(s, os.path.join(DOCS, "diagramme-classes.png"), Inches(0.4), Inches(1.15), Inches(12.5), Inches(5.8), False)

# 5. Schéma BDD
s = new_slide(); header(s, "Schéma de la base de données", n)
picture(s, os.path.join(DOCS, "schema-base-de-donnees.png"), Inches(0.4), Inches(1.15), Inches(7.6), Inches(5.8), False)
bullets(s, Inches(8.3), Inches(1.4), Inches(4.7), Inches(5.5), [
    "Tables générées par les migrations EF Core",
    "Tables Identity : AspNetUsers, AspNetRoles, AspNetUserRoles…",
    "CartItems : panier persistant",
    "  CartId = Id utilisateur ou cookie anonyme",
    "  Fusion du panier anonyme à la connexion",
    "OrderItems copie nom et prix",
    "  L'historique reste exact si le produit change",
    "Index uniques : Categories.Name, Orders.OrderNumber, (CartId, ProductId)",
], 16)

# Interfaces
screens = [
    ("01-accueil", "Page d'accueil", "Hero, catégories, produits les plus populaires et nouveautés."),
    ("02-catalogue", "Catalogue paginé", "Liste des produits avec filtres à gauche et tri par popularité."),
    ("03-catalogue-filtres", "Filtres de recherche", "Catégorie Informatique, prix entre 100 et 1000 DT, tri par prix croissant."),
    ("04-details-produit", "Détails d'un produit", "Description, prix, stock, galerie d'images et produits similaires."),
    ("05-panier", "Panier d'achat", "Modification des quantités (− / +), suppression, total et frais de livraison."),
    ("06-inscription", "Inscription", "Création de compte avec validation côté client et serveur."),
    ("07-connexion", "Connexion", "Redirection automatique vers la commande après connexion."),
    ("08-validation-commande", "Validation de la commande", "Adresse d'expédition pré-remplie depuis le profil, choix du paiement."),
    ("09-confirmation", "Confirmation de commande", "Numéro de commande généré et récapitulatif."),
    ("10-profil", "Profil utilisateur", "Modification des informations personnelles et historique des commandes."),
    ("11-detail-commande", "Détail d'une commande", "Lignes de commande, adresse, mode de paiement et statut."),
    ("12-admin-dashboard", "Administration : tableau de bord", "Chiffre d'affaires, commandes, stock et meilleures ventes."),
    ("13-admin-produits", "Administration : produits", "Ajout, modification (avec upload d'image) et suppression."),
    ("14-admin-commandes", "Administration : commandes", "Filtre par statut et changement de statut (annulation = remise en stock)."),
    ("15-mobile", "Affichage mobile", "Interface responsive grâce à Bootstrap."),
]
for f, title, caption in screens:
    s = new_slide(); header(s, title, n)
    path = os.path.join(CAP, f + ".png")
    if f == "15-mobile":
        picture(s, path, Inches(0.6), Inches(1.2), Inches(5.5), Inches(5.6))
        text(s, Inches(6.6), Inches(3.2), Inches(6.2), Inches(1.5), caption, 22, False, INK)
    else:
        picture(s, path, Inches(0.5), Inches(1.2), Inches(12.3), Inches(5.25))
        text(s, Inches(0.5), Inches(6.5), Inches(12.3), Inches(0.5), caption, 16, False, MUTED, PP_ALIGN.CENTER)

# Conclusion
s = new_slide(); header(s, "Lancement et conclusion", n)
bullets(s, Inches(0.6), Inches(1.3), Inches(6.2), Inches(5.5), [
    "Lancer le projet",
    "  cd ShopEase  puis  dotnet run",
    "  La base SQLite est créée et remplie au démarrage",
    "Comptes de démonstration",
    "  Client : client@shopease.tn / Client123",
    "  Admin : admin@shopease.tn / Admin123",
    "  Carte de test : 4242 4242 4242 4242",
], 20)
bullets(s, Inches(7.0), Inches(1.3), Inches(5.9), Inches(5.5), [
    "Toutes les fonctionnalités demandées sont réalisées",
    "Sécurité : Identity, jetons anti-CSRF, contrôle des rôles",
    "Intégrité : commande en transaction, vérification du stock",
    "Améliorations possibles",
    "  Paiement réel (Stripe, Konnect…)",
    "  Avis clients, liste de souhaits",
    "  E-mails de confirmation",
], 20)

out = os.path.join(DOCS, "Presentation-ShopEase.pptx")
prs.save(out); print("écrit", out, n, "diapositives")
