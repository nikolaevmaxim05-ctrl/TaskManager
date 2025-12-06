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

    try {
        const res = await fetch("/api/user/me");
        if (!res.ok) throw new Error("Не удалось получить id пользователя");
        const userId = await res.json();

        // Получаем данные пользователя (аватар)
        const userRes = await fetch(`/api/user/profile/${userId}`);
        if (!userRes.ok) throw new Error("Ошибка загрузки профиля");
        const user = await userRes.json();

        const avatar = user.avatarPath || "/photo/ava/default-avatar.jpg";

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
const noteId = window.location.pathname.split("/").pop();
let currentNote = null;

async function loadTask() {
    const res = await fetch(`/api/tasks/${noteId}`);
    if (res.ok) {
        currentNote = await res.json();

        document.getElementById("noteTitle").textContent = currentNote.headOfNote;
        document.getElementById("noteBody").textContent = currentNote.bodyOfNote;
        document.getElementById("noteDeadline").textContent =
            new Date(currentNote.dateOfDeadLine).toLocaleString("ru-RU", {
                day: "2-digit",
                month: "long",
                year: "numeric",
                hour: "2-digit",
                minute: "2-digit",
            });

        updateStatusUI();
        renderImages(currentNote.images);
    } else {
        document.getElementById("noteTitle").textContent = "Задача не найдена";
    }
}

// 🖼️ Отображение изображений
function renderImages(images) {
    const gallery = document.getElementById("imageGallery");
    gallery.innerHTML = "";

    if (!images || images.length === 0) {
        gallery.innerHTML = "<p style='text-align:center;color:#888;'>Нет прикреплённых изображений</p>";
        return;
    }

    images.forEach(src => {
        const img = document.createElement("img");
        img.src = src;
        img.alt = "Изображение задачи";
        img.addEventListener("click", () => openImageModal(src));
        gallery.appendChild(img);
    });
}

// 🔍 Открытие и закрытие модального окна
function openImageModal(src) {
    const modal = document.getElementById("imageModal");
    const modalImg = document.getElementById("modalImage");
    modalImg.src = src;
    modal.style.display = "flex";
}

document.getElementById("imageModal").addEventListener("click", () => {
    document.getElementById("imageModal").style.display = "none";
});

function updateStatusUI() {
    const indicator = document.getElementById("statusIndicator");
    const select = document.getElementById("statusSelect");

    indicator.className = "status-indicator";
    switch (currentNote.status) {
        case 1:
            indicator.classList.add("completed");
            select.value = "completed";
            break;
        case 2:
            indicator.classList.add("overdue");
            select.value = "overdue";
            break;
        default:
            indicator.classList.add("inprogress");
            select.value = "inprogress";
            break;
    }
}

document.getElementById("saveStatusBtn").addEventListener("click", async () => {
    const newStatus = document.getElementById("statusSelect").value;
    let updatedNote = { ...currentNote };

    if (newStatus === "completed") updatedNote.status = 1;
    else if (newStatus === "overdue") updatedNote.status = 2;
    else if (newStatus === "inprogress") updatedNote.status = 0;

    const formData = new FormData();
    formData.append("HeadOfNote", updatedNote.headOfNote);
    formData.append("BodyOfNote", updatedNote.bodyOfNote);
    formData.append("DateOfDeadLine", updatedNote.dateOfDeadLine);
    formData.append("Status", updatedNote.status);

// Добавляем текущие изображения в ExistingImages, чтобы сервер не удалял их
    if (currentNote.images && currentNote.images.length > 0) {
        currentNote.images.forEach(img => formData.append("ExistingImages", img));
    }


    loadCsrfToken();
    const token = document.getElementById("csrfToken").value;
    const res = await fetch(`/api/tasks/${noteId}`, {
        headers: {"X-CSRF-TOKEN": token} ,
        method: "PUT",
        body: formData
    });

    if (res.ok) {
        alert("✅ Статус успешно обновлён");
        try { currentNote = await res.json(); }
        catch { currentNote.status = updatedNote.status; }
        updateStatusUI();
    } else {
        alert("❌ Ошибка при обновлении статуса");
    }
});

document.getElementById("editBtn").onclick = () => {
    window.location.href = `/tasks/edit/${noteId}`;
};

function logout() {
    window.location.href = "/logout.html";
}

loadTask();