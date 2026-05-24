/**
 * Barangay Pharma System — Main Application JavaScript
 * Handles: sidebar toggle, toast auto-dismiss, form loading spinner, UX enhancements.
 */

(function () {
    'use strict';

    // ── Sidebar Toggle (mobile) ─────────────────────────────────────────────
    const sidebar   = document.getElementById('bps-sidebar');
    const toggleBtn = document.getElementById('bps-sidebar-toggle');

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

    // ── Form Loading Spinner ────────────────────────────────────────────────
    function initFormSpinner() {
        // Create the global spinner overlay once
        const spinner = document.createElement('div');
        spinner.className = 'bps-form-spinner';
        spinner.id = 'bps-form-spinner';
        spinner.innerHTML = `
            <div class="bps-spinner-ring"></div>
            <span class="bps-spinner-text">Processing, please wait…</span>
        `;
        document.body.appendChild(spinner);

        // Hook all forms that have data-loading attribute, or POST forms
        document.querySelectorAll('form[method="post"], form[method="POST"]').forEach(function (form) {
            // Skip logout forms — they should be instant
            if (form.id === 'logout-form') return;

            form.addEventListener('submit', function () {
                // Show spinner
                spinner.classList.add('is-active');

                // Safety: hide after 15 seconds in case something goes wrong
                setTimeout(function () {
                    spinner.classList.remove('is-active');
                }, 15000);
            });
        });
    }

    // ── Delete Confirmation ─────────────────────────────────────────────────
    function initDeleteConfirmation() {
        document.querySelectorAll('[data-confirm]').forEach(function (el) {
            el.addEventListener('click', function (e) {
                const message = el.getAttribute('data-confirm') || 'Are you sure you want to delete this record? This action cannot be undone.';
                if (!confirm(message)) {
                    e.preventDefault();
                    e.stopPropagation();
                }
            });
        });
    }

    // ── File Input Label Update ─────────────────────────────────────────────
    function initFileInputs() {
        document.querySelectorAll('input[type="file"]').forEach(function (input) {
            input.addEventListener('change', function () {
                const label = document.querySelector('label[for="' + input.id + '"]');
                if (label && input.files && input.files.length > 0) {
                    label.textContent = input.files[0].name;
                }
            });
        });
    }

    // ── DOM Ready ───────────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        autoDismissAlerts();
        initFormSpinner();
        initDeleteConfirmation();
        initFileInputs();
    });

})();
