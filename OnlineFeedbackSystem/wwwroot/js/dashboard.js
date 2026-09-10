// ==========================================================
// Pulse — Dashboard interactions (Mobile Sidebar Drawer & Notifications)
// ==========================================================

document.addEventListener('DOMContentLoaded', function () {
    // ---------- Mobile Sidebar Drawer ----------
    var navToggle = document.getElementById('dashNavToggle');
    var sidebar = document.querySelector('.dash-sidebar');
    var overlay = document.getElementById('dashSidebarOverlay');

    function openSidebar() {
        if (sidebar) sidebar.classList.add('dash-sidebar-open');
        if (overlay) overlay.classList.add('dash-overlay-visible');
        if (navToggle) navToggle.setAttribute('aria-expanded', 'true');
        document.body.classList.add('dash-noscroll');
    }

    function closeSidebar() {
        if (sidebar) sidebar.classList.remove('dash-sidebar-open');
        if (overlay) overlay.classList.remove('dash-overlay-visible');
        if (navToggle) navToggle.setAttribute('aria-expanded', 'false');
        document.body.classList.remove('dash-noscroll');
    }

    if (navToggle) {
        navToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            if (sidebar && sidebar.classList.contains('dash-sidebar-open')) {
                closeSidebar();
            } else {
                openSidebar();
            }
        });
    }

    if (overlay) {
        overlay.addEventListener('click', closeSidebar);
    }

    // Close when clicking a nav link on mobile
    if (sidebar) {
        sidebar.querySelectorAll('.dash-nav-link').forEach(function (link) {
            link.addEventListener('click', function () {
                if (window.innerWidth <= 992) {
                    closeSidebar();
                }
            });
        });
    }

    // ---------- Live Unread Notifications Poller ----------
    function updateUnreadBadge() {
        fetch('/Notifications/GetUnreadCount')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                var badges = document.querySelectorAll('.notif-badge, .notif-badge-sidebar');
                badges.forEach(function (badge) {
                    if (data && data.count > 0) {
                        badge.textContent = data.count > 99 ? '99+' : data.count;
                        badge.style.display = 'inline-flex';
                    } else {
                        badge.style.display = 'none';
                    }
                });
            })
            .catch(function () { });
    }

    setInterval(updateUnreadBadge, 25000);
});
