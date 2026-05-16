const el = {
    timer: $("#countdown-timer"),
    progress: $("#timer-progress"),
    summary: $("#reservation-summary"),
    payBtn: $("#pay-button"),
    cancelLink: $("#cancel-link"),
    toastContainer: $("#toast-container")
};

const state = {
    reservation: null,
    interval: null
};

document.addEventListener("DOMContentLoaded", () => {
    if (!ticketixAuth.requireAuth()) return;
    el.payBtn.onclick = handlePayment;
    loadReservation();
});


function showToast(message, type = "is-danger") {
    const toast = document.createElement("div");
    toast.className = `toast box ${type}`;
    const icon = type === 'is-danger' ? 'fa-triangle-exclamation' : 'fa-circle-check';
    const colorClass = type === 'is-danger' ? 'has-text-danger' : 'has-text-primary';
    
    toast.innerHTML = `
        <div class="is-flex is-align-items-center">
            <span class="icon is-large mr-3 ${colorClass}"><i class="fa-solid ${icon} fa-2xl"></i></span>
            <div>
                <p class="has-text-weight-bold mb-0">${type === 'is-danger' ? 'Atención' : 'Operación Exitosa'}</p>
                <p class="mb-0">${esc(message)}</p>
            </div>
        </div>`;
        
    el.toastContainer.appendChild(toast);
    setTimeout(() => {
        toast.style.opacity = "0";
        setTimeout(() => toast.remove(), 300);
    }, 4500);
}

async function loadReservation() {
    const userId = ticketixAuth.getSession()?.id;
    if (!userId) return;

    const urlParams = new URLSearchParams(location.search);
    const eventId = urlParams.get('eventId');
    
    try {
        const endpoint = eventId 
            ? `/api/v1/users/${userId}/reservations?eventId=${eventId}&status=Pending`
            : `/api/v1/users/${userId}/reservations?status=Pending`;

        const reservations = await fetchJson(endpoint, { headers: { Accept: "application/json" } });
        
        if (!reservations || reservations.length === 0) {
            renderEmpty("No se encontraron reservas pendientes o la reserva expiró.");
            return;
        }

        state.reservation = reservations[0];
        el.cancelLink.href = `/event.html?id=${state.reservation.eventId}`;
        
        renderSummary();
        startTimer();
        el.payBtn.disabled = false;
    } catch (error) {
        showToast("Error al cargar la información del carrito.", "is-danger");
        renderEmpty("Ocurrió un problema al cargar tu reserva.");
    }
}

function renderSummary() {
    const r = state.reservation;
    el.summary.innerHTML = `
        <p class="panel-kicker">Butaca Reservada</p>
        <p class="panel-seat">${esc(r.seatRowIdentifier)}${r.seatNumber}</p>
        <p class="panel-copy panel-copy-strong mb-3">Sector ${esc(r.sectorName)}</p>
        <div class="is-flex is-justify-content-space-between is-align-items-center pt-3 mt-3" style="border-top: 1px solid rgba(0,0,0,0.05);">
            <span class="has-text-weight-bold has-text-grey" style="letter-spacing: 0.1em; text-transform: uppercase; font-size: 0.85rem;">Total a Pagar</span>
            <span class="title is-4 mb-0 has-text-primary" style="font-family: 'Space Grotesk', sans-serif;">${fmtMoney(r.price)}</span>
        </div>`;
}

function renderEmpty(message) {
    el.summary.innerHTML = `<p class="has-text-centered py-3 mb-0">${esc(message)}</p>`;
    el.timer.textContent = "00:00";
    el.progress.value = 0;
    el.payBtn.disabled = true;
}

function startTimer() {
    if (state.interval) clearInterval(state.interval);

    const expiresAt = new Date(state.reservation.expiresAt).getTime();
    const reservedAt = new Date(state.reservation.reservedAt).getTime();
    const totalMs = expiresAt - reservedAt;

    const updateClock = () => {
        const now = new Date().getTime();
        const remaining = Math.max(0, expiresAt - now);

        if (remaining <= 0 || totalMs <= 0) {
            clearInterval(state.interval);
            state.interval = null;
            el.timer.textContent = "Reserva expirada";
            el.timer.classList.add("timer-danger");
            el.progress.value = 0;
            el.progress.classList.replace("is-primary", "is-danger");
            el.payBtn.disabled = true;
            
            showToast("La reserva ya no esta disponible.");
            setTimeout(() => { location.href = el.cancelLink.href; }, 3500);
            return;
        }

        const m = Math.floor(remaining / (1000 * 60));
        const s = Math.floor((remaining % (1000 * 60)) / 1000);
        
        el.timer.textContent = `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
        el.progress.value = Math.min(100, Math.max(0, (remaining / totalMs) * 100));

        if (m === 0 && s <= 30) {
            el.timer.classList.add("timer-danger");
            el.progress.classList.replace("is-primary", "is-danger");
        }
    };

    updateClock();
    state.interval = setInterval(updateClock, 1000);
}

async function handlePayment() {
    if (!state.reservation) return;
    el.payBtn.disabled = true;
    el.payBtn.classList.add("is-loading");

    try {
     
        const currentSession = ticketixAuth.getSession();

        if (!currentSession || !currentSession.id) {
            showToast("No se encontró una sesión activa. Por favor, vuelve a iniciar sesión.", "is-danger");
            setTimeout(() => { location.href = "/login.html"; }, 2500);
            return;
        }

        await fetchJson(`/api/v1/payments`, {
            method: "POST",
            headers: { Accept: "application/json", "Content-Type": "application/json" },
            body: JSON.stringify({
                reservationId: state.reservation.reservationId,
                userId: currentSession.id 
            })
        });


        clearInterval(state.interval);
        showToast("¡Pago procesado correctamente! Tu compra ha sido confirmada.", "is-success");
        setTimeout(() => { location.href = "/"; }, 3000);

    } catch (error) {
        el.payBtn.classList.remove("is-loading");

 
        if (error.status === 409) {
            clearInterval(state.interval);
            el.payBtn.disabled = true;
            showToast("Asiento ya no disponible. La reserva fue procesada por otro usuario o ha caducado.", "is-danger");
            setTimeout(() => { location.href = el.cancelLink.href; }, 3500);
        } else {

            showToast(resolveErrorMessage(error, "No se pudo procesar el pago."), "is-danger");
            el.payBtn.disabled = false;
        }
    }
}
