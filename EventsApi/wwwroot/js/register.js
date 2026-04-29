const registerForm = $("#register-form"), registerFeedback = $("#auth-feedback"), registerSubmit = $("#register-submit"), loginSwitchLink = $("#login-switch-link");

document.addEventListener("DOMContentLoaded", () => {
    ticketixAuth.redirectIfAuthenticated("/");
    if (loginSwitchLink) loginSwitchLink.href = ticketixAuth.buildAuthUrl("/login.html", ticketixAuth.getRedirectTarget("/"));
    registerForm?.addEventListener("submit", submitRegister);
});

// Envia el formulario de registro y crea la sesion del usuario nuevo.
async function submitRegister(e) {
    e.preventDefault();
    registerSubmit.disabled = true;
    registerSubmit.textContent = "Creando cuenta...";
    setRegisterFeedback();

    const formData = new FormData(registerForm);
    try {
        const user = await fetchJson("/api/v1/auth/register", {
            method: "POST",
            headers: { Accept: "application/json", "Content-Type": "application/json" },
            body: JSON.stringify({ name: formData.get("name"), email: formData.get("email"), password: formData.get("password") })
        });
        ticketixAuth.setSession(user);
        ticketixAuth.redirectAfterAuth("/");
    } catch (error) {
        setRegisterFeedback(resolveErrorMessage(error, "No se pudo crear la cuenta."), "is-danger is-light");
    }

    registerSubmit.disabled = false;
    registerSubmit.textContent = "Crear cuenta";
}

// Muestra mensajes de error o estado dentro del formulario de registro.
function setRegisterFeedback(message = "", type = "is-hidden") {
    registerFeedback.className = `notification ${type} mb-4`;
    registerFeedback.textContent = message;
}
