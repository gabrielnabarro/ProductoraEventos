// Centraliza la sesion del usuario y la navegacion asociada al login.
const ticketixAuth = (() => {
    const storageKey = "ticketix:session";
    // Acepta solo redirecciones internas para evitar rutas invalidas.
    const sanitizeRedirect = (value, fallback = "/") => value && value.startsWith("/") && !value.startsWith("//") ? value : fallback;
  
    const currentUrl = () => `${location.pathname}${location.search}`;

    const isAuthPage = () => location.pathname.endsWith("/login.html") || location.pathname.endsWith("/register.html");

    // Lectura y validación de sesion guardada en localStorage
    const getSession = () => {
        try {
            const raw = localStorage.getItem(storageKey);
            if (!raw) return null;
            const parsed = JSON.parse(raw);
            if (!parsed || typeof parsed.id !== "number") return null;
            return { id: parsed.id, name: String(parsed.name || ""), email: String(parsed.email || "") };
        } catch {
            return null;
        }
    };


    const setSession = (user) => {
        const session = { id: Number(user.id), name: String(user.name || ""), email: String(user.email || "") };
        localStorage.setItem(storageKey, JSON.stringify(session));
        renderAuthNav();
        return session;
    };

    const clearSession = () => {
        localStorage.removeItem(storageKey);
        renderAuthNav();
    };

    // Redirecciones
    const getRedirectTarget = (fallback = "/") => sanitizeRedirect(new URLSearchParams(location.search).get("redirect"), fallback);

    const buildAuthUrl = (path, redirect = currentUrl()) => `${path}?redirect=${encodeURIComponent(sanitizeRedirect(redirect, "/"))}`;

    const redirectToLogin = (redirect = currentUrl()) => { location.href = buildAuthUrl("/login.html", redirect); };
    
    const redirectAfterAuth = (fallback = "/") => { location.href = getRedirectTarget(fallback); };

    const redirectIfAuthenticated = (fallback = "/") => {
        if (getSession()) {
            location.href = getRedirectTarget(fallback);
        }
    };

    // Renderiza la barra de navegación según el estado de sesión.
    function renderAuthNav() {
        const session = getSession();
        document.querySelectorAll("[data-auth-nav]").forEach((container) => {
            const authView = String(container.dataset.authView || "").toLowerCase();
            container.innerHTML = session
                ? `<div class="auth-nav-group"><span class="auth-user-chip"><span class="icon" aria-hidden="true"><i class="fa-regular fa-user"></i></span><span>Hola, ${esc(session.name)}</span></span><button class="button topbar-link auth-nav-link auth-nav-link-danger" type="button" data-logout-button>Cerrar sesion</button></div>`
                : `<div class="auth-nav-group"><a class="button topbar-link auth-nav-link${authView === "login" ? " auth-nav-link-current" : ""}" href="${buildAuthUrl("/login.html")}">Iniciar sesion</a><a class="button topbar-link auth-nav-link auth-nav-link-primary${authView === "register" ? " auth-nav-link-current" : ""}" href="${buildAuthUrl("/register.html")}">Crear usuario</a></div>`;
        });

        document.querySelectorAll("[data-logout-button]").forEach((button) => {
            button.onclick = () => {
                clearSession();
                if (isAuthPage()) {
                    location.href = "/";
                    return;
                }
                location.reload();
            };
        });
    }

    // Verifica si hay sesion; si no existe, envia al login.
    const requireAuth = (redirect = currentUrl()) => {
        if (getSession()) return true;
        redirectToLogin(redirect);
        return false;
    };

    return { getSession, setSession, clearSession, getRedirectTarget, buildAuthUrl, redirectToLogin, redirectAfterAuth, redirectIfAuthenticated, renderAuthNav, requireAuth };
})();

window.ticketixAuth = ticketixAuth;
document.addEventListener("DOMContentLoaded", () => ticketixAuth.renderAuthNav());
