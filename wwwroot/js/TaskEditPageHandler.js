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
let currentFiles = [];         // новые файлы (File объекты)
let existingImages = [];       // старые изображения (пути)
let deletedOldImages = [];     // какие старые удалили

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

async function loadTask() {
    const res = await fetch(`/api/tasks/${noteId}`);
    if (res.ok) {
        const note = await res.json();
        document.getElementById("title").value = note.headOfNote;
        document.getElementById("body").value = note.bodyOfNote;

        const deadline = new Date(note.dateOfDeadLine);
        document.getElementById("deadline-date").value = deadline.toISOString().split("T")[0];
        document.getElementById("deadline-time").value = deadline.toTimeString().substring(0, 5);

        if (note.images && note.images.length) {
            existingImages = note.images.map(src =>
                src.startsWith("/") ? src : "/" + src.replace(/^.*wwwroot[\\/]/, "")
            );
        }

        renderAllPhotos();
    }
}

// когда пользователь выбирает новые файлы
document.getElementById("imageUpload").addEventListener("change", e => {
    const files = Array.from(e.target.files);
    currentFiles.push(...files);
    renderAllPhotos();
});

// отрисовываем все фото (старые + новые)
function renderAllPhotos() {
    const preview = document.getElementById("imagePreview");
    preview.innerHTML = "";

    // старые изображения
    existingImages.forEach(path => {
        const div = document.createElement("div");
        div.className = "image-item";
        div.innerHTML = `
                <img src="${path}" alt="old">
                <button class="delete-image" data-old="${path}">🗑</button>
            `;
        preview.appendChild(div);
    });

    // новые файлы
    currentFiles.forEach((file, index) => {
        const url = URL.createObjectURL(file);
        const div = document.createElement("div");
        div.className = "image-item";
        div.innerHTML = `
                <img src="${url}" alt="new">
                <button class="delete-image" data-index="${index}">🗑</button>
            `;
        preview.appendChild(div);
    });
}

// обработчик удаления
document.getElementById("imagePreview").addEventListener("click", e => {
    if (e.target.classList.contains("delete-image")) {
        // если удаляем новое
        if (e.target.dataset.index !== undefined) {
            currentFiles.splice(e.target.dataset.index, 1);
        }
        // если удаляем старое
        else if (e.target.dataset.old) {
            const path = e.target.dataset.old;
            deletedOldImages.push(path);
            existingImages = existingImages.filter(p => p !== path);
        }
        renderAllPhotos();
    }
});

// отправка формы
// отправка формы
document.getElementById("editNoteForm").addEventListener("submit", async e => {
    e.preventDefault();

    const formData = new FormData();
    formData.append("HeadOfNote", document.getElementById("title").value);
    formData.append("BodyOfNote", document.getElementById("body").value);

    const date = document.getElementById("deadline-date").value;
    const time = document.getElementById("deadline-time").value;
    const deadline = new Date(`${date}T${time}:00`);
    formData.append("DateOfDeadLine", deadline.toISOString());

    // 🔹 Сохраняем старые, оставшиеся картинки
    existingImages.forEach((path, i) => formData.append(`ExistingImages[${i}]`, path));

    // 🔹 Добавляем новые файлы
    currentFiles.forEach((file, i) => formData.append(`Images[${i}]`, file));

    // 🔹 Удалённые (если хочешь, можно обработать на сервере)
    deletedOldImages.forEach((path, i) => formData.append(`DeletedImages[${i}]`, path));

    const csrfToken = document.getElementById("csrfToken").value;
    const res = await fetch(`/api/tasks/${noteId}`, {
        method: "PUT",
        headers: { "X-CSRF-TOKEN": csrfToken },
        body: formData,
        credentials: "include"
    });

    const msg = document.getElementById("message");
    if (res.ok) {
        msg.className = "success-msg";
        msg.textContent = "✅ Изменения сохранены!";
        setTimeout(() => window.location.href = `/tasks/${noteId}`, 1000);
    } else {
        msg.className = "error-msg";
        msg.textContent = "❌ Ошибка при сохранении.";
    }
});


async function logout() {
    await fetch("/logout", { credentials: "include" });
    window.location.href = "/";
}

(async () => {
    await loadCsrfToken();
    await loadTask();
})();