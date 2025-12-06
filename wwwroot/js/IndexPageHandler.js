// Простая проверка авторизации
async function checkAuth() {
    try {
        const res = await fetch('/api/me', { credentials: 'include' });
        const buttons = document.getElementById('auth-buttons');

        if (res.status === 200) {
            buttons.innerHTML = `
          <button class="btn btn-work" onclick="window.location.href='/work'">К задачам</button>
          <button class="btn btn-logout" onclick="logout()">Выйти</button>
        `;
        } else if (res.status === 401) {
            buttons.innerHTML = `
          <button class="btn btn-login" onclick="window.location.href='/login'">Войти</button>
          <button class="btn btn-register" onclick="window.location.href='/signin'">Регистрация</button>
        `;
        }
    } catch (err) {
        console.error('Ошибка проверки авторизации:', err);
    }
}

function logout() {
    window.location.href = "/logout";
}

checkAuth();