// ============================================
// Sidebar JavaScript - sidebar.js
// ============================================

(function () {
    'use strict';

    // ============================================
    // Toggle Sidebar Collapse/Expand
    // ============================================
    window.toggleSidebar = function () {
        const sidebar = document.getElementById('sidebar');
        const mainContent = document.getElementById('mainContent');

        if (!sidebar || !mainContent) return;

        sidebar.classList.toggle('collapsed');
        mainContent.classList.toggle('expanded');

        // Close all dropdowns when collapsing
        if (sidebar.classList.contains('collapsed')) {
            const openDropdowns = sidebar.querySelectorAll('.nav-dropdown.open');
            openDropdowns.forEach(dropdown => {
                dropdown.classList.remove('open');
            });
        }

        // Save state to localStorage
        localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
    };

    // ============================================
    // Toggle Dropdown Menus
    // ============================================
    window.toggleDropdown = function (event, element) {
        event.preventDefault();
        event.stopPropagation();

        const sidebar = document.getElementById('sidebar');

        // Don't toggle if sidebar is collapsed
        if (sidebar && sidebar.classList.contains('collapsed')) {
            return;
        }

        const dropdown = element.closest('.nav-dropdown');
        if (!dropdown) return;

        const isOpen = dropdown.classList.contains('open');

        // Close all other dropdowns at the same level
        const parentList = dropdown.parentElement;
        const siblingDropdowns = parentList.querySelectorAll('.nav-dropdown');
        siblingDropdowns.forEach(d => {
            if (d !== dropdown) {
                d.classList.remove('open');
            }
        });

        // Toggle current dropdown
        dropdown.classList.toggle('open');
    };

    // ============================================
    // Toggle Mobile Sidebar
    // ============================================
    window.toggleMobileSidebar = function () {
        const sidebar = document.getElementById('sidebar');
        const overlay = document.querySelector('.sidebar-overlay');

        if (!sidebar || !overlay) return;

        sidebar.classList.toggle('mobile-open');
        overlay.classList.toggle('active');

        // Prevent body scroll when sidebar is open on mobile
        if (sidebar.classList.contains('mobile-open')) {
            document.body.style.overflow = 'hidden';
        } else {
            document.body.style.overflow = '';
        }
    };

    // ============================================
    // Close Mobile Sidebar on Link Click
    // ============================================
    function closeMobileSidebarOnNavigation() {
        const sidebar = document.getElementById('sidebar');
        const overlay = document.querySelector('.sidebar-overlay');
        const navLinks = document.querySelectorAll('.sidebar .nav-link:not(.nav-dropdown-toggle)');

        if (!sidebar || !overlay) return;

        navLinks.forEach(link => {
            link.addEventListener('click', function () {
                if (window.innerWidth <= 768) {
                    sidebar.classList.remove('mobile-open');
                    overlay.classList.remove('active');
                    document.body.style.overflow = '';
                }
            });
        });
    }

    // ============================================
    // Highlight Active Page
    // ============================================
    function highlightActivePage() {
        const currentPath = window.location.pathname.toLowerCase();
        const navLinks = document.querySelectorAll('.sidebar .nav-link');

        navLinks.forEach(link => {
            const href = link.getAttribute('href');
            if (href && href.toLowerCase() === currentPath) {
                link.classList.add('active');

                // Open parent dropdown if this is a sub-link
                const parentDropdown = link.closest('.nav-dropdown');
                if (parentDropdown) {
                    parentDropdown.classList.add('open');
                }
            }
        });
    }

    // ============================================
    // Load Saved Sidebar State
    // ============================================
    function loadSidebarState() {
        const sidebarCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        const sidebar = document.getElementById('sidebar');
        const mainContent = document.getElementById('mainContent');

        if (sidebarCollapsed && sidebar && mainContent) {
            sidebar.classList.add('collapsed');
            mainContent.classList.add('expanded');
        }
    }

    // ============================================
    // Handle Window Resize
    // ============================================
    function handleResize() {
        const sidebar = document.getElementById('sidebar');
        const overlay = document.querySelector('.sidebar-overlay');

        if (window.innerWidth > 768) {
            // Desktop: remove mobile classes
            if (sidebar) sidebar.classList.remove('mobile-open');
            if (overlay) overlay.classList.remove('active');
            document.body.style.overflow = '';
        }
    }

    // ============================================
    // Close Dropdown When Clicking Outside
    // ============================================
    function handleClickOutside(event) {
        const sidebar = document.getElementById('sidebar');
        if (!sidebar || sidebar.classList.contains('collapsed')) return;

        const dropdowns = document.querySelectorAll('.nav-dropdown');

        dropdowns.forEach(dropdown => {
            if (!dropdown.contains(event.target)) {
                dropdown.classList.remove('open');
            }
        });
    }

    // ============================================
    // Keyboard Navigation
    // ============================================
    function setupKeyboardNavigation() {
        document.addEventListener('keydown', function (event) {
            // ESC to close mobile sidebar
            if (event.key === 'Escape') {
                const sidebar = document.getElementById('sidebar');
                const overlay = document.querySelector('.sidebar-overlay');

                if (sidebar && sidebar.classList.contains('mobile-open')) {
                    sidebar.classList.remove('mobile-open');
                    if (overlay) overlay.classList.remove('active');
                    document.body.style.overflow = '';
                }
            }
        });
    }

    // ============================================
    // Initialize Tooltips for Collapsed State
    // ============================================
    function initializeTooltips() {
        const sidebar = document.getElementById('sidebar');
        if (!sidebar) return;

        // Add title attributes if not present
        const navLinks = sidebar.querySelectorAll('.nav-link');
        navLinks.forEach(link => {
            if (!link.hasAttribute('title')) {
                const textElement = link.querySelector('.nav-text');
                if (textElement) {
                    link.setAttribute('title', textElement.textContent.trim());
                }
            }
        });
    }

    // ============================================
    // Smooth Scroll for Anchor Links
    // ============================================
    function setupSmoothScroll() {
        const links = document.querySelectorAll('.sidebar a[href^="#"]');

        links.forEach(link => {
            link.addEventListener('click', function (e) {
                const href = this.getAttribute('href');
                if (href === '#') return;

                e.preventDefault();
                const target = document.querySelector(href);

                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    // ============================================
    // Initialize Everything on DOM Ready
    // ============================================
    function init() {
        // Wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', init);
            return;
        }

        loadSidebarState();
        highlightActivePage();
        closeMobileSidebarOnNavigation();
        initializeTooltips();
        setupKeyboardNavigation();
        setupSmoothScroll();

        // Event listeners
        window.addEventListener('resize', handleResize);
        document.addEventListener('click', handleClickOutside);
    }

    // Start initialization
    init();

})();