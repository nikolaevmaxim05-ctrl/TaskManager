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

const form = document.getElementById("registerForm");
const message = document.getElementById("message");
const modal = document.getElementById("confirmModal");
const confirmMsg = document.getElementById("confirmMessage");

let pendingEmail = "";
let pendingPassword = "";
let pendingNickname = "";

form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const nickname = document.getElementById("nickname").value.trim();
    const Email = document.getElementById("Email").value.trim();
    const password = document.getElementById("password").value.trim();
    const confirmPassword = document.getElementById("confirmPassword").value.trim();
    const csrfToken = document.getElementById("csrfToken").value;

    if (!nickname || !Email || !password || !confirmPassword) {
        message.textContent = "❌ Заполните все поля!";
        message.className = "error-msg show";
        return;
    }
    if (password !== confirmPassword) {
        message.textContent = "❌ Пароли не совпадают!";
        message.className = "error-msg show";
        return;
    }

    // Отправляем запрос на отправку кода на почту
    const res = await fetch("/api/auth/send-confirm-code", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-CSRF-TOKEN": csrfToken
        },
        body: JSON.stringify({ email: Email })
    });
    if (res.ok) {
        pendingEmail = Email;
        pendingPassword = password;
        pendingNickname = nickname;
        modal.classList.add("active");
    } else {
        message.textContent = "⚠️ Не удалось отправить код подтверждения!";
        message.className = "error-msg show";
    }
});

async function confirmEmail() {
    const code = document.getElementById("confirmCode").value.trim();
    const csrfToken = document.getElementById("csrfToken").value;
    if (!code) {
        confirmMsg.textContent = "Введите код!";
        confirmMsg.className = "error-msg show";
        return;
    }

    const res = await fetch("/api/auth/confirm-email", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-CSRF-TOKEN": csrfToken
        },
        body: JSON.stringify({
            email: pendingEmail,
            nickname: pendingNickname,
            password: pendingPassword,
            code: code
        })
    });
    if (res.ok) {
        confirmMsg.textContent = "✅ Почта подтверждена! Аккаунт создан.";
        confirmMsg.className = "success-msg show";
        setTimeout(() => window.location.href = "/", 1500);
    } else {
        confirmMsg.textContent = "❌ Неверный код!";
        confirmMsg.className = "error-msg show";
    }
}