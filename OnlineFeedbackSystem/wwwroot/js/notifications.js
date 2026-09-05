(function () {
    // simple unread count poller that updates elements with data-unread-badge-id="notifications"
    async function fetchUnread() {
        try {
            const res = await fetch('/Notifications/GetUnreadCount');
            if (!res.ok) return;
            const data = await res.json();
            const badges = document.querySelectorAll('[data-unread-badge-id="notifications"]');
            badges.forEach(b => {
                const count = data.count || 0;
                b.textContent = count > 0 ? count : '';
                b.style.display = count > 0 ? 'inline-block' : 'none';
            });
        } catch (e) {
            // ignore
        }
    }

    // initial fetch and interval
    fetchUnread();
    setInterval(fetchUnread, 30000); // every 30s
})();