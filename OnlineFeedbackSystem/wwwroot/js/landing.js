// ==========================================================
// Pulse — Online Feedback System
// Landing page interactions: mobile nav + scroll reveal
// ==========================================================

document.addEventListener('DOMContentLoaded', function () {

    // ---------- Mobile nav toggle ----------
    var navToggle = document.getElementById('navToggle');
    var navLinks = document.getElementById('navLinks');

    if (navToggle && navLinks) {
        navToggle.addEventListener('click', function () {
            var isOpen = navLinks.classList.toggle('nav-links-open');
            navToggle.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        });

        // Close the menu after a link is tapped
        navLinks.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', function () {
                navLinks.classList.remove('nav-links-open');
                navToggle.setAttribute('aria-expanded', 'false');
            });
        });

        // Close when clicking outside
        document.addEventListener('click', function (e) {
            if (navLinks.classList.contains('nav-links-open') &&
                !navLinks.contains(e.target) &&
                !navToggle.contains(e.target)) {
                navLinks.classList.remove('nav-links-open');
                navToggle.setAttribute('aria-expanded', 'false');
            }
        });
    }

    // ---------- Scroll reveal for clay cards ----------
    var revealTargets = document.querySelectorAll(
        '.clay-card, .clay-chip-static, .lane-card, .role-card, .feature-card'
    );

    if ('IntersectionObserver' in window && revealTargets.length) {
        revealTargets.forEach(function (el) {
            el.style.opacity = '0';
            el.style.transform = 'translateY(18px)';
            el.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
        });

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.15 });

        revealTargets.forEach(function (el) {
            observer.observe(el);
        });
    }

});