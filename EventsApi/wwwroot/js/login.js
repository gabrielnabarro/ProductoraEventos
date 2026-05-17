const loginForm = $("#login-form"), loginFeedback = $("#auth-feedback"), loginSubmit = $("#login-submit"), registerSwitchLink = $("#register-switch-link");

document.addEventListener("DOMContentLoaded", () => {
    ticketixAuth.redirectIfAuthenticated("/");
    if (registerSwitchLink) registerSwitchLink.href = ticketixAuth.buildAuthUrl("/register.html", ticketixAuth.getRedirectTarget("/"));
    loginForm?.addEventListener("submit", submitLogin);
});

async function submitLogin(e) {
    e.preventDefault();
    loginSubmit.disabled = true;
    loginSubmit.textContent = "Ingresando...";
    setLoginFeedback();

    const formData = new FormData(loginForm);
    try {
        const user = await fetchJson("/api/v1/auth/login", {
            method: "POST",
            headers: { Accept: "application/json", "Content-Type": "application/json" },
            body: JSON.stringify({ email: formData.get("email"), password: formData.get("password") })
        });
        ticketixAuth.setSession(user);
        ticketixAuth.redirectAfterAuth("/");
    } catch (error) {
        setLoginFeedback(resolveErrorMessage(error, "No se pudo iniciar sesión."), "is-danger is-light");
    }

    loginSubmit.disabled = false;
    loginSubmit.textContent = "Entrar";
}

function setLoginFeedback(message = "", type = "is-hidden") {
    loginFeedback.className = `notification ${type} mb-4`;
    loginFeedback.textContent = message;
}
