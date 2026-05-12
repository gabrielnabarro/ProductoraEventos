const el = { title: $("#detail-title"), feedback: $("#detail-feedback"), summary: $("#event-summary"), choice: $("#selected-seat-summary"), pay: $("#payment-button"), map: $("#seat-map"), modal: $("#reserve-confirm-modal"), modalKicker: $("#reserve-confirm-kicker"), modalTitle: $("#reserve-confirm-title"), modalText: $("#reserve-confirm-text"), modalConfirm: $("#reserve-confirm-button"), modalCancel: $("#reserve-cancel-button") };
const headers = { headers: { Accept: "application/json" } };
const seats = { Available: ["available", "fa-circle-check", "Disponible"], Reserved: ["reserved", "fa-hourglass-half", "Reservado"], Sold: ["sold", "fa-ban", "Vendido"] };
const state = { id: Number(new URLSearchParams(location.search).get("id")), event: null, sectors: [], activeReservation: null, reserving: null, pendingSeatId: null };

document.addEventListener("DOMContentLoaded", () => {
    el.map.onclick = (e) => {
        const seat = e.target.closest("button[data-seat-id]");
        if (seat && !seat.disabled && !state.reserving) openReserveConfirm(seat.dataset.seatId);
    };

    el.modal.onclick = (e) => {
        if (e.target.dataset.closeModal === "true") closeReserveConfirm();
    };

    el.modalCancel.onclick = () => closeReserveConfirm();

    el.modalConfirm.onclick = () => {
        if (!state.pendingSeatId || state.reserving) return;
        const seatId = state.pendingSeatId;
        closeReserveConfirm();
        reserve(seatId);
    };

    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape" && state.pendingSeatId) closeReserveConfirm();
    });

    // --- CÓDIGO DE REDIRECCIÓN AL PAGO ---
    el.pay.addEventListener("click", (e) => {
        e.preventDefault();
        if (state.id) {
            window.location.assign(`/payment.html?eventId=${state.id}`);
        }
    });

    init();
});

const session = () => ticketixAuth.getSession();
const userId = () => session()?.id || 0;
const cloneSectors = (list) => list.map((s) => ({ ...s, seats: (s.seats || []).map((seat) => ({ ...seat })) }));
const seatState = (status) => seats[status]?.[0] || "sold";
const setFeedback = (msg = "", type = "is-hidden") => { el.feedback.className = `notification ${type} mb-5`; el.feedback.textContent = msg; };
const getSeat = (id) => { for (const sector of state.sectors) { const seat = sector.seats.find((s) => String(s.id) === String(id)); if (seat) return { sector, seat }; } return null; };
const reservationsUrl = () => `/api/v1/users/${userId()}/reservations?eventId=${state.id}&status=Pending`;
const selectedSeatId = () => state.activeReservation?.seatId || null;
const isReservationChange = (seatId) => !!state.activeReservation && String(state.activeReservation.seatId) !== String(seatId);
const reservationSeatLabel = (reservation) => reservation ? `${reservation.seatRowIdentifier}${reservation.seatNumber}` : "";
const setActiveReservation = (reservations) => { state.activeReservation = Array.isArray(reservations) && reservations.length ? reservations[0] : null; };
const setSeatStatus = (seatId, status) => { state.sectors.forEach((sector) => sector.seats.forEach((seat) => { if (String(seat.id) === String(seatId)) seat.status = status; })); };

async function fetchReservationSnapshot() {
    if (!userId()) return { reservations: [], error: null };
    try {
        return { reservations: await fetchJson(reservationsUrl(), headers), error: null };
    } catch (error) {
        return { reservations: [], error };
    }
}

function buildReservationFallback(seatId, data) {
    const hit = getSeat(seatId);
    if (!hit) return null;
    return {
        reservationId: data.reservationId,
        userId: data.userId,
        eventId: state.id,
        seatId: data.seatId || seatId,
        sectorId: hit.sector.id,
        sectorName: hit.sector.name,
        price: hit.sector.price,
        seatStatus: data.seatStatus || "Reserved",
        seatRowIdentifier: hit.seat.rowIdentifier,
        seatNumber: hit.seat.seatNumber,
        reservationStatus: data.reservationStatus || "Pending",
        reservedAt: data.reservedAt,
        expiresAt: data.expiresAt
    };
}

function applyModalContent(seatId, hit) {
    if (isReservationChange(seatId)) {
        const current = state.activeReservation;
        el.modalKicker.textContent = "Confirmar cambio";
        el.modalTitle.textContent = "Cambiar reserva";
        el.modalText.textContent = `Vas a cambiar tu reserva de la butaca ${reservationSeatLabel(current)} a la butaca ${hit.seat.rowIdentifier}${hit.seat.seatNumber}. Se liberara tu reserva anterior del Sector ${current.sectorName} y se reservara la nueva en Sector ${hit.sector.name} - ${fmtMoney(hit.sector.price)}.`;
        el.modalConfirm.textContent = "Si, cambiar";
        return;
    }

    el.modalKicker.textContent = "Confirmar reserva";
    el.modalTitle.textContent = "Reservar butaca";
    el.modalText.textContent = `Vas a reservar la butaca ${hit.seat.rowIdentifier}${hit.seat.seatNumber}. Entrada Sector ${hit.sector.name} - ${fmtMoney(hit.sector.price)}.`;
    el.modalConfirm.textContent = "Si, reservar";
}

function openReserveConfirm(seatId) {
    if (!ticketixAuth.requireAuth(`${location.pathname}${location.search}`)) return;
    const hit = getSeat(seatId);
    if (!hit) return;
    state.pendingSeatId = seatId;
    applyModalContent(seatId, hit);
    el.modal.classList.add("is-active");
    el.modal.setAttribute("aria-hidden", "false");
    el.modalConfirm.focus();
}

function closeReserveConfirm() {
    state.pendingSeatId = null;
    el.modal.classList.remove("is-active");
    el.modal.setAttribute("aria-hidden", "true");
}

const eventSummary = (event) => `<article class="detail-panel"><p class="panel-kicker">Evento</p><h3 class="panel-title">${esc(event.name)}</h3><div class="panel-meta"><p class="panel-meta-item"><span class="icon" aria-hidden="true"><i class="fa-solid fa-location-dot"></i></span><span>${esc(event.venue || "Lugar a confirmar")}</span></p><p class="panel-meta-item"><span class="icon" aria-hidden="true"><i class="fa-regular fa-clock"></i></span><span>${fmtDate(event.eventDate, true)}</span></p></div></article>`;

function render() {
    if (!state.event) return;
    el.title.textContent = state.event.name;
    el.summary.innerHTML = eventSummary(state.event);
    renderChoice();
    renderMap();
}

// Variable global para controlar el intervalo del reloj en esta pantalla
let eventTimerInterval = null;

function renderChoice(message = "") {
    const reservation = state.activeReservation;
    const logged = !!session();
    el.pay.disabled = !reservation || !!message || !logged;
    el.choice.className = `detail-panel detail-panel-selection${message ? " detail-panel-error" : ""}${reservation && !message ? " is-selected" : ""} mb-4`;

    // Inyectamos el diseño del contador dentro del HTML si hay reserva
    el.choice.innerHTML = message
        ? `<p class="panel-kicker">Tu reserva</p><p class="panel-copy mb-0">${esc(message)}</p>`
        : reservation
            ? `<p class="panel-kicker">Tu reserva</p>
               <div class="is-flex is-justify-content-space-between is-align-items-flex-start">
                   <div>
                       <p class="panel-seat">${esc(reservationSeatLabel(reservation))}</p>
                       <p class="panel-copy panel-copy-strong mb-0">Sector ${esc(reservation.sectorName)}</p>
                   </div>
                   <div class="has-text-right">
                       <p class="heading mb-1" style="font-size: 0.65rem;">Expira en</p>
                       <div id="event-timer-display" class="has-text-primary has-text-weight-bold" style="font-family: 'Space Grotesk', sans-serif; font-size: 1.5rem; line-height: 1;">05:00</div>
                   </div>
               </div>
               <p class="panel-copy mb-2 mt-1">${fmtMoney(reservation.price)}</p>
               <p class="panel-copy mb-0" style="font-size: 0.85rem;">Puedes cambiar tu reserva seleccionando otra butaca.</p>`
            : logged
                ? `<p class="panel-kicker">Tu reserva</p><p class="panel-copy mb-0">No tienes una reserva pendiente. Selecciona una butaca disponible para reservar.</p>`
                : `<p class="panel-kicker">Tu reserva</p><p class="panel-copy mb-0">Inicia sesion para reservar una butaca y continuar al pago.</p>`;

    // Disparamos el contador solo si tenemos una reserva activa y la UI se renderizó
    if (reservation && !message) {
        startEventTimer();
    } else {
        if (eventTimerInterval) clearInterval(eventTimerInterval);
    }
}

// Nueva función exclusiva para manejar el tiempo en la pantalla del mapa
function startEventTimer() {
    if (eventTimerInterval) clearInterval(eventTimerInterval);

    const reservation = state.activeReservation;
    if (!reservation || !reservation.expiresAt) return;

    const display = document.getElementById("event-timer-display");
    if (!display) return;

    // Calculamos el tiempo base leyendo la expiración provista por la API (C#)
    const expiresAt = new Date(reservation.expiresAt).getTime();

    const updateClock = () => {
        const now = new Date().getTime();
        const remaining = expiresAt - now;

        // Si el tiempo llega a cero o negativo
        if (remaining <= 0) {
            clearInterval(eventTimerInterval);
            display.textContent = "00:00";
            display.classList.replace("has-text-primary", "has-text-danger");

            // Creamos una notificación Toast al vuelo
            let toastC = document.getElementById("toast-container");
            if (!toastC) {
                toastC = document.createElement("div");
                toastC.id = "toast-container";
                document.body.appendChild(toastC);
            }
            toastC.innerHTML = `<div class="toast box is-danger"><div class="is-flex is-align-items-center"><span class="icon is-large has-text-danger mr-3"><i class="fa-solid fa-triangle-exclamation fa-2xl"></i></span><div><p class="has-text-weight-bold mb-0">Atención</p><p class="mb-0">El tiempo de tu reserva expiró. La butaca ha sido liberada.</p></div></div></div>`;
            setTimeout(() => { if (toastC.firstChild) toastC.firstChild.remove(); }, 4500);

            // Forzamos al mapa a recargar la información (la API ya no traerá esta reserva)
            refresh();
            return;
        }

        // Formateo de los minutos y segundos restantes
        const m = Math.floor((remaining % (1000 * 60 * 60)) / (1000 * 60));
        const s = Math.floor((remaining % (1000 * 60)) / 1000);

        display.textContent = `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;

        // Alerta visual: cuando queden 30 segundos, el texto se vuelve rojo
        if (m === 0 && s <= 30) {
            display.classList.replace("has-text-primary", "has-text-danger");
        }
    };

    // Ejecutamos la primera vez inmediatamente y luego cada 1000ms
    updateClock();
    eventTimerInterval = setInterval(updateClock, 1000);
}

function renderMap() {
    if (!state.sectors.length) {
        el.map.innerHTML = '<div class="notification is-light has-text-centered">Todavia no hay un mapa de butacas disponible para este evento.</div>';
        return;
    }
    el.map.innerHTML = state.sectors.map((sector) => `<section class="box mb-4"><header class="is-flex is-justify-content-space-between is-align-items-center mb-4 is-flex-wrap-wrap"><h3 class="title is-5 mb-0">${esc(sector.name)}</h3><span class="tag is-primary is-light is-medium">${fmtMoney(sector.price)}</span></header><div class="seats-grid">${rows(sector.seats).map((row) => `<div class="seat-row"><div class="row-seats"><div class="seat-cluster">${row.map(seatButton).join("")}</div></div></div>`).join("")}</div></section>`).join("");
}

const rows = (list) => { const map = list.reduce((m, seat) => ((m[seat.rowIdentifier || "?"] ||= []).push(seat), m), {}); return Object.keys(map).sort((a, b) => a.localeCompare(b, "es", { numeric: true })).map((key) => map[key].sort((a, b) => a.seatNumber - b.seatNumber)); };

function seatButton(seat) {
    const [kind, icon, label] = seats[seat.status] || seats.Sold, selected = String(selectedSeatId()) === String(seat.id), reserving = String(state.reserving) === String(seat.id);
    return `<button class="seat-chip ${kind}${selected ? " is-selected" : ""}${reserving ? " is-reserving" : ""}" type="button" data-seat-id="${seat.id}" title="${label}" aria-label="Butaca ${esc(seat.rowIdentifier)}${seat.seatNumber} - ${label}" ${kind !== "available" || reserving ? "disabled" : ""}><i class="fa-solid ${icon}"></i><span class="seat-code">${esc(seat.rowIdentifier)}${seat.seatNumber}</span></button>`;
}

async function init() {
    if (!state.id) return fail("El identificador del evento no es valido.");
    try {
        const [event, map, reservationSnapshot] = await Promise.all([fetchJson(`/api/v1/events/${state.id}`, headers), fetchJson(`/api/v1/events/${state.id}/seats`, headers), fetchReservationSnapshot()]);
        state.event = event;
        state.sectors = cloneSectors(map.sectors || []);
        setActiveReservation(reservationSnapshot.reservations);
        render();
        reservationSnapshot.error
            ? setFeedback(resolveErrorMessage(reservationSnapshot.error, "No se pudo obtener tu reserva actual."), "is-warning is-light")
            : setFeedback();
    } catch (e) { fail(resolveErrorMessage(e, "No se pudo obtener el detalle del evento.")); }
}

async function refresh() {
    const [map, reservationSnapshot] = await Promise.all([fetchJson(`/api/v1/events/${state.id}/seats`, headers), fetchReservationSnapshot()]);
    state.sectors = cloneSectors(map.sectors || []);
    setActiveReservation(reservationSnapshot.reservations);
    render();
    reservationSnapshot.error
        ? setFeedback(resolveErrorMessage(reservationSnapshot.error, "No se pudo obtener tu reserva actual."), "is-warning is-light")
        : setFeedback();
}

async function reserve(seatId) {
    closeReserveConfirm();
    if (!userId()) return;
    state.reserving = seatId; renderMap(); setFeedback();
    try {
        const data = await fetchJson("/api/v1/reservations", { method: "POST", headers: { Accept: "application/json", "Content-Type": "application/json" }, body: JSON.stringify({ seatId, userId: userId() }) });
        try {
            const [map, reservations] = await Promise.all([fetchJson(`/api/v1/events/${state.id}/seats`, headers), fetchJson(reservationsUrl(), headers)]);
            state.sectors = cloneSectors(map.sectors || []);
            setActiveReservation(reservations);
            if (!state.activeReservation) throw new Error("No se encontro la reserva actual del usuario.");
            setFeedback();
        } catch {
            if (state.activeReservation && String(state.activeReservation.seatId) !== String(seatId)) {
                setSeatStatus(state.activeReservation.seatId, "Available");
            }
            setSeatStatus(seatId, data.seatStatus || "Reserved");
            state.activeReservation = buildReservationFallback(seatId, data);
            render();
            setFeedback("La reserva se realizo, pero no se pudo recargar el mapa actualizado.", "is-warning is-light");
        }
    } catch (e) {
        // Bloque Try-Catch corregido sin errores de sintaxis
        if (e.status === 409) {
            await refresh().catch(() => { });

            let toastC = document.getElementById("toast-container");
            if (!toastC) {
                toastC = document.createElement("div");
                toastC.id = "toast-container";
                document.body.appendChild(toastC);
            }
            toastC.innerHTML = `<div class="toast box is-danger"><div class="is-flex is-align-items-center"><span class="icon is-large has-text-danger mr-3"><i class="fa-solid fa-triangle-exclamation fa-2xl"></i></span><div><p class="has-text-weight-bold mb-0">Atención</p><p class="mb-0">Asiento ya no disponible.</p></div></div></div>`;
            setTimeout(() => { if (toastC.firstChild) toastC.firstChild.remove(); }, 4000);

        } else {
            setFeedback(resolveErrorMessage(e, "No se pudo reservar la butaca."), "is-danger is-light");
        }
    }
    state.reserving = null; render();
}

function fail(message) {
    el.title.textContent = "Detalle no disponible";
    el.summary.innerHTML = `<article class="detail-panel detail-panel-error mb-4"><p class="panel-kicker">Error</p><h3 class="panel-title panel-title-sm">No se pudo abrir este evento</h3><p class="panel-copy mb-0">${esc(message)}</p><a class="button is-primary is-fullwidth detail-pay-button mt-4" href="/">Volver a la cartelera</a></article>`;
    el.map.innerHTML = ""; renderChoice(message); setFeedback(message, "is-danger is-light");
}