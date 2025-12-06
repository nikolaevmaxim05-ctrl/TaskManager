// /js/Chat.js

const chatId = window.location.pathname.split("/").pop();
let currentUserId = null;
let csrfToken = null;
let connection = null;


// ---------------- ИНИЦИАЛИЗАЦИЯ ----------------
async function init() {
    await loadCsrfToken();
    await initProfileButton();
    currentUserId = (await getCurrentUserId())?.toString() ?? null;
    await loadChat();
    connectSignalR();
}

async function getCurrentUserId() {
    try {
        const res = await fetch("/api/user/me", { credentials: "include" });
        if (!res.ok) return null;
        // Ожидаем что сервис возвращает простой GUID в теле; если возвращает объект — адаптируйте
        const j = await res.json();
        return j;
    } catch (e) {
        console.warn("Не удалось получить текущего пользователя:", e);
        return null;
    }
}

async function loadCsrfToken() {
    try {
        const res = await fetch("/csrf-token", { credentials: "include" });
        if (res.ok) {
            const data = await res.json();
            csrfToken = data.token;
            const hidden = document.getElementById("csrfToken");
            if (hidden) hidden.value = csrfToken;
        }
    } catch (e) {
        console.warn("Не удалось получить CSRF токен", e);
    }
}

// ---------------- ПОДКЛЮЧЕНИЕ К WEBSOCKET ----------------

function connectSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl(`/chatHub?chatId=${chatId}`) // можно передавать параметры
        .withAutomaticReconnect()       // авто-переподключение
        .build();
    
    start();
    
    connection.on("ReceiveMessage", (message) => {renderMessage(message)});
    connection.on("UpdateMessage", (message) => {updateLocalMessage(message)});
    connection.on("DeleteMessage", (message) => {removeLocalMessage(message.id)});
}
function start() {

    try {
        connection.start();
    }
    catch (error) {
        console.error(error);
        setTimeout(start, 3000);
    }
}

// ---------------- ПРОФИЛЬ ----------------
async function initProfileButton() {
    const profileIcon = document.getElementById("profileIcon");

    try {
        const res = await fetch("/api/user/me", { credentials: "include" });
        if (!res.ok) throw new Error("Не удалось получить id пользователя");
        const userId = await res.json();

        const userRes = await fetch(`/api/user/profile/${userId}`, { credentials: "include" });
        if (!userRes.ok) throw new Error("Не удалось получить профиль пользователя");
        const user = await userRes.json();
        const avatar = user.avatarPath || "/photo/ava/default-avatar.jpg";

        profileIcon.style.backgroundImage = `url('${avatar}')`;
        profileIcon.textContent = "";
        profileIcon.onclick = () => window.location.href = `/profile/${userId}`;
    } catch (e) {
        console.warn("initProfileButton failed:", e);
        profileIcon.onclick = () => alert("Ошибка: не удалось загрузить профиль");
    }
}

// ---------------- ЗАГРУЗКА ЧАТА ----------------
async function loadChat() {
    try {
        const res = await fetch(`/api/chat/${chatId}`, { credentials: "include" });
        if (!res.ok) {
            alert("Ошибка загрузки чата");
            return;
        }
        const chat = await res.json();
        const sortedMessages = (chat.messages || []).sort((a, b) => new Date(a.sendTime) - new Date(b.sendTime));
        renderMessages(sortedMessages);
    } catch (e) {
        console.error("Ошибка при загрузке чата:", e);
        alert("Ошибка загрузки чата");
    }
}
function renderMessages(messages) {
    const box = document.getElementById("chatMessages");
    box.innerHTML = "";
    messages.forEach(m => renderMessage(m));
}

function renderMessage(msg) {
    const box = document.getElementById("chatMessages");

    if (document.getElementById("msg-" + msg.id)) return; // не добавлять если есть

    const div = document.createElement("div");
    div.className = "message " + (msg.sender === currentUserId ? "sent" : "received");
    div.id = "msg-" + msg.id;

    div.innerHTML = `
        <div>${msg.body}</div>
        <small>${new Date(msg.sendTime).toLocaleTimeString()}</small>

        ${msg.sender === currentUserId ? `
            <div class="message-actions">
                <span onclick="editMessage('${msg.id}', '${msg.body.replace(/'/g, "\\'")}')">✏</span>
                <span onclick="deleteMessage('${msg.id}')">🗑</span>
            </div>
        ` : ""}
    `;

    box.appendChild(div);
    box.scrollTop = box.scrollHeight;
}


let editingMessageId = null; // глобальная переменная для редактируемого сообщения

// ---------------- РЕДАКТИРОВАНИЕ ----------------
function editMessage(msgId, currentText) {
    const input = document.getElementById("messageInput");
    input.value = currentText;          // помещаем текст в поле ввода
    input.focus();                       // фокусируем
    editingMessageId = msgId;            // запоминаем id редактируемого сообщения
}

// ---------------- ОТПРАВКА (fetch) ----------------
document.getElementById("messageForm").addEventListener("submit", async e => {
    e.preventDefault();
    const input = document.getElementById("messageInput");
    const text = input.value.trim();
    if (!text) return;

    if (editingMessageId) {
        // --- Редактирование сообщения ---
        try {
            const formData = new FormData();
            formData.append("Id", editingMessageId);
            formData.append("Body", text);

            const res = await fetch(`/api/chat/${chatId}/redactMessage`, {
                method: "PUT",
                body: formData,
                credentials: "include",
                headers: { "X-CSRF-TOKEN": csrfToken }
            });

            if (!res.ok) {
                const errText = await res.text().catch(() => "");
                alert("Ошибка при редактировании: " + errText);
                return;
            }

            // локально не обновляем — ждём событие SignalR UpdateMessage
            editingMessageId = null;   // очищаем id
            input.value = "";           // очищаем поле
        } catch (e) {
            console.error("Ошибка при редактировании:", e);
            alert("Ошибка сети при редактировании");
        }

    } else {
        // --- Отправка нового сообщения ---
        const formData = new FormData();
        formData.append("Body", text);
        formData.append("Sender", currentUserId);

        try {
            const res = await fetch(`/api/chat/${chatId}/sendMessage`, {
                method: "POST",
                body: formData,
                credentials: "include",
                headers: { "X-CSRF-TOKEN": csrfToken }
            });

            if (!res.ok) {
                const textErr = await res.text().catch(()=>null);
                alert("Ошибка отправки сообщения" + (textErr ? (": " + textErr) : ""));
                return;
            }

            input.value = "";
        } catch (e) {
            console.error("Ошибка запроса:", e);
        }
    }
});

function updateLocalMessage(msg) {
    const div = document.getElementById("msg-" + msg.id);
    if (!div) return;

    div.querySelector("div").textContent = msg.body;
}


// ---------------- УДАЛЕНИЕ ----------------
async function deleteMessage(id) {

    const formData = new FormData();
    formData.append("messageId", id);
    const res = await fetch(`/api/chat/${chatId}/deleteMessage`, {
        method: "DELETE",
        headers: { "X-CSRF-TOKEN": csrfToken },
        credentials: "include",
        body: formData
    });

    if (!res.ok) alert("Ошибка при удалении");
}

function removeLocalMessage(id) {
    const elem = document.getElementById("msg-" + id);
    if (elem) elem.remove();
}



// ---------------- ЛОГАУТ-------------------
function logout() {
    window.location.href = "/logout";
}

init();
