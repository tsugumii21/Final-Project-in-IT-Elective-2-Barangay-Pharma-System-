/**
 * Barangay Pharma System — Main Application JavaScript
 * Handles: sidebar toggle, toast auto-dismiss, form loading spinner, UX enhancements, dynamic confirmations, scroll handlers.
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

    // ── Navbar Scroll Drop Shadow ──────────────────────────────────────────
    const navbar = document.querySelector('.bps-navbar');
    if (navbar) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 10) {
                navbar.classList.add('bps-navbar-scrolled');
            } else {
                navbar.classList.remove('bps-navbar-scrolled');
            }
        });
    }

    // ── Toast Auto-Dismiss (3 Seconds) ──────────────────────────────────────
    const AUTO_DISMISS_DELAY_MS = 3000;

    function autoDismissAlerts() {
        const alerts = document.querySelectorAll('.bps-alert.alert-dismissible');
        alerts.forEach(function (alertEl) {
            setTimeout(function () {
                // Apply a smooth fade out transition
                alertEl.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
                alertEl.style.opacity = '0';
                alertEl.style.transform = 'translateX(50px)';
                setTimeout(function () {
                    const bsAlert = bootstrap.Alert.getOrCreateInstance(alertEl);
                    if (bsAlert) bsAlert.close();
                }, 500);
            }, AUTO_DISMISS_DELAY_MS);
        });
    }

    // ── Form Loading Spinner ────────────────────────────────────────────────
    function initFormSpinner() {
        // Create the global spinner overlay once if it doesn't exist
        if (document.getElementById('bps-form-spinner')) return;

        const spinner = document.createElement('div');
        spinner.className = 'bps-form-spinner';
        spinner.id = 'bps-form-spinner';
        spinner.innerHTML = `
            <div class="bps-spinner-ring"></div>
            <span class="bps-spinner-text">Processing, please wait…</span>
        `;
        document.body.appendChild(spinner);

        // Hook all POST forms
        document.querySelectorAll('form[method="post"], form[method="POST"]').forEach(function (form) {
            // Skip logout and dispensing forms — they should be instant
            if (form.id === 'logout-form' || form.id === 'dispenseForm') return;

            form.addEventListener('submit', function () {
                spinner.classList.add('is-active');

                // Safety: hide after 15 seconds in case something goes wrong
                setTimeout(function () {
                    spinner.classList.remove('is-active');
                }, 15000);
            });
        });
    }

    // ── Dynamic Confirmations ──────────────────────────────────────────────
    function initConfirmations() {
        document.querySelectorAll('[data-confirm]').forEach(function (el) {
            // Remove previous event listeners if any (by replacing node, though not strictly needed here)
            el.addEventListener('click', function (e) {
                const message = el.getAttribute('data-confirm') || 'Are you sure?';
                if (!confirm(message)) {
                    e.preventDefault();
                    e.stopPropagation();
                }
            });
        });
    }

    // ── Vanilla JS File Input Handlers ─────────────────────────────────────
    function initFileInputs() {
        // Handle native browser file inputs
        document.querySelectorAll('input[type="file"]').forEach(function (input) {
            input.addEventListener('change', function () {
                const label = document.querySelector('label[for="' + input.id + '"]') || input.nextElementSibling;
                if (label && input.files && input.files.length > 0) {
                    label.textContent = input.files[0].name;
                }
            });
        });
    }

    // ── Vanilla Bootstrap Tooltip/Popver Auto-Init ──────────────────────────
    function initTooltips() {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    }

    // ── Stacking Context Modal Helper (Fix for CSS Transform z-index issues) ──
    function initStackingModals() {
        document.querySelectorAll('.modal').forEach(function (modal) {
            if (modal && modal.parentNode !== document.body) {
                document.body.appendChild(modal);
            }
        });
    }

    // ── Password Visibility Toggles ─────────────────────────────────────────
    function initPasswordToggles() {
        document.body.addEventListener('click', function (e) {
            const btn = e.target.closest('[data-password-toggle]');
            if (!btn) return;

            e.preventDefault();
            const targetId = btn.getAttribute('data-password-toggle');
            const input = document.getElementById(targetId);
            if (!input) return;

            const icon = btn.querySelector('i');
            if (input.type === 'password') {
                input.type = 'text';
                if (icon) {
                    icon.className = 'bi bi-eye';
                }
            } else {
                input.type = 'password';
                if (icon) {
                    icon.className = 'bi bi-eye-slash';
                }
            }
        });
    }

    // ── DOM Ready ───────────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        autoDismissAlerts();
        initFormSpinner();
        initConfirmations();
        initFileInputs();
        initTooltips();
        initStackingModals();
        initPasswordToggles();
    });

})();
