// Devuelve el primer elemento que coincide con un selector.
const $ = (s) => document.querySelector(s);
// Escapa texto para poder insertarlo en HTML sin romper la vista.
const esc = (v) => String(v).replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;").replaceAll('"', "&quot;").replaceAll("'", "&#39;");
const fmtDate = (v, full = false) => !v ? "Fecha a confirmar" : new Intl.DateTimeFormat("es-AR", full ? { dateStyle: "full", timeStyle: "short" } : { dateStyle: "medium" }).format(new Date(v));
const fmtMoney = (v) => new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS", maximumFractionDigits: 0 }).format(v || 0);
// Lee la respuesta de fetch y lanza un error si la API devolvio un fallo.
const readJson = async (r) => {
    if (r.ok) return r.json();
    let m = null;
    try { const p = await r.json(); m = p.message || p.title || p.detail; } catch {}
    const e = new Error(m || "No pudimos completar la solicitud.");
    e.status = r.status;
    throw e;
};
// Ejecuta fetch y procesa la respuesta JSON en un solo paso.
const fetchJson = (u, o) => fetch(u, o).then(readJson);
// Traduce errores tecnicos a un mensaje de fallback para el usuario.
const resolveErrorMessage = (error, fallback) => {
    const message = String(error?.message || "").trim();
    return !message || message === "Failed to fetch" ? fallback : message;
};
