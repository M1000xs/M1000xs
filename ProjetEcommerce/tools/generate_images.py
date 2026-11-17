"""Génère les illustrations SVG des produits (3 vues par produit) dans wwwroot/images/products."""
import os, sys
sys.path.insert(0, os.path.dirname(__file__))
from catalog import CATEGORIES, PRODUCTS

OUT = os.path.join(os.path.dirname(__file__), "..", "ShopEase", "wwwroot", "images", "products")
os.makedirs(OUT, exist_ok=True)

W = "#ffffff"
ICONS = {
 "laptop": '<rect x="170" y="170" width="260" height="170" rx="14" fill="{d}"/><rect x="186" y="186" width="228" height="138" rx="6" fill="{l}"/><path d="M130 350h340l-24 36H154z" fill="{d}"/><rect x="270" y="356" width="60" height="8" rx="4" fill="{l}"/>',
 "mouse": '<rect x="220" y="150" width="160" height="260" rx="80" fill="{d}"/><line x1="300" y1="150" x2="300" y2="250" stroke="{l}" stroke-width="8"/><line x1="220" y1="250" x2="380" y2="250" stroke="{l}" stroke-width="8"/><rect x="290" y="185" width="20" height="40" rx="10" fill="{l}"/>',
 "keyboard": '<rect x="110" y="210" width="380" height="180" rx="18" fill="{d}"/>' + "".join(f'<rect x="{130+i*44}" y="{230+j*40}" width="34" height="30" rx="5" fill="{{l}}"/>' for j in range(3) for i in range(8)) + '<rect x="196" y="350" width="208" height="26" rx="5" fill="{l}"/>',
 "monitor": '<rect x="120" y="140" width="360" height="230" rx="14" fill="{d}"/><rect x="138" y="158" width="324" height="194" rx="4" fill="{l}"/><rect x="280" y="370" width="40" height="50" fill="{d}"/><rect x="220" y="415" width="160" height="16" rx="8" fill="{d}"/>',
 "headphones": '<path d="M170 330v-50a130 130 0 0 1 260 0v50" fill="none" stroke="{d}" stroke-width="26" stroke-linecap="round"/><rect x="140" y="300" width="70" height="120" rx="28" fill="{d}"/><rect x="390" y="300" width="70" height="120" rx="28" fill="{d}"/><rect x="160" y="320" width="30" height="80" rx="12" fill="{l}"/><rect x="410" y="320" width="30" height="80" rx="12" fill="{l}"/>',
 "earbuds": '<rect x="190" y="290" width="220" height="140" rx="60" fill="{d}"/><line x1="200" y1="340" x2="400" y2="340" stroke="{l}" stroke-width="6"/><circle cx="240" cy="210" r="42" fill="{d}"/><rect x="226" y="230" width="28" height="70" rx="14" fill="{d}"/><circle cx="360" cy="210" r="42" fill="{d}"/><rect x="346" y="230" width="28" height="70" rx="14" fill="{d}"/><circle cx="240" cy="210" r="18" fill="{l}"/><circle cx="360" cy="210" r="18" fill="{l}"/>',
 "phone": '<rect x="215" y="110" width="170" height="340" rx="30" fill="{d}"/><rect x="230" y="135" width="140" height="285" rx="12" fill="{l}"/><rect x="275" y="122" width="50" height="6" rx="3" fill="{l}"/><circle cx="300" cy="436" r="7" fill="{l}"/>',
 "tablet": '<rect x="150" y="130" width="300" height="340" rx="26" fill="{d}"/><rect x="170" y="152" width="260" height="290" rx="8" fill="{l}"/><circle cx="300" cy="456" r="7" fill="{l}"/>',
 "watch": '<rect x="255" y="120" width="90" height="360" rx="22" fill="{d}"/><rect x="215" y="205" width="170" height="190" rx="46" fill="{d}"/><rect x="232" y="222" width="136" height="156" rx="34" fill="{l}"/><line x1="300" y1="300" x2="300" y2="255" stroke="{d}" stroke-width="8" stroke-linecap="round"/><line x1="300" y1="300" x2="330" y2="318" stroke="{d}" stroke-width="8" stroke-linecap="round"/>',
 "charger": '<rect x="210" y="190" width="180" height="200" rx="26" fill="{d}"/><rect x="250" y="140" width="22" height="60" rx="6" fill="{d}"/><rect x="328" y="140" width="22" height="60" rx="6" fill="{d}"/><path d="M310 230l-50 75h38l-14 60 52-80h-38z" fill="{l}"/><path d="M300 390v50q0 30 40 30h60" fill="none" stroke="{d}" stroke-width="14" stroke-linecap="round"/>',
 "lamp": '<rect x="200" y="420" width="200" height="26" rx="13" fill="{d}"/><line x1="300" y1="425" x2="250" y2="290" stroke="{d}" stroke-width="16" stroke-linecap="round"/><line x1="250" y1="290" x2="340" y2="190" stroke="{d}" stroke-width="16" stroke-linecap="round"/><path d="M310 150l120 40-50 70z" fill="{d}"/><circle cx="396" cy="232" r="22" fill="{l}"/>',
 "chair": '<rect x="210" y="120" width="180" height="170" rx="30" fill="{d}"/><rect x="190" y="290" width="220" height="40" rx="18" fill="{d}"/><rect x="290" y="330" width="20" height="70" fill="{d}"/><path d="M200 440l100-40 100 40" fill="none" stroke="{d}" stroke-width="16" stroke-linecap="round"/><circle cx="200" cy="448" r="12" fill="{d}"/><circle cx="400" cy="448" r="12" fill="{d}"/><rect x="232" y="145" width="136" height="120" rx="20" fill="{l}" opacity=".5"/>',
 "mug": '<rect x="180" y="190" width="200" height="230" rx="24" fill="{d}"/><path d="M380 240h30a45 45 0 0 1 0 90h-30" fill="none" stroke="{d}" stroke-width="24"/><rect x="200" y="250" width="160" height="40" rx="8" fill="{l}"/><path d="M240 130q20 25 0 45M300 120q20 25 0 50" fill="none" stroke="{l}" stroke-width="10" stroke-linecap="round"/>',
 "coffee": '<rect x="170" y="120" width="260" height="70" rx="16" fill="{d}"/><rect x="190" y="190" width="220" height="200" fill="{d}"/><rect x="150" y="390" width="300" height="40" rx="14" fill="{d}"/><rect x="250" y="190" width="100" height="30" fill="{l}"/><rect x="255" y="300" width="90" height="85" rx="10" fill="{l}"/><circle cx="380" cy="155" r="14" fill="{l}"/>',
 "plant": '<path d="M220 340h160l-22 110H242z" fill="{d}"/><path d="M300 340V230" stroke="{d}" stroke-width="12"/><path d="M300 260c-80 0-110-60-100-120 60 0 100 50 100 120zM300 240c70 0 110-50 105-110-60 5-100 45-105 110z" fill="{d}"/><path d="M300 250c-40-20-60-50-70-90M300 232c35-20 60-50 75-80" stroke="{l}" stroke-width="6" fill="none"/>',
 "shirt": '<path d="M230 140l-110 60 40 80 40-20v190h200V260l40 20 40-80-110-60q-30 40-70 40t-70-40z" fill="{d}"/><path d="M250 150q50 50 100 0" fill="none" stroke="{l}" stroke-width="8"/>',
 "hoodie": '<path d="M230 150l-110 60 30 210h50V280v170h200V280v140h50l30-210-110-60z" fill="{d}"/><path d="M230 150q70-70 140 0q-10 70-70 70t-70-70z" fill="{d}" stroke="{l}" stroke-width="6"/><rect x="240" y="340" width="120" height="60" rx="12" fill="{l}" opacity=".6"/><line x1="285" y1="220" x2="285" y2="280" stroke="{l}" stroke-width="5"/><line x1="315" y1="220" x2="315" y2="280" stroke="{l}" stroke-width="5"/>',
 "shoe": '<path d="M120 380q0-70 40-120l60-20q30 50 90 50 70 0 140 50 30 20 30 50v20H120z" fill="{d}"/><rect x="120" y="400" width="360" height="24" rx="10" fill="{l}"/><path d="M230 270l20 30M260 275l18 28M290 282l14 26" stroke="{l}" stroke-width="7" stroke-linecap="round"/>',
 "backpack": '<path d="M250 150a50 50 0 0 1 100 0" fill="none" stroke="{d}" stroke-width="16"/><rect x="180" y="150" width="240" height="300" rx="60" fill="{d}"/><rect x="215" y="300" width="170" height="110" rx="20" fill="{l}" opacity=".6"/><line x1="230" y1="330" x2="370" y2="330" stroke="{d}" stroke-width="6"/>',
 "glasses": '<circle cx="210" cy="310" r="75" fill="{d}"/><circle cx="390" cy="310" r="75" fill="{d}"/><path d="M285 300q15-20 30 0" stroke="{d}" stroke-width="14" fill="none"/><line x1="135" y1="290" x2="100" y2="250" stroke="{d}" stroke-width="12" stroke-linecap="round"/><line x1="465" y1="290" x2="500" y2="250" stroke="{d}" stroke-width="12" stroke-linecap="round"/><path d="M175 280q20-25 50-25" stroke="{l}" stroke-width="10" fill="none" stroke-linecap="round"/><path d="M355 280q20-25 50-25" stroke="{l}" stroke-width="10" fill="none" stroke-linecap="round"/>',
 "ball": '<circle cx="300" cy="300" r="150" fill="{d}"/><path d="M300 245l52 38-20 62h-64l-20-62z" fill="{l}"/><path d="M300 245V160M352 283l80-26M332 345l50 70M268 345l-50 70M248 283l-80-26" stroke="{l}" stroke-width="10"/>',
 "mat": '<rect x="150" y="260" width="300" height="140" rx="20" fill="{d}"/><circle cx="450" cy="260" r="70" fill="{d}"/><circle cx="450" cy="260" r="40" fill="{l}" opacity=".6"/><line x1="170" y1="300" x2="420" y2="300" stroke="{l}" stroke-width="6" opacity=".7"/><line x1="170" y1="340" x2="420" y2="340" stroke="{l}" stroke-width="6" opacity=".7"/>',
 "bottle": '<rect x="265" y="110" width="70" height="50" rx="10" fill="{d}"/><path d="M250 175q0-15 15-15h70q15 0 15 15v20q40 30 40 80v170q0 20-20 20H230q-20 0-20-20V275q0-50 40-80z" fill="{d}"/><rect x="225" y="290" width="150" height="90" rx="10" fill="{l}" opacity=".6"/>',
 "dumbbell": '<rect x="200" y="285" width="200" height="30" rx="10" fill="{d}"/><rect x="120" y="220" width="50" height="160" rx="12" fill="{d}"/><rect x="170" y="240" width="35" height="120" rx="10" fill="{d}"/><rect x="430" y="220" width="50" height="160" rx="12" fill="{d}"/><rect x="395" y="240" width="35" height="120" rx="10" fill="{d}"/><rect x="132" y="240" width="12" height="120" rx="6" fill="{l}" opacity=".6"/><rect x="442" y="240" width="12" height="120" rx="6" fill="{l}" opacity=".6"/>',
 "book": '<path d="M180 130h220q20 0 20 20v300H200q-20 0-20-20z" fill="{d}"/><path d="M200 410h220v40H200a20 20 0 0 1 0-40z" fill="{l}"/><rect x="220" y="180" width="160" height="14" rx="7" fill="{l}"/><rect x="220" y="210" width="120" height="10" rx="5" fill="{l}" opacity=".7"/><rect x="220" y="290" width="160" height="60" rx="10" fill="{l}" opacity=".35"/>',
}

def svg(icon, c1, c2, variant):
    angles = [(0, 0, 600, 600), (600, 0, 0, 600), (0, 600, 600, 0)]
    x1, y1, x2, y2 = angles[variant]
    deco = ["", '<circle cx="490" cy="110" r="70" fill="#fff" opacity=".12"/><circle cx="90" cy="520" r="110" fill="#fff" opacity=".08"/>',
            '<rect x="-60" y="420" width="400" height="400" rx="60" transform="rotate(-20)" fill="#fff" opacity=".08"/>'][variant]
    transform = ["", 'transform="rotate(-8 300 300)"', 'transform="translate(300 300) scale(.85) translate(-300 -300)"'][variant]
    body = ICONS[icon].replace("{d}", W).replace("{l}", c2)
    return f'''<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 600 600" width="600" height="600">
<defs><linearGradient id="g" gradientUnits="userSpaceOnUse" x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}"><stop offset="0" stop-color="{c1}"/><stop offset="1" stop-color="{c2}"/></linearGradient></defs>
<rect width="600" height="600" fill="url(#g)"/>{deco}
<ellipse cx="300" cy="490" rx="170" ry="18" fill="#000" opacity=".15"/>
<g {transform}>{body}</g>
</svg>'''

colors = {c[0]: (c[2], c[3]) for c in CATEGORIES}
for p in PRODUCTS:
    slug, cat, icon = p[0], p[2], p[3]
    c1, c2 = colors[cat]
    for v in range(3):
        with open(os.path.join(OUT, f"{slug}-{v+1}.svg"), "w", encoding="utf-8") as f:
            f.write(svg(icon, c1, c2, v))
with open(os.path.join(OUT, "placeholder.svg"), "w", encoding="utf-8") as f:
    f.write(svg("book", "#94a3b8", "#334155", 0).replace(ICONS["book"].replace("{d}", W).replace("{l}", "#334155"),
            '<text x="300" y="320" text-anchor="middle" font-family="sans-serif" font-size="48" fill="#fff">Image</text>'))
print(len(PRODUCTS) * 3 + 1, "images générées")
