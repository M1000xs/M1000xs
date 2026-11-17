// Galerie de la page produit : clic sur une vignette => change l'image principale.
document.addEventListener('click', function (e) {
    const thumb = e.target.closest('.gallery .thumb');
    if (!thumb) return;
    document.getElementById('mainImage').src = thumb.dataset.src;
    document.querySelectorAll('.gallery .thumb').forEach(t => t.classList.toggle('active', t === thumb));
});
