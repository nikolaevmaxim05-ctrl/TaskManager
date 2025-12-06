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

const uploadInput = document.getElementById("imageUpload");
const imageSection = document.getElementById("imageSection");
let selectedImages = [];

uploadInput.addEventListener("change", function () {
    for (const file of this.files) {
        selectedImages.push(file);

        const preview = document.createElement("div");
        preview.classList.add("image-preview");

        const img = document.createElement("img");
        img.src = URL.createObjectURL(file);

        const removeBtn = document.createElement("button");
        removeBtn.textContent = "✖";
        removeBtn.classList.add("remove-image");
        removeBtn.addEventListener("click", () => {
            preview.remove();
            selectedImages = selectedImages.filter(f => f !== file);
        });

        preview.appendChild(img);
        preview.appendChild(removeBtn);
        imageSection.insertBefore(preview, imageSection.firstChild);
    }

    this.value = "";
});

document.getElementById("noteForm").addEventListener("submit", async function (event) {
    event.preventDefault();

    const date = document.getElementById("deadline-date").value;
    const time = document.getElementById("deadline-time").value;
    const HeadOfNote = document.getElementById("HeadOfNote").value.trim();
    const BodyOfNote = document.getElementById("BodyOfNote").value.trim();
    const csrfToken = document.getElementById("csrfToken").value;

    if (!date || !time) {
        alert("Пожалуйста, выберите дату и время дедлайна");
        return;
    }

    const DateOfDeadLine = `${date}T${time}`;
    const formData = new FormData();
    formData.append("HeadOfNote", HeadOfNote);
    formData.append("BodyOfNote", BodyOfNote);
    formData.append("DateOfDeadLine", DateOfDeadLine);
    if (selectedImages) {
        selectedImages.forEach((p, i) => formData.append(`Images[${i}]`, p));
    }

    try {
        const response = await fetch("/api/tasks/create", {
            method: "POST",
            headers: csrfToken ? { "X-CSRF-TOKEN": csrfToken } : {},
            body: formData,
            credentials: "include"
        });

        if (response.ok) {
            alert("✅ Задача успешно создана!");
            window.location.href = "/work";
        } else {
            const errorText = await response.text();
            alert("⚠️ Ошибка: " + errorText);
        }
    } catch (error) {
        console.error("Ошибка при отправке запроса:", error);
        alert("❌ Не удалось отправить задачу. Проверьте соединение.");
    }
});