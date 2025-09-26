document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('loginForm');
    const errorAlert = document.getElementById('errorAlert');
    const successAlert = document.getElementById('successAlert');
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');

    // Şifre görünürlüğünü değiştirme
    togglePassword.addEventListener('click', function () {
        if (passwordInput.type === 'password') {
            passwordInput.type = 'text';
            togglePassword.innerHTML = '<i class="fas fa-eye-slash"></i>';
        } else {
            passwordInput.type = 'password';
            togglePassword.innerHTML = '<i class="fas fa-eye"></i>';
        }
    });

    // Demo kullanıcı bilgileri
    const demoUsers = [
        { email: 'demo@satici.com', password: 'demo123' },
        { email: 'ahmet@modaevim.com', password: 'ahmet123' },
        { email: 'test@pazaryeri.com', password: 'test123' }
    ];

    // Form gönderimi
    loginForm.addEventListener('submit', function (e) {
        e.preventDefault();

        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;
        const rememberMe = document.getElementById('rememberMe').checked;

        // Basit doğrulama
        if (!email || !password) {
            showError('Lütfen tüm alanları doldurun.');
            return;
        }

        // E-posta formatı kontrolü
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            showError('Lütfen geçerli bir e-posta adresi girin.');
            return;
        }

        // Demo giriş kontrolü
        const validUser = demoUsers.find(user =>
            user.email === email && user.password === password
        );

        if (validUser) {
            // Başarılı giriş
            showSuccess();

            // Remember me seçeneği
            if (rememberMe) {
                localStorage.setItem('rememberedEmail', email);
            } else {
                localStorage.removeItem('rememberedEmail');
            }

            // Yönlendirme
            setTimeout(function () {
                window.location.href = 'dashboard.html';
            }, 1500);
        } else {
            // Hatalı giriş
            showError('E-posta adresi veya şifre hatalı.');
        }
    });

    // Hata mesajını göster
    function showError(message) {
        errorAlert.style.display = 'block';
        document.getElementById('errorMessage').textContent = message;
        successAlert.style.display = 'none';

        // 5 saniye sonra hata mesajını gizle
        setTimeout(function () {
            errorAlert.style.display = 'none';
        }, 5000);
    }

    // Başarı mesajını göster
    function showSuccess() {
        successAlert.style.display = 'block';
        errorAlert.style.display = 'none';
    }

    // Remember me özelliği
    const rememberedEmail = localStorage.getItem('rememberedEmail');
    if (rememberedEmail) {
        document.getElementById('email').value = rememberedEmail;
        document.getElementById('rememberMe').checked = true;
    }

    // Enter tuşu ile gönderme
    document.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            loginForm.dispatchEvent(new Event('submit'));
        }
    });
});