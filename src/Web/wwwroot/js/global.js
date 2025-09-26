document.addEventListener('DOMContentLoaded', function () {
    // Navbar scroll efekti
    const navbar = document.getElementById('navbar');
    window.addEventListener('scroll', function () {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    });

    // Hero image hover effect
    const heroImage = document.querySelector('.hero-image img');

    // İstatistik sayma efekti
    const statNumbers = document.querySelectorAll('.stat-number');
    const statsSection = document.querySelector('.hero');

    const options = {
        root: null,
        rootMargin: '0px',
        threshold: 0.5
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                statNumbers.forEach(stat => {
                    const target = +stat.getAttribute('data-target');
                    const count = +stat.innerText.replace(/[^0-9]/g, '');
                    if (!stat.getAttribute('data-target')) {
                        stat.setAttribute('data-target', stat.innerText.replace(/[^0-9]/g, ''));
                        return;
                    }

                    let startTime = null;
                    const duration = 2000;

                    function animateCounter(timestamp) {
                        if (!startTime) startTime = timestamp;
                        const progress = Math.min((timestamp - startTime) / duration, 1);

                        const value = Math.floor(progress * target);
                        stat.innerText = stat.innerText.replace(/[0-9,]+/, value.toLocaleString());

                        if (progress < 1) {
                            requestAnimationFrame(animateCounter);
                        }
                    }

                    requestAnimationFrame(animateCounter);
                });

                observer.unobserve(entry.target);
            }
        });
    }, options);

    observer.observe(statsSection);

    // Floating elements mouse move effect
    const floatingElements = document.querySelectorAll('.floating-element');
    document.addEventListener('mousemove', (e) => {
        const x = e.clientX / window.innerWidth;
        const y = e.clientY / window.innerHeight;

        floatingElements.forEach((element, index) => {
            const speed = 10 + (index * 5);
            const xMove = x * speed - (speed / 2);
            const yMove = y * speed - (speed / 2);

            element.style.transform = `translate(${xMove}px, ${yMove}px) rotate(${xMove}deg)`;
        });
    });

    // Mağaza açma formu
    const applicationForm = document.querySelector('.application-form');
    if (applicationForm) {
        applicationForm.addEventListener('submit', function (e) {
            e.preventDefault();
            alert('Mağaza başvurunuz alınmıştır. En kısa sürede sizinle iletişime geçilecektir.');
        });
    }

    // Scroll animasyonları
    const animatedElements = document.querySelectorAll('.section-title, .section-subtitle, .feature-card, .store-card, .step, .application-form, .testimonial-card, .cta-title, .cta-subtitle, .cta-buttons');

    const animationObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
                animationObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    animatedElements.forEach(element => {
        animationObserver.observe(element);
    });

    // CTA buton etkileşimi
    const ctaButtons = document.querySelectorAll('.cta-buttons .btn');
    ctaButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            const buttonText = this.textContent;
            if (buttonText.includes('Ücretsiz Deneyin')) {
                alert('14 günlük ücretsiz deneme sürümüne yönlendiriliyorsunuz...');
            } else {
                alert('Daha fazla bilgi için iletişim sayfasına yönlendiriliyorsunuz...');
            }
        });
    });
});