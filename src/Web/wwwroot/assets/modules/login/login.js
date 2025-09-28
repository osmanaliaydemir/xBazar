btnLogin.addEventListener('click', function (e) {
    e.preventDefault();
    e.stopPropagation();
    console.debug('[login] btnLogin click');
    const form = document.getElementById('loginForm');
    const btnLogin = document.getElementById('btnLogin');
    const errorAlert = document.getElementById('errorAlert');
    const successAlert = document.getElementById('successAlert');
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');
    const emailInput = document.getElementById('email');

    const API_BASE_URL = (window.apiBaseUrl || 'https://localhost:7001/api').replace(/\/+$/, '');

    const hideAlerts = () => {
        if (errorAlert) errorAlert.style.display = 'none';
        if (successAlert) successAlert.style.display = 'none';
    };
    const showError = (m) => {
        if (!errorAlert) return;
        errorAlert.style.display = 'block';
        document.getElementById('errorMessage').textContent = m || 'Bir hata oluştu.';
        if (successAlert) successAlert.style.display = 'none';
        setTimeout(() => errorAlert.style.display = 'none', 5000);
    };
    const showSuccess = (m) => {
        if (!successAlert) return;
        successAlert.style.display = 'block';
        successAlert.querySelector('span').textContent = m || 'Giriş başarılı, yönlendiriliyorsunuz...';
        if (errorAlert) errorAlert.style.display = 'none';
    };
    const parseJsonSafe = async (resp) => {
        const ct = resp.headers.get('content-type') || '';
        if (ct.includes('application/json')) { try { return await resp.json(); } catch { return null; } }
        try { return { message: await resp.text() }; } catch { return null; }
    };
    const getReturnUrl = () => {
        const u = new URL(window.location.href);
        const r = u.searchParams.get('returnUrl');
        return r && r.startsWith('/') ? r : '/dashboard';
    };

    // Şifre görünürlüğü
    if (togglePassword && passwordInput) {
        togglePassword.addEventListener('click', function () {
            const isPwd = passwordInput.type === 'password';
            passwordInput.type = isPwd ? 'text' : 'password';
            togglePassword.innerHTML = isPwd ? '<i class="fas fa-eye-slash"></i>' : '<i class="fas fa-eye"></i>';
        });
    }

    // Remembered email
    const rememberedEmail = localStorage.getItem('rememberedEmail');
    if (rememberedEmail && emailInput) {
        emailInput.value = rememberedEmail;
        const rememberMe = document.getElementById('rememberMe');
        if (rememberMe) rememberMe.checked = true;
    }

    // Güvence: form submit olursa bile post’u iptal et
    if (form) {
        form.addEventListener('submit', function (e) {
            console.debug('[login] native submit yakalandı, preventDefault');
            e.preventDefault();
            e.stopPropagation();
            // hiçbir şey yapma; tüm akış btnLogin üzerinden
        });

        // Enter → butona bas
        form.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                btnLogin?.click();
            }
        });
    }

    async function doLogin() {
        hideAlerts();
        const email = (emailInput?.value || '').trim();
        const password = (passwordInput?.value || '');
        const rememberMe = document.getElementById('rememberMe')?.checked === true;

        if (!email || !password) return showError('Lütfen tüm alanları doldurun.');
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) return showError('Lütfen geçerli bir e-posta adresi girin.');
        if (password.length < 6) return showError('Şifre en az 6 karakter olmalı.');

        const originalText = btnLogin?.textContent;
        if (btnLogin) { btnLogin.disabled = true; btnLogin.textContent = 'Giriş yapılıyor...'; }

        try {
            // BACKEND: [HttpPost("login")] → /api/auth/login bekleniyor
            const resp = await fetch(`${API_BASE_URL}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include', // refresh_token cookie için şart
                body: JSON.stringify({ email, password, rememberMe })
            });

            const data = await parseJsonSafe(resp) || {};
            if (resp.ok) {
                const accessToken = data.accessToken || null;
                const expiresAt = data.expiresAt || null;
                const user = data.user || null;

                if (accessToken) {
                    try {
                        sessionStorage.setItem('accessToken', accessToken);
                        if (expiresAt) sessionStorage.setItem('tokenExpiry', expiresAt);
                        if (user) sessionStorage.setItem('user', JSON.stringify(user));
                    } catch { }
                }

                if (rememberMe) localStorage.setItem('rememberedEmail', email);
                else localStorage.removeItem('rememberedEmail');

                showSuccess();
                setTimeout(() => window.location.href = getReturnUrl(), 800);
            } else {
                const msg = data.message || 'E-posta adresi veya şifre hatalı.';
                showError(msg);
            }
        } catch (err) {
            console.error('Login error:', err);
            showError('Giriş sırasında bir hata oluştu. Lütfen tekrar deneyin.');
        } finally {
            if (btnLogin && originalText) {
                btnLogin.disabled = false;
                btnLogin.textContent = originalText;
            }
        }
    }

    // Tek giriş noktası
    if (btnLogin) {
        btnLogin.addEventListener('click', function (e) {
            e.preventDefault(); // ekstra güvence
            e.stopPropagation();
            console.debug('[login] btnLogin click');
            doLogin();
        });
    } else {
        console.warn('[login] btnLogin bulunamadı, script bağlanmadı.');
    }

    console.debug('[login] script yüklendi, baseUrl:', API_BASE_URL);
});


