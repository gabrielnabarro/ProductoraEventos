
const $ = (s) => document.querySelector(s);

const esc = (v) => String(v).replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;").replaceAll('"', "&quot;").replaceAll("'", "&#39;");
const fmtDate = (v, full = false) => !v ? "Fecha a confirmar" : new Intl.DateTimeFormat("es-AR", full ? { dateStyle: "full", timeStyle: "short" } : { dateStyle: "medium" }).format(new Date(v));
const fmtMoney = (v) => new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS", maximumFractionDigits: 0 }).format(v || 0);

const readJson = async (r) => {
    if (r.ok) return r.json();
    let m = null;
    try { const p = await r.json(); m = p.message || p.title || p.detail; } catch {}
    const e = new Error(m || "No pudimos completar la solicitud.");
    e.status = r.status;
    throw e;
};

const fetchJson = (u, o) => fetch(u, o).then(readJson);

const resolveErrorMessage = (error, fallback) => {
    const message = String(error?.message || "").trim();
    return !message || message === "Failed to fetch" ? fallback : message;
};
