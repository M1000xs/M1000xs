"""Generate TP2/TP3 SOA reports as HTML (one fixed-size div per A4 page)."""
import html, sys, pathlib

CSS = r"""
@page { size: A4; margin: 0; }
* { box-sizing: border-box; margin: 0; padding: 0; }
html, body { font-family: 'Inter', sans-serif; color: #1f2433; font-size: 13.4px; line-height: 1.6;
  -webkit-print-color-adjust: exact; print-color-adjust: exact; }
.page { width: 210mm; height: 297mm; position: relative; overflow: hidden; page-break-after: always; background: #fff; }
.page:last-child { page-break-after: auto; }
.hdr, .ftr { position: absolute; left: 75px; right: 75px; display: flex; justify-content: space-between;
  font-size: 10px; color: #6b7086; letter-spacing: .02em; }
.hdr { top: 36px; } .ftr { bottom: 30px; }
.ftr .n { color: #4f46e5; font-weight: 600; }
.body { position: absolute; top: 72px; left: 75px; right: 75px; bottom: 70px; }
h2 { display: flex; align-items: baseline; gap: 12px; font-size: 19.5px; font-weight: 700; color: #111427;
  letter-spacing: -.02em; padding-bottom: 10px; border-bottom: 1px solid #e3e5f0; margin: 24px 0 12px; }
h2:first-child { margin-top: 30px; }
h2 .num { font-size: 13px; font-weight: 600; color: #4f46e5; letter-spacing: 0; }
h3 { font-size: 13.5px; font-weight: 600; color: #111427; margin: 14px 0 6px; display: flex; align-items: center; gap: 8px; }
h3::before { content: ''; width: 6px; height: 6px; border-radius: 2px; background: #6366f1; }
p { margin: 0 0 8px; }
ul.dash { list-style: none; margin: 0 0 8px; }
ul.dash li { position: relative; padding-left: 18px; margin-bottom: 5px; }
ul.dash li::before { content: '–'; position: absolute; left: 0; color: #4f46e5; font-weight: 700; }
code { font-family: 'DejaVu Sans Mono', monospace; font-size: 10.6px; background: #eef0fb; color: #3730a3;
  padding: 1px 5px; border-radius: 4px; }
em { font-style: italic; } strong { font-weight: 600; color: #111427; }
table { width: 100%; border-collapse: separate; border-spacing: 0; border: 1px solid #e3e5f0; border-radius: 8px;
  overflow: hidden; margin: 8px 0 10px; }
th { text-align: left; font-size: 9.5px; letter-spacing: .1em; font-weight: 600; color: #6b7086; background: #f7f8fc;
  padding: 9px 12px; border-bottom: 1px solid #e3e5f0; }
td { padding: 8px 12px; border-bottom: 1px solid #eceef5; vertical-align: middle; }
tr:last-child td { border-bottom: 0; }
td:first-child { font-weight: 600; color: #111427; }
table.vars td:first-child { font-weight: 400; }
figure { border: 1px solid #e3e5f0; background: #f7f8fc; border-radius: 10px; padding: 10px 10px 8px; margin: 10px 0 12px; }
figure img { display: block; width: 100%; border-radius: 6px; border: 1px solid #e3e5f0; background: #fff; }
figcaption { font-size: 10.5px; color: #6b7086; margin-top: 7px; }
figcaption b { color: #4f46e5; font-weight: 600; }
.lbl { display: inline-block; font-size: 9px; font-weight: 600; letter-spacing: .14em; color: #4f46e5; background: #eef0fb;
  border-radius: 99px; padding: 2px 8px; margin: 4px 0 6px; }
pre { font-family: 'DejaVu Sans Mono', monospace; font-size: 10.6px; line-height: 1.6; color: #d6dbeb; background: #121828;
  border-top: 3px solid #6366f1; border-radius: 8px; padding: 12px 15px; margin: 0 0 10px; white-space: pre; }
.ok { border: 1px solid #b6ebd0; background: #ecfbf3; color: #14532d; border-radius: 8px; padding: 9px 14px;
  margin: 4px 0 12px; display: flex; align-items: center; gap: 10px; }
.ok .ic { flex: none; width: 18px; height: 18px; border-radius: 50%; background: #10b981; color: #fff;
  display: grid; place-items: center; font-size: 11px; font-weight: 700; }
.ok .tx strong { color: #14532d; }

/* cover */
.cover { background: #0b1020; color: #fff; }
.cover .grid { position: absolute; inset: 0;
  background-image: linear-gradient(rgba(255,255,255,.035) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,.035) 1px, transparent 1px);
  background-size: 42px 42px; }
.cover .g1 { position: absolute; width: 520px; height: 520px; right: -170px; top: -130px; border-radius: 50%;
  background: radial-gradient(circle, rgba(99,102,241,.55), rgba(99,102,241,0) 65%); }
.cover .g2 { position: absolute; width: 420px; height: 420px; left: -170px; top: 570px; border-radius: 50%;
  background: radial-gradient(circle, rgba(14,165,233,.30), rgba(14,165,233,0) 65%); }
.cover .top { position: absolute; left: 75px; right: 75px; top: 80px; display: flex; justify-content: space-between; }
.cover .top b { display: block; font-size: 11.5px; font-weight: 600; margin-bottom: 4px; }
.cover .top span { font-size: 10px; color: #a3a9c4; letter-spacing: .02em; }
.cover .top .r { text-align: right; }
.cover .mid { position: absolute; left: 75px; right: 75px; top: 350px; }
.cover .kick { font-size: 10px; font-weight: 600; letter-spacing: .32em; color: #a5b4fc; margin-bottom: 10px; }
.cover .tp { border-top: 1px solid rgba(255,255,255,.28); border-bottom: 1px solid rgba(255,255,255,.28); padding: 4px 0 0; }
.cover .tp div { font-size: 118px; font-weight: 800; letter-spacing: -.05em; line-height: 1;
  background: linear-gradient(180deg, #ffffff 30%, #b8bdf7); -webkit-background-clip: text; background-clip: text; color: transparent;
  padding-bottom: 6px; }
.cover h1 { font-size: 27px; font-weight: 700; letter-spacing: -.02em; line-height: 1.22; margin-top: 18px; max-width: 560px; }
.cover .sub { color: #a3a9c4; font-size: 14px; margin-top: 12px; max-width: 520px; line-height: 1.55; }
.cover .bar { width: 46px; height: 3px; border-radius: 2px; background: linear-gradient(90deg,#6366f1,#38bdf8); margin-top: 26px; }
.cover .bot { position: absolute; left: 75px; right: 75px; bottom: 115px; border-top: 1px solid rgba(255,255,255,.18);
  padding-top: 16px; display: grid; grid-template-columns: 1fr 1fr 1fr; }
.cover .bot span { display: block; font-size: 9px; letter-spacing: .14em; font-weight: 600; color: #8a90ad; margin-bottom: 5px; }
.cover .bot b { font-size: 13px; font-weight: 600; }
"""


def esc(s):
    return html.escape(s, quote=False)


def cover(d):
    return f"""<section class="page cover"><div class="grid"></div><div class="g1"></div><div class="g2"></div>
<div class="top"><div><b>ISIMG</b><span>Institut Supérieur d'Informatique et de Multimédia de Gabès</span></div>
<div class="r"><b>Architecture SOA</b><span>Année universitaire 2026–2027</span></div></div>
<div class="mid"><div class="kick">COMPTE RENDU DE TRAVAUX PRATIQUES</div>
<div class="tp"><div>TP0{d['n']}</div></div>
<h1>{d['title']}</h1><div class="sub">{d['subtitle']}</div><div class="bar"></div></div>
<div class="bot"><div><span>RÉALISÉ PAR</span><b>Ali Ezzet Yahyaoui</b></div>
<div><span>CLASSE</span><b>LSIM3</b></div><div><span>ENSEIGNANTE</span><b>Mme Imen Nbiba</b></div></div>
</section>"""


def page(d, i, content):
    return f"""<section class="page"><div class="hdr"><span>TP{d['n']} · Architecture SOA</span><span>Ali Ezzet Yahyaoui — LSIM3</span></div>
<div class="body">{content}</div>
<div class="ftr"><span>ISIMG · 2026–2027</span><span class="n">{i:02d}</span></div></section>"""


def fig(src, n, cap, maxh=None):
    st = f' style="max-height:{maxh}px;object-fit:contain"' if maxh else ''
    return f'<figure><img src="{src}"{st}><figcaption><b>Figure {n}</b> — {cap}</figcaption></figure>'


def h2(n, t):
    return f'<h2><span class="num">{n:02d}</span>{t}</h2>'


def pre(s):
    return f'<pre>{esc(s.strip(chr(10)))}</pre>'


def env_table(rows):
    return '<table><tr><th>OUTIL</th><th>VERSION</th></tr>' + ''.join(
        f'<tr><td>{a}</td><td>{b}</td></tr>' for a, b in rows) + '</table>'


def ok(html_text):
    # Single wrapper span so the flex gap only applies between icon and text
    # (previously the text nodes and <strong> were separate flex items, which
    # inserted a large gap around the number).
    return f'<div class="ok"><span class="ic">✓</span><span class="tx">{html_text}</span></div>'


def doc(d, pages):
    out = [cover(d)] + [page(d, i + 2, c) for i, c in enumerate(pages)]
    return f"""<!doctype html><html lang="fr"><head><meta charset="utf-8"><title>TP{d['n']} SOA</title>
<style>{CSS}</style></head><body>{''.join(out)}</body></html>"""


# ---------------------------------------------------------------- TP2
TP2 = dict(n=2, title="Création d'un service Web SOAP avec Apache Axis2 et Eclipse",
           subtitle="Initiation aux services Web : exposition d'une méthode Java comme opération SOAP.")

tp2_req = '''
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:com="http://com.calc">
   <soapenv:Body>
      <com:addTwoNumbers>
         <com:args0>25</com:args0>
         <com:args1>17</com:args1>
      </com:addTwoNumbers>
   </soapenv:Body>
</soapenv:Envelope>'''
tp2_res = '''
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
   <soapenv:Body>
      <ns:addTwoNumbersResponse xmlns:ns="http://com.calc">
         <ns:return>42</ns:return>
      </ns:addTwoNumbersResponse>
   </soapenv:Body>
</soapenv:Envelope>'''

tp2_pages = [
    h2(1, 'Objectifs') + '''<ul class="dash">
<li>Comprendre le rôle d'un service Web : faire communiquer des applications hétérogènes dans un environnement distribué.</li>
<li>Réaliser pas à pas un service Web SOAP simple avec le moteur Apache Axis2 et l'IDE Eclipse.</li>
<li>Exposer la méthode Java <code>addTwoNumbers</code> comme opération du service et vérifier son déploiement.</li></ul>'''
    + h2(2, 'Environnement de travail') + env_table([
        ('IDE', 'Eclipse IDE for Enterprise Java and Web Developers 2025-09'), ('JDK', 'OpenJDK 21'),
        ('Serveur', 'Apache Tomcat 8.5.100'), ('Moteur de services Web', 'Apache Axis2 1.8.0 (distribution binaire)'),
        ('Outil de build', 'Apache Ant 1.10.15')])
    + h2(3, 'Configuration') + '''<p>Les variables d'environnement suivantes ont été créées puis ajoutées au <code>PATH</code> :</p>
<table class="vars"><tr><th>VARIABLE</th><th>CONTENU</th></tr>
<tr><td><code>JAVA_HOME</code></td><td>Dossier d'installation du JDK</td></tr>
<tr><td><code>CATALINA_HOME</code></td><td>Dossier d'installation de Tomcat 8.5</td></tr>
<tr><td><code>ANT_HOME</code></td><td>Dossier d'installation d'Ant</td></tr></table>
<p>Côté Eclipse, deux réglages sont nécessaires :</p><ul class="dash">
<li><strong>Window › Preferences › Server › Runtime Environments</strong> : ajout d'Apache Tomcat v8.5 avec son répertoire d'installation.</li>
<li><strong>Window › Preferences › Web Services › Axis2 Preferences</strong> : choix du dossier de la distribution binaire d'Axis2.</li></ul>''',

    h2(4, 'Mise en place du service Web') + '''<h3>4.1 Projet Web dynamique</h3>
<p>Le projet <code>MyFirstWebService</code> a été créé via <em>File › New › Other › Web › Dynamic Web Project</em>, avec Tomcat 8.5 comme runtime, le module Web dynamique en version <strong>2.5</strong> et la facette <strong>Axis2 Web Services</strong> activée depuis le bouton <em>Modify</em>.</p>
<h3>4.2 Classe Java</h3>
<p>La classe <code>FirstWebService</code>, placée dans le paquetage <code>calc.com</code>, contient la méthode <code>addTwoNumbers</code>. Elle renvoie la somme de deux entiers et deviendra l'opération exposée par le service.</p>'''
    + fig('tp2-1.png', 1, 'Classe FirstWebService.java dans Eclipse') + '''<h3>4.3 Génération du service</h3>
<p>Avec l'assistant <em>File › New › Other › Web Services › Web Service</em>, la classe <code>calc.com.FirstWebService</code> a été sélectionnée et le moteur <strong>Apache Axis2</strong> choisi dans <em>Server runtime</em> (à la place d'Axis, proposé par défaut). L'assistant produit le descripteur <code>services.xml</code>, lance Tomcat et publie le service.</p>
<p>Ce descripteur précise le nom du service, la classe exposée (paramètre <code>ServiceClass</code>) et les récepteurs de messages RPC utilisés par Axis2.</p>'''
    + fig('tp2-2.png', 2, 'Descripteur services.xml de FirstWebService'),

    h2(5, 'Résultats') + '''<p>Une fois le serveur démarré, la page <code>http://localhost:8080/MyFirstWebService/services/listServices</code> liste les services déployés. <strong>FirstWebService</strong> y figure avec le statut <em>Active</em> et l'opération <code>addTwoNumbers</code>.</p>'''
    + fig('tp2-3.png', 3, 'FirstWebService déployé par Axis2')
    + '<p>Un clic sur le nom du service affiche le <strong>WSDL</strong> généré automatiquement par Axis2. Ce document décrit le contrat du service : types, messages, opérations, liaisons SOAP et adresse.</p>'
    + fig('tp2-4.png', 4, 'Document WSDL de FirstWebService'),

    h2(6, 'Test du service') + '<p>Le service a été appelé avec une requête SOAP contenant les valeurs 25 et 17.</p>'
    + '<span class="lbl">REQUÊTE</span>' + pre(tp2_req) + '<span class="lbl">RÉPONSE</span>' + pre(tp2_res)
    + ok('Le résultat <strong>42</strong> correspond bien à 25 + 17 : le service répond correctement.')
    + h2(7, 'Conclusion') + "<p>Ce premier TP a permis de publier une simple méthode Java comme opération d'un service Web SOAP grâce à Axis2 et Eclipse. Le service est décrit automatiquement par un WSDL et peut être appelé à distance par des messages SOAP. Il reste toutefois dépendant d'Eclipse pour s'exécuter : son archivage et son déploiement sur un serveur autonome font l'objet du TP3.</p>",
]

# ---------------------------------------------------------------- TP3
TP3 = dict(n=3, title="Archivage et déploiement d'un service Web SOAP",
           subtitle="Empaquetage d'un service Axis2 en archive .aar et déploiement sur Tomcat autonome.")

tp3_jar = '''
jar cvf Product.aar ./*
jar tvf Product.aar
     0 META-INF/
    58 META-INF/MANIFEST.MF
   541 META-INF/services.xml
     0 pro/
     0 pro/com/
   403 pro/com/Product.class'''
tp3_req = '''
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:pro="http://com.pro">
   <soapenv:Body>
      <pro:ProductTwoNumbers>
         <pro:args0>6</pro:args0>
         <pro:args1>7</pro:args1>
      </pro:ProductTwoNumbers>
   </soapenv:Body>
</soapenv:Envelope>'''
tp3_res = '''
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
   <soapenv:Body>
      <ns:ProductTwoNumbersResponse xmlns:ns="http://com.pro">
         <ns:return>42</ns:return>
      </ns:ProductTwoNumbersResponse>
   </soapenv:Body>
</soapenv:Envelope>'''

tp3_pages = [
    h2(1, 'Objectifs') + '''<ul class="dash">
<li>Créer un service Web SOAP <strong>Product</strong> avec Axis2 et Eclipse.</li>
<li>Empaqueter ce service dans une archive <code>.aar</code> (Axis Archive).</li>
<li>Le déployer sur un serveur Apache Tomcat autonome via l'application Web Axis2, sans passer par Eclipse.</li></ul>'''
    + h2(2, 'Environnement de travail') + env_table([
        ('IDE', 'Eclipse IDE for Enterprise Java and Web Developers 2025-09'), ('JDK', 'OpenJDK 21'),
        ('Serveur', 'Apache Tomcat 8.5.100'), ('Moteur de services Web', 'Apache Axis2 1.8.0 (distributions binaire et WAR)')])
    + h2(3, 'Création du service Web SOAP') + '''<h3>3.1 Projet</h3>
<p>Un projet Web dynamique <code>ProductService</code> a été créé avec Tomcat 8.5 comme runtime, le module Web dynamique en version 2.5 et la facette Axis2 Web Services.</p>
<h3>3.2 Classe Java</h3>
<p>La classe <code>Product</code>, dans le paquetage <code>pro.com</code>, définit la méthode <code>ProductTwoNumbers</code> qui renvoie le produit de deux entiers.</p>'''
    + fig('tp3-1.png', 1, 'Classe Product.java dans Eclipse') + '''<h3>3.3 Génération du service</h3>
<p>L'assistant <em>File › New › Other › Web Service</em> a été lancé sur la classe <code>pro.com.Product</code> avec le moteur <strong>Apache Axis2</strong>. L'option de génération d'un <code>services.xml</code> par défaut a été conservée, puis le serveur a été démarré et le service publié avec les paramètres par défaut.</p>''',

    fig('tp3-2.png', 2, 'Descripteur services.xml du service Product')
    + "<p>Le déploiement a été vérifié sur <code>http://localhost:8080/ProductService/services/listServices</code>. À ce stade, le service ne tourne que tant qu'Eclipse exécute le serveur, d'où la nécessité de l'archiver.</p>"
    + h2(4, 'Archivage et déploiement') + '''<h3>4.1 Création de Product.aar</h3>
<p>Dans le workspace, le service se trouve dans <code>ProductService/WebContent/WEB-INF/services/Product</code>. Depuis ce dossier, la commande <code>jar</code> (accessible après l'ajout du dossier <code>bin</code> du JDK au <code>PATH</code>) crée l'archive, puis permet d'en vérifier le contenu :</p>'''
    + pre(tp3_jar) + '''<p>L'archive regroupe le descripteur <code>META-INF/services.xml</code> et la classe compilée <code>pro/com/Product.class</code>.</p>
<h3>4.2 Déploiement sur Tomcat autonome</h3>
<table><tr><th style="width:44px">#</th><th>ÉTAPE</th></tr>
<tr><td>1</td><td>Copie de <code>axis2.war</code> (distribution WAR d'Axis2) dans le dossier <code>webapps</code> de Tomcat.</td></tr>
<tr><td>2</td><td>Arrêt du Tomcat lancé par Eclipse, puis démarrage de Tomcat autonome avec le script <code>startup</code> du dossier <code>bin</code>.</td></tr>
<tr><td>3</td><td>Ouverture de la page d'accueil d'Axis2 : <code>http://localhost:8080/axis2/</code>.</td></tr>
<tr><td>4</td><td>Lien <em>Administration</em>, connexion avec <code>admin / axis2</code>, puis <em>Upload Service</em> de <code>Product.aar</code> (équivalent à copier l'archive dans <code>webapps/axis2/WEB-INF/services</code>).</td></tr></table>''',

    h2(5, 'Résultats') + "<p>Sur <code>http://localhost:8080/axis2/services/listServices</code>, le service <strong>Product</strong> apparaît avec le statut <em>Active</em> et l'opération <code>ProductTwoNumbers</code>.</p>"
    + fig('tp3-3.png', 3, 'Service Product déployé sur Tomcat autonome', maxh=300)
    + '<p>Axis2 génère automatiquement le document WSDL du service :</p>'
    + fig('tp3-4.png', 4, 'Document WSDL du service Product', maxh=405),

    h2(6, 'Test du service') + '<p>Le service a été appelé avec une requête SOAP contenant les valeurs 6 et 7.</p>'
    + '<span class="lbl">REQUÊTE</span>' + pre(tp3_req) + '<span class="lbl">RÉPONSE</span>' + pre(tp3_res)
    + ok('Le résultat <strong>42</strong> correspond bien à 6 × 7.')
    + h2(7, 'Conclusion') + "<p>Ce TP a permis d'empaqueter le service SOAP Product dans l'archive <code>Product.aar</code> puis de le déployer sur un Tomcat autonome grâce à l'application Web Axis2. Le service devient accessible indépendamment d'Eclipse, ce qui correspond au mode de déploiement utilisé en production.</p>",
]

if __name__ == '__main__':
    out = pathlib.Path(sys.argv[1])
    (out / 'TP2.html').write_text(doc(TP2, tp2_pages), encoding='utf-8')
    (out / 'TP3.html').write_text(doc(TP3, tp3_pages), encoding='utf-8')
