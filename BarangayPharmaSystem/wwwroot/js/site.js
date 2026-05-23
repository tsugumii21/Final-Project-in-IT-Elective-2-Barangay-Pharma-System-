/**
 * Barangay Pharma System — Main Application JavaScript
 * Handles: sidebar toggle, toast auto-dismiss, general UX enhancements.
 */

(function () {
    'use strict';

    // ── Sidebar Toggle (mobile) ─────────────────────────────────────────────
    const sidebar        = document.getElementById('bps-sidebar');
    const toggleBtn      = document.getElementById('bps-sidebar-toggle');

    if (sidebar && toggleBtn) {
        // Create overlay element dynamically
        const overlay = document.createElement('div');
        overlay.className = 'bps-sidebar-overlay';
        overlay.id = 'bps-sidebar-overlay';
        document.body.appendChild(overlay);

        function openSidebar() {
            sidebar.classList.add('is-open');
            overlay.classList.add('is-visible');
            toggleBtn.setAttribute('aria-expanded', 'true');
            document.body.style.overflow = 'hidden';
        }

        function closeSidebar() {
            sidebar.classList.remove('is-open');
            overlay.classList.remove('is-visible');
            toggleBtn.setAttribute('aria-expanded', 'false');
            document.body.style.overflow = '';
        }

        toggleBtn.addEventListener('click', function () {
            const isOpen = sidebar.classList.contains('is-open');
            isOpen ? closeSidebar() : openSidebar();
        });

        overlay.addEventListener('click', closeSidebar);

        // Close on Escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && sidebar.classList.contains('is-open')) {
                closeSidebar();
            }
        });
    }

    // ── Toast Auto-Dismiss ──────────────────────────────────────────────────
    const AUTO_DISMISS_DELAY_MS = 5000;

    function autoDismissAlerts() {
        const alerts = document.querySelectorAll('.bps-alert.alert-dismissible');
        alerts.forEach(function (alertEl) {
            setTimeout(function () {
                const bsAlert = bootstrap.Alert.getOrCreateInstance(alertEl);
                if (bsAlert) bsAlert.close();
            }, AUTO_DISMISS_DELAY_MS);
        });
    }

    // ── DOM Ready ───────────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        autoDismissAlerts();
    });

})();
