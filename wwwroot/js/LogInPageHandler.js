// 👁 Переключение видимости пароля
function togglePassword() {
    const input = document.getElementById("password");
    const icon = document.querySelector(".toggle-password");
    input.type = input.type === "password" ? "text" : "password";
    icon.textContent = input.type === "password" ? "👁" : "🙈";
}

// 🚨 Простая защита от XSS при выводе сообщений
function sanitize(text) {
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
}

// 📜 Загружаем CSRF токен с сервера при старте страницы
async function loadCsrfToken() {
    try {
        const res = await fetch("/csrf-token", { credentials: "include" });
        if (res.ok) {
            const data = await res.json();
            document.getElementById("csrfToken").value = data.token;
        }
    } catch (e) {
        console.warn("Не удалось получить CSRF токен", e);
    }
}

loadCsrfToken();

document.getElementById("loginForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("password").value.trim();
    const csrfToken = document.getElementById("csrfToken").value;
    const message = document.getElementById("message");
    const box = document.getElementById("loginBox");

    if (!email || !password) {
        message.innerHTML = sanitize("❌ Заполните все поля!");
        message.className = "error-msg show";
        box.classList.add("shake");
        setTimeout(() => box.classList.remove("shake"), 400);
        return;
    }

    // 📤 Отправляем CSRF токен в заголовке
    const response = await fetch("/postuser", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-CSRF-TOKEN": csrfToken
        },
        credentials: "include",
        body: JSON.stringify({ email, password })
    });

    if (response.ok) {
        message.innerHTML = sanitize("✅ Успешный вход! Перенаправление...");
        message.className = "success-msg show";
        setTimeout(() => (window.location.href = "/"), 1000);
    } else {
        const errorText = response.status === 401
            ? "❌ Неверный логин или пароль."
            : "⚠️ Ошибка сервера. Попробуйте позже.";
        message.innerHTML = sanitize(errorText);
        message.className = "error-msg show";
        box.classList.add("shake");
        setTimeout(() => box.classList.remove("shake"), 400);
    }
});