async function getCurrentUserId() {
    try {
        const res = await fetch("/api/user/me"); // или твой актуальный эндпоинт
        if (!res.ok) return null;
        const id = await res.json();
        return id;
    } catch {
        return null;
    }
}

async function initProfileButton() {
    const userId = await getCurrentUserId();
    const profileIcon = document.getElementById("profileIcon");
    if (userId) {
        profileIcon.onclick = () => window.location.href = `/profile/${userId}`;
    } else {
        profileIcon.onclick = () => alert("Ошибка: не удалось получить профиль пользователя");
    }
}

initProfileButton();
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
const csrfToken = document.getElementById("csrfToken").value;



// Переключение вкладок
function showTab(tabId) {
    document.querySelectorAll(".tab").forEach(tab => tab.style.display = "none");
    document.querySelectorAll(".sidebar button").forEach(btn => btn.classList.remove("active"));
    document.getElementById(tabId).style.display = "block";
    event.target.classList.add("active");
}

// Предпросмотр аватара
function previewAvatar() {
    const input = document.getElementById("avatarInput");
    const preview = document.getElementById("avatarPreview");
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = e => preview.src = e.target.result;
        reader.readAsDataURL(input.files[0]);
    }
}

// Загрузка данных пользователя
async function loadUser() {
    try {
        const res = await fetch("/api/user/profile");
        if (!res.ok) throw new Error("Ошибка загрузки профиля");
        const user = await res.json();
        document.getElementById("nickname").value = user.nickName || "";
        document.getElementById("email").value = user.eMail || "";
        document.getElementById("avatarPreview").src = user.avatarPath || "/photo/ava/default-avatar.jpg";
    } catch {
        console.error("Не удалось загрузить данные пользователя");
    }
}

// Сохранение настроек профиля
async function saveProfile() {
    const nickname = document.getElementById("nickname").value.trim();
    const avatarPath = document.getElementById("avatarInput").files[0];
    const status = document.getElementById("profileStatus");
    const csrfToken = document.getElementById("csrfToken").value;
    const formData = new FormData();
    formData.append("nickName", nickname);
    formData.append("avatar", avatarPath);

    try {
        const res = await fetch("/api/user/stats", {
            headers: {
                "X-CSRF-TOKEN": csrfToken
            },
            method: "PUT",
            body: formData
        });
        status.textContent = res.ok ? "✅ Профиль обновлён" : "❌ Ошибка при обновлении";
    } catch {
        status.textContent = "❌ Ошибка подключения к серверу";
    }
}

// Сохранение настроек аккаунта
async function saveAccount() {
    const email = document.getElementById("email").value.trim();
    const oldPass = document.getElementById("oldPassword").value;
    const newPass = document.getElementById("newPassword").value;
    const status = document.getElementById("accountStatus");

    if (!oldPass || !newPass) {
        status.textContent = "⚠️ Введите старый и новый пароль";
        return;
    }

    try {
        const res = await fetch("/api/user/", {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
                "X-CSRF-TOKEN": csrfToken
            },
            body: JSON.stringify({ email, OldPassword: oldPass, NewPassword: newPass })
        });
        status.textContent = res.ok ? "✅ Данные аккаунта обновлены" : "❌ Ошибка обновления";
    } catch {
        status.textContent = "❌ Ошибка подключения к серверу";
    }
}

loadUser();