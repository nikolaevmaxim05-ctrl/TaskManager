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
async function searchUsers() {
    const query = document.getElementById("searchInput").value.trim();
    const userList = document.getElementById("userList");
    const noResults = document.getElementById("noResults");

    userList.innerHTML = "";
    noResults.style.display = "none";

    if (!query) {
        noResults.textContent = "Введите никнейм для поиска";
        noResults.style.display = "block";
        return;
    }

    try {
        const res = await fetch(`/api/user/search?query=${encodeURIComponent(query)}`);
        if (!res.ok) throw new Error("Ошибка запроса");

        const users = await res.json();

        if (!users || users.length === 0) {
            noResults.textContent = "Ничего не найдено";
            noResults.style.display = "block";
            return;
        }

        for (const user of users) {
            const item = document.createElement("div");
            item.className = "user-item";
            item.onclick = () => window.location.href = `/profile/${user.id}`;

            const avatar = document.createElement("img");
            avatar.className = "user-avatar";
            avatar.src = user.avatarPath
                ? user.avatarPath.replaceAll("\\", "/")
                : "/images/ava/default-avatar.jpg";

            const name = document.createElement("div");
            name.className = "user-name";
            name.textContent = user.nickName || "Без имени";

            item.appendChild(avatar);
            item.appendChild(name);
            userList.appendChild(item);
        }
    } catch (err) {
        console.error(err);
        noResults.textContent = "Ошибка при поиске пользователей";
        noResults.style.display = "block";
    }
}


function logout() {
    window.location.href = "/logout";
}