// Subtle mouse-follow glow on hero title
(function titleGlow() {
    const title = document.querySelector('.hero-title');
    if (!title) return;

    document.addEventListener('mousemove', (e) => {
        const rect = title.getBoundingClientRect();
        const x = ((e.clientX - rect.left) / rect.width) * 100;
        const y = ((e.clientY - rect.top) / rect.height) * 100;

        title.style.setProperty('--mx', x + '%');
        title.style.setProperty('--my', y + '%');
    });
})();

// Fade in on scroll
(function scrollReveal() {
    const elements = document.querySelectorAll('.feature-card, .sysreq-item, .quick-install-inner');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, {
        threshold: 0.1,
        rootMargin: '0px 0px -40px 0px'
    });

    elements.forEach((el) => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(el);
    });
})();

// Animated number counter
function animateNumber(el, target) {
    const duration = 1500;
    const start = 0;
    const startTime = performance.now();

    function tick(now) {
        const elapsed = now - startTime;
        const progress = Math.min(elapsed / duration, 1);
        const eased = 1 - Math.pow(1 - progress, 3);
        const current = Math.floor(start + (target - start) * eased);

        if (target >= 1000) {
            el.textContent = current.toLocaleString();
        } else {
            el.textContent = current;
        }

        if (progress < 1) {
            requestAnimationFrame(tick);
        }
    }

    requestAnimationFrame(tick);
}

// Fetch GitHub stats
(function fetchStats() {
    const downloadsEl = document.getElementById('stat-downloads');
    const versionEl = document.getElementById('stat-version');
    const starsEl = document.getElementById('stat-stars');

    const repo = 'lekdravpm-del/NexusStrap';

    fetch('https://api.github.com/repos/' + repo)
        .then(function(r) { return r.json(); })
        .then(function(data) {
            if (starsEl && data.stargazers_count !== undefined) {
                animateNumber(starsEl, data.stargazers_count);
            }
        })
        .catch(function() {
            if (starsEl) starsEl.textContent = '--';
        });

    fetch('https://api.github.com/repos/' + repo + '/releases/latest')
        .then(function(r) { return r.json(); })
        .then(function(data) {
            if (versionEl && data.tag_name) {
                versionEl.textContent = data.tag_name;
            }
            if (downloadsEl && data.assets) {
                var total = 0;
                for (var i = 0; i < data.assets.length; i++) {
                    total += data.assets[i].download_count || 0;
                }
                animateNumber(downloadsEl, total);
            }
        })
        .catch(function() {
            if (versionEl) versionEl.textContent = '--';
            if (downloadsEl) downloadsEl.textContent = '--';
    });
})();

// Lightbox
(function lightbox() {
    var img = document.getElementById('screenshot-img');
    var box = document.getElementById('lightbox');
    if (!img || !box) return;

    img.style.cursor = 'pointer';

    img.addEventListener('click', function() {
        box.classList.add('active');
        document.body.style.overflow = 'hidden';
    });

    box.addEventListener('click', function(e) {
        if (e.target === box || e.target.tagName === 'IMG') {
            box.classList.remove('active');
            document.body.style.overflow = '';
        }
    });

    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && box.classList.contains('active')) {
            box.classList.remove('active');
            document.body.style.overflow = '';
        }
    });
})();

// Copy install command
(function copyInstall() {
    var btn = document.getElementById('copy-install');
    var cmd = document.getElementById('install-cmd');
    if (!btn || !cmd) return;

    btn.addEventListener('click', function() {
        navigator.clipboard.writeText(cmd.textContent).then(function() {
            btn.classList.add('copied');
            btn.innerHTML = '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>';
            setTimeout(function() {
                btn.classList.remove('copied');
                btn.innerHTML = '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="14" height="14" x="8" y="8" rx="2" ry="2"/><path d="M4 16c-1.1 0-2-.9-2-2V4c0-1.1.9-2 2-2h10c1.1 0 2 .9 2 2"/></svg>';
            }, 2000);
        });
    });
})();
