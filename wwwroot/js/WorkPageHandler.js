async function loadNotes() {
    const res = await fetch("/api/tasks");
    if (!res.ok) {
        document.getElementById("noteList").innerHTML = "<p>Ошибка загрузки задач.</p>";
        return;
    }

    const notes = await res.json();
    const container = document.getElementById("noteList");
    container.innerHTML = "";

    if (notes.length === 0) {
        container.innerHTML = "<p>Задач пока нет.</p>";
        return;
    }

    const now = new Date();

    notes.forEach(note => {
        const card = document.createElement("div");
        card.classList.add("note-card");

        const status = document.createElement("div");
        status.classList.add("status-indicator");

        // 🔹 Определяем цвет по полю status
        if (note.status === 1) {
            status.classList.add("status-completed");
        } else if (note.status === 2) {
            status.classList.add("status-overdue");
        } else if (note.status === 0) {
            status.classList.add("status-inprogress");
        }

        const content = document.createElement("div");
        content.classList.add("note-content");
        content.onclick = () => window.location.href = `/tasks/${note.id}`;

        const title = document.createElement("div");
        title.classList.add("note-title");
        title.textContent = note.headOfNote;

        const body = document.createElement("div");
        body.classList.add("note-body");
        body.textContent = note.bodyOfNote;

        const deadline = document.createElement("div");
        deadline.classList.add("note-deadline");
        deadline.textContent = "Дедлайн: " + new Date(note.dateOfDeadLine).toLocaleString("ru-RU");

        content.appendChild(title);
        content.appendChild(body);
        content.appendChild(deadline);

        // Добавляем индикатор и контент
        card.appendChild(status);
        card.appendChild(content);
        container.appendChild(card);
    });
}

function logout() {
    window.location.href = "/logout";
}

loadNotes();

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
    const profileIcon = document.getElementById("profileIcon");
    alert("123");
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