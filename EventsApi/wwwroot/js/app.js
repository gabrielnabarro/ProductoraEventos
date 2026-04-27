const grid = $("#events-grid"), feedback = $("#events-feedback"), prev = $("#pagination-prev"), next = $("#pagination-next"), status = $("#pagination-status"), size = 9;
let page = 1, pages = 1;

document.addEventListener("DOMContentLoaded", () => {
    prev.onclick = () => page > 1 && load(page - 1);
    next.onclick = () => page < pages && load(page + 1);
    load();
});

// Muestra mensajes informativos o de error arriba de la grilla.
const setFeedback = (msg = "", type = "is-hidden") => {
    feedback.className = `notification ${type} mb-5`;
    feedback.textContent = msg;
};

// Actualiza el estado visual de la paginacion segun la pagina actual.
const paintPager = (p = page, t = pages, loading = false) => {
    status.textContent = loading ? `Cargando pagina ${p}...` : `Pagina ${p} de ${t}`;
    prev.disabled = loading || p <= 1;
    next.disabled = loading || p >= t;
};

// Genera una card placeholder mientras llegan los eventos desde la API.
const skeleton = () => `<div class="column is-6-tablet is-6-desktop is-4-widescreen"><article class="card event-card"><div class="card-content"><p class="panel-kicker">Evento</p><h2 class="panel-title">Cargando evento</h2><div class="panel-meta"><p class="panel-meta-item"><span class="icon" aria-hidden="true"><i class="fa-solid fa-location-dot"></i></span><span>Buscando venue...</span></p><p class="panel-meta-item"><span class="icon" aria-hidden="true"><i class="fa-regular fa-calendar"></i></span><span>Confirmando fecha...</span></p></div><a class="button is-primary is-fullwidth event-card-cta mt-auto" aria-disabled="true">Ver detalles</a></div></article></div>`;
// Genera el HTML de una card real a partir de un evento recibido.
const card = (e) => `<div class="column is-6-tablet is-6-desktop is-4-widescreen"><article class="card event-card"><div class="card-content"><p class="panel-kicker">Evento</p><h2 class="panel-title">${esc(e.name)}</h2><div class="panel-meta"><p class="panel-meta-item"><span class="icon" aria-hidden="true"><i class="fa-solid fa-location-dot"></i></span><span>${esc(e.venue || "Lugar a confirmar")}</span></p><p class="panel-meta-item"><span class="icon" aria-hidden="true"><i class="fa-regular fa-calendar"></i></span><span>${fmtDate(e.eventDate)}</span></p></div><a class="button is-primary is-fullwidth event-card-cta mt-auto" href="/event.html?id=${e.id}">Ver detalles</a></div></article></div>`;

// Consulta una pagina de eventos y actualiza la grilla y la paginacion.
async function load(target = page) {
    grid.innerHTML = Array.from({ length: size }, skeleton).join("");
    paintPager(target, pages, true);
    try {
        const { items = [], page: current = target, totalPages = 1 } = await fetchJson(`/api/v1/events?page=${target}&pageSize=${size}`, { headers: { Accept: "application/json" } });
        page = current; pages = Math.max(totalPages, 1); grid.innerHTML = items.map(card).join("");
        setFeedback(items.length ? "" : "No hay eventos activos para mostrar en este momento.", items.length ? "is-hidden" : "is-warning is-light");
    } catch (e) {
        grid.innerHTML = "";
        setFeedback(resolveErrorMessage(e, "No se pudieron obtener los eventos."), "is-danger is-light");
    }
    paintPager(page, pages);
}
