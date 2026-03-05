let currentUserId = null;

async function initProfileButton() {
    const profileIcon = document.getElementById("profileIcon");

    try {
        const res = await fetch("/api/user/me");
        if (!res.ok) throw new Error("Не удалось получить id пользователя");
        const userId = await res.json();

        // Получаем данные пользователя (аватар)
        const userRes = await fetch(`/api/user/profile/${userId}`);
        if (!userRes.ok) throw new Error("Ошибка загрузки профиля");
        const user = await userRes.json();

        const avatar = user.avatarPath || "/images/ava/default-avatar.jpg";

        // Заменяем иконку 👤 на реальную аватарку
        profileIcon.style.backgroundColor = "transparent";
        profileIcon.style.backgroundImage = `url('${avatar}')`;
        profileIcon.style.backgroundSize = "cover";
        profileIcon.style.backgroundPosition = "center";
        profileIcon.style.border = "2px solid #fff";
        profileIcon.textContent = ""; // убираем символ 👤
        profileIcon.onclick = () => window.location.href = `/profile/${userId}`;
    } catch (err) {
        console.error(err);
        profileIcon.onclick = () => alert("Ошибка: не удалось загрузить аватар пользователя");
    }
}


initProfileButton();
// 🔔 Уведомления
const bell = document.getElementById("bell");
const popup = document.getElementById("notificationPopup");

bell.addEventListener("click", () => {
    popup.classList.toggle("active");
    bell.classList.remove("unread"); // при открытии — все прочитаны
});

document.addEventListener("click", (e) => {
    if (!popup.contains(e.target) && !bell.contains(e.target)) {
        popup.classList.remove("active");
    }
});
``
connectSignalR()
async function connectSignalR() {

    try {
        const res = await fetch("/api/user/me", { credentials: "include" });
        if (!res.ok) return null;
        // Ожидаем что сервис возвращает простой GUID в теле; если возвращает объект — адаптируйте
        currentUserId = await res.json();
    } catch (e) {
        console.warn("Не удалось получить текущего пользователя:", e);
        return null;
    }
    connection = new signalR.HubConnectionBuilder()
        .withUrl(`/notificationHub?userId=${currentUserId}`, { withCredentials: true })
        .configureLogging(signalR.LogLevel.Information)
        .withAutomaticReconnect()
        .build();

    connection.onreconnecting(err => {
        console.warn("🔄 Reconnecting...", err);
    });

    connection.onreconnected(id => {
        console.log("🔌 Reconnected", id);
    });

    connection.onclose(err => {
        console.error("❌ Connection closed:", err);
    });
    
    connection.on("ReceiveNotification", async (notification) => {await renderNotification(notification)});

    start();
}

function start() {
    try {
        connection.start();
        console.log("✅ Connected to SignalR");
    } catch (err) {
        console.error("❌ Error connecting:", err);
        setTimeout(start, 3000);
    }
}




async function loadNotifications() {
    try {
        const res = await fetch("/api/notifications");
        if (!res.ok) throw new Error("Ошибка при загрузке уведомлений");
        const notifications = await res.json();
        await renderNotifications(notifications);
    } catch (err) {
        console.error(err);
        popup.innerHTML = `<div class="notification-empty">Не удалось загрузить уведомления</div>`;
    }
}
async function renderNotification(n) {
    // Если popup пустой и там надпись "Нет новых уведомлений" — очищаем
    if (popup.innerHTML.includes("notification-empty")) {
        popup.innerHTML = "";
    }

    // Загружаем данные отправителя
    const senderRes = await fetch(`/api/user/profile/${n.senderId}`);
    const sender = await senderRes.json();

    const avatar = sender.avatarPath || "/images/ava/default-avatar.jpg";
    const nickname = sender.nickName || sender.eMail;

    // Основной контейнер
    const item = document.createElement("div");
    item.className = "notification-item";
    item.style.position = "relative";

    // --- Контент уведомления ---
    const content = document.createElement("div");
    content.style.display = "flex";
    content.style.alignItems = "center";
    content.style.gap = "10px";

    content.innerHTML = `
        <img src="${avatar}" width="40" height="40" style="border-radius:50%;object-fit:cover;">
        <div style="flex-grow:1;">${n.body}</div>
    `;

    item.appendChild(content);

    // --- Типы уведомлений ---
    if (n.notificationType === 0) {
        // Friend request: добавить кнопки
        const btnBlock = document.createElement("div");
        btnBlock.style.marginTop = "8px";
        btnBlock.style.display = "flex";
        btnBlock.style.gap = "8px";
        btnBlock.style.justifyContent = "flex-end";

        btnBlock.innerHTML = `
            <button style="background:#2ecc71;color:white;border:none;padding:6px 10px;border-radius:6px;cursor:pointer;"
                    onclick="acceptRequest('${n.id}')">Принять</button>
            <button style="background:#e74c3c;color:white;border:none;padding:6px 10px;border-radius:6px;cursor:pointer;"
                    onclick="dismissRequest('${n.id}')">Отклонить</button>
        `;

        item.appendChild(btnBlock);

    } else {
        // Остальные уведомления — добавить крестик удаления
        const closeBtn = document.createElement("div");
        closeBtn.textContent = "✖";
        closeBtn.style.position = "absolute";
        closeBtn.style.top = "6px";
        closeBtn.style.right = "10px";
        closeBtn.style.cursor = "pointer";
        closeBtn.style.fontSize = "14px";
        closeBtn.style.color = "#999";
        closeBtn.style.transition = "0.2s";
        closeBtn.title = "Удалить уведомление";

        closeBtn.onmouseover = () => closeBtn.style.color = "#333";
        closeBtn.onmouseout  = () => closeBtn.style.color = "#999";

        closeBtn.onclick = async (e) => {
            e.stopPropagation();
            await deleteNotification(n.id);
        };

        item.appendChild(closeBtn);
    }

    // Помечаем колокольчик
    if (n.status === 1) {
        bell.classList.add("unread");
    }

    // Добавляем НОВОЕ уведомление в начало списка
    popup.prepend(item);
}

async function renderNotifications(notifications) {
    popup.innerHTML = "";

    if (!notifications || notifications.length === 0) {
        popup.innerHTML = `<div class="notification-empty">Нет новых уведомлений</div>`;
        return;
    }

    let hasUnread = false;

    for (const n of notifications) {
        const item = document.createElement("div");
        item.className = "notification-item";
        item.style.position = "relative";

        // Загружаем данные отправителя
        const senderRes = await fetch(`/api/user/profile/${n.senderId}`);
        const sender = await senderRes.json();

        const avatar = sender.avatarPath || "/images/ava/default-avatar.jpg";
        const nickname = sender.nickName || sender.eMail;

        // --- контент уведомления ---
        const content = document.createElement("div");
        content.style.display = "flex";
        content.style.alignItems = "center";
        content.style.gap = "10px";

        content.innerHTML = `
            <img src="${avatar}" width="40" height="40" style="border-radius:50%;object-fit:cover;">
            <div style="flex-grow:1;">${n.body}</div>
        `;

        item.appendChild(content);

        // --- если это запрос в друзья ---
        if (n.notificationType === 0) {
            const btnBlock = document.createElement("div");
            btnBlock.style.marginTop = "8px";
            btnBlock.style.display = "flex";
            btnBlock.style.gap = "8px";
            btnBlock.style.justifyContent = "flex-end";
            btnBlock.innerHTML = `
                <button style="background:#2ecc71;color:white;border:none;padding:6px 10px;border-radius:6px;cursor:pointer;"
                        onclick="acceptRequest('${n.id}')">Принять</button>
                <button style="background:#e74c3c;color:white;border:none;padding:6px 10px;border-radius:6px;cursor:pointer;"
                        onclick="dismissRequest('${n.id}')">Отклонить</button>
            `;
            item.appendChild(btnBlock);
        } else {
            // --- крестик удаления ---
            const closeBtn = document.createElement("div");
            closeBtn.textContent = "✖";
            closeBtn.style.position = "absolute";
            closeBtn.style.top = "6px";
            closeBtn.style.right = "10px";
            closeBtn.style.cursor = "pointer";
            closeBtn.style.fontSize = "14px";
            closeBtn.style.color = "#999";
            closeBtn.style.transition = "0.2s";
            closeBtn.title = "Удалить уведомление";
            closeBtn.onmouseover = () => closeBtn.style.color = "#333";
            closeBtn.onmouseout = () => closeBtn.style.color = "#999";
            closeBtn.onclick = async (e) => {
                e.stopPropagation();
                await deleteNotification(n.id);
            };
            item.appendChild(closeBtn);
        }

        if (n.status === 1) hasUnread = true;
        popup.appendChild(item);
    }

    bell.classList.toggle("unread", hasUnread);
}


async function acceptRequest(id) {
    await sendRequest(`/api/notifications/acceptFriendRequest/${id}`, "POST");
    await loadNotifications();
}

async function dismissRequest(id) {
    await sendRequest(`/api/notifications/dismissFriendRequest/${id}`, "POST");
    await loadNotifications();
}

async function deleteNotification(id) {
    try {
        const res = await fetch(`/api/notifications/${id}`, { method: "DELETE" });
        if (!res.ok) throw new Error("Ошибка при удалении уведомления");
        await loadNotifications();
    } catch (err) {
        console.error(err);
        alert("Не удалось удалить уведомление");
    }
}

// --- Остальной код профиля без изменений ---

function getProfileIdFromUrl() {
    const parts = window.location.pathname.split("/");
    return parts.length > 2 ? parts[2] : null;
}

async function loadProfile() {
    const content = document.getElementById("profileContent");
    const profileId = getProfileIdFromUrl();

    if (!profileId) {
        content.innerHTML = `<div class="loading">❌ Некорректный адрес профиля</div>`;
        return;
    }

    try {
        const [userRes, statsRes, statusRes] = await Promise.all([
            fetch(`/api/user/profile/${profileId}`),
            fetch(`/api/tasks/stats/${profileId}`),
            fetch(`/api/user/status/${profileId}`)
        ]);

        if (!userRes.ok || !statsRes.ok || !statusRes.ok)
            throw new Error("Ошибка загрузки данных");

        const user = await userRes.json();
        const stats = await statsRes.json();
        const status = await statusRes.json();

        renderProfile(user, stats, status);
    } catch (err) {
        console.error(err);
        content.innerHTML = `<div class="loading">❌ Не удалось загрузить профиль</div>`;
    }
}

function renderProfile(user, stats, status) {
    const content = document.getElementById("profileContent");
    const displayName = user.nickName?.trim() || user.eMail;

    content.innerHTML = `
            ${status === 0 ? `<button class="edit-icon" onclick="window.location.href='/ProfileEditPage.html'">⚙️</button>` : ""}
            <img src="${user.avatarPath || '/images/ava/default-avatar.jpg'}" alt="Аватар" class="avatar" />
            <div class="nickname">${displayName}</div>
            ${user.nickName ? `<div class="user-email">${user.eMail}</div>` : ""}

            <div class="stats">
                <div class="stat-box"><div class="stat-title">Всего задач</div><div class="stat-value">${stats.total ?? 0}</div></div>
                <div class="stat-box"><div class="stat-title">В процессе</div><div class="stat-value">${stats.inProgress ?? 0}</div></div>
                <div class="stat-box"><div class="stat-title">Выполнено</div><div class="stat-value">${stats.completed ?? 0}</div></div>
                <div class="stat-box"><div class="stat-title">Просрочено</div><div class="stat-value">${stats.overdue ?? 0}</div></div>
            </div>

            <div class="action-buttons">
                ${renderButtons(status)}
            </div>
        `;
}

function renderButtons(status) {
    const profileId = window.location.pathname.split("/").pop();
    if (status === 0) {
        return `
                <button onclick="window.location.href='/api/friendList'">Посмотреть друзей</button>
                <button onclick="window.location.href='/api/add-friend'">Добавить друга</button>
            `;
    }

    switch (status) {
        case 1: return `<button onclick="sendFriendRequest('${profileId}')">Добавить в друзья</button>`;
        case 2: return `<button onclick="removeFriend('${profileId}')">Удалить из друзей</button>`;
        case 4: return `<button onclick="cancelRequest('${profileId}')">Отменить заявку</button>`;
        case 3: return `<button onclick="unblockUser('${profileId}')">Разблокировать</button>`;
        default: return "";
    }
}

async function sendRequest(url, method = "POST") {
    try {
        const res = await fetch(url, { method, headers: { "Content-Type": "application/json" } });
        if (!res.ok) throw new Error(await res.text());
        alert("✅ Операция успешно выполнена");
    } catch (err) {
        alert("❌ Ошибка: " + err.message);
    }
}

async function sendFriendRequest(profileId) {
    await sendRequest(`/api/friends/request/${profileId}`);
}

async function removeFriend(profileId) {
    await sendRequest(`/api/friends/remove/${profileId}`, "DELETE");
}

async function cancelRequest(profileId) {
    await sendRequest(`/api/friends/cancel/${profileId}`);
}

async function unblockUser(profileId) {
    await sendRequest(`/api/friends/unblock/${profileId}`);
}

function logout() {
    window.location.href = "/logout";
}

loadProfile();
loadNotifications();