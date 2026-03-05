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
async function loadFriends() {
    const list = document.getElementById("friendsList");
    const noFriends = document.getElementById("noFriends");

    list.innerHTML = "";
    noFriends.style.display = "none";

    try {
        const res = await fetch("/api/friends");
        if (!res.ok) throw new Error("Ошибка при получении друзей");

        const friends = await res.json();

        if (!friends || friends.length === 0) {
            noFriends.style.display = "block";
            return;
        }

        for (const friend of friends) {
            const item = document.createElement("div");
            item.className = "user-item";

            const info = document.createElement("div");
            info.className = "user-info";
            info.onclick = () => window.location.href = `/profile/${friend.id}`;

            const avatar = document.createElement("img");
            avatar.className = "user-avatar";
            avatar.src = friend.avatarPath
                ? friend.avatarPath.replaceAll("\\", "/")
                : "/images/ava/default-avatar.jpg";

            const name = document.createElement("div");
            name.className = "user-name";
            name.textContent = friend.nickName || "Без имени";

            info.appendChild(avatar);
            info.appendChild(name);
            item.appendChild(info);

            // === Новая кнопка "Написать сообщение" ===
            const chatButton = document.createElement("button");
            chatButton.textContent = "Написать сообщение";
            chatButton.className = "message-button";
            chatButton.onclick = (e) => {
                e.stopPropagation();
                if (friend.chat && friend.chat.id) {
                    window.location.href = `/chat/${friend.chat.id}`;
                } else {
                    alert("Чат с этим пользователем ещё не создан.");
                }
            };

            // === Кнопка "Удалить" ===
            const removeButton = document.createElement("button");
            removeButton.className = "remove-button";
            removeButton.textContent = "Удалить";
            removeButton.onclick = async (e) => {
                e.stopPropagation();
                await removeFriend(friend.id);
            };

            // Оборачиваем кнопки в отдельный контейнер
            const buttonsContainer = document.createElement("div");
            buttonsContainer.style.display = "flex";
            buttonsContainer.style.gap = "8px";

            buttonsContainer.appendChild(chatButton);
            buttonsContainer.appendChild(removeButton);

            item.appendChild(buttonsContainer);
            list.appendChild(item);
        }
    } catch (err) {
        console.error(err);
        noFriends.textContent = "Ошибка при загрузке друзей";
        noFriends.style.display = "block";
    }
}


async function removeFriend(friendId) {
    if (!confirm("Вы уверены , что хотите удалить этого друга?")) return;
    try {
        const res = await fetch(`/api/friends/remove/${friendId}`, { method: "DELETE" });
        if (!res.ok) throw new Error("Ошибка при удалении друга");

        await loadFriends(); // перезагрузка списка
    } catch (err) {
        alert("Не удалось удалить друга");
        console.error(err);
    }
}

function logout() {
    window.location.href = "/logout";
}

loadFriends();