// Notification System JavaScript

let notificationConnection = null;

// Initialize notification system
function initNotifications() {
    // Load initial notifications
    loadNotifications();

    // Setup SignalR connection
    setupSignalRConnection();

    // Setup dropdown event
    const dropdown = document.getElementById('notificationDropdown');
    if (dropdown) {
        dropdown.addEventListener('show.bs.dropdown', function () {
            loadNotifications();
        });
    }
}

// Load notifications via AJAX
function loadNotifications() {
    fetch('/Notification/GetLatest')
        .then(response => response.json())
        .then(notifications => {
            renderNotificationList(notifications);
        })
        .catch(error => {
            console.error('Error loading notifications:', error);
        });
}

// Render notification list in dropdown
function renderNotificationList(notifications) {
    const container = document.getElementById('notification-list');
    if (!container) return;

    if (notifications.length === 0) {
        container.innerHTML = '<div class="text-center py-3 text-muted"><i class="bi bi-bell-slash"></i> No notifications</div>';
        return;
    }

    let html = '';
    notifications.forEach(n => {
        const icon = getNotificationIcon(n.type);
        const unreadClass = n.isRead ? '' : 'bg-light';

        html += `
            <a href="#" class="dropdown-item d-flex align-items-start py-2 ${unreadClass}" 
               onclick="handleNotificationClick(${n.notificationId}, ${n.referenceId}, '${n.typeName}')">
                <span class="me-2">${icon}</span>
                <div class="flex-grow-1">
                    <div class="fw-semibold small">${escapeHtml(n.title)}</div>
                    <div class="text-muted small text-truncate" style="max-width: 250px;">${escapeHtml(n.message)}</div>
                    <div class="text-muted small">${n.timeAgo}</div>
                </div>
            </a>
        `;
    });

    container.innerHTML = html;
}

// Get icon based on notification type
function getNotificationIcon(type) {
    switch (type) {
        case 1: // Booking
            return '<i class="bi bi-calendar-check text-primary"></i>';
        case 2: // Emergency
            return '<i class="bi bi-exclamation-triangle text-danger"></i>';
        case 3: // System
            return '<i class="bi bi-gear text-secondary"></i>';
        case 4: // Admin
            return '<i class="bi bi-shield-check text-info"></i>';
        case 5: // Review
            return '<i class="bi bi-star text-warning"></i>';
        default:
            return '<i class="bi bi-bell text-secondary"></i>';
    }
}

// Handle notification click
function handleNotificationClick(notificationId, referenceId, typeName) {
    // Mark as read
    fetch('/Notification/MarkAsRead?id=' + notificationId, { method: 'POST' });
    updateBadgeCount();

    // Navigate based on type
    switch (typeName) {
        case 'Booking':
            if (referenceId) window.location.href = '/Booking/Details/' + referenceId;
            break;
        case 'Emergency':
            if (referenceId) window.location.href = '/Emergency/Details/' + referenceId;
            break;
        case 'Review':
            window.location.href = '/Profile';
            break;
        default:
            window.location.href = '/Notification';
    }
}

// Update badge count
function updateBadgeCount() {
    fetch('/Notification/GetUnreadCount')
        .then(response => response.json())
        .then(data => {
            const badge = document.getElementById('notification-badge');
            if (badge) {
                badge.textContent = data.count > 99 ? '99+' : data.count;
                badge.style.display = data.count > 0 ? 'inline' : 'none';
            }
        });
}

// Setup SignalR connection
function setupSignalRConnection() {
    if (typeof signalR === 'undefined') {
        console.warn('SignalR not loaded');
        return;
    }

    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/notification')
        .withAutomaticReconnect()
        .build();

    notificationConnection.on('ReceiveNotification', function (notification) {
        // Show toast
        showNotificationToast(notification);

        // Update badge
        updateBadgeCount();

        // Reload dropdown if open
        loadNotifications();
    });

    notificationConnection.start()
        .then(() => console.log('Notification hub connected'))
        .catch(err => console.error('Notification hub error:', err));
}

// Show toast notification
function showNotificationToast(notification) {
    const toastContainer = document.getElementById('toast-container') || createToastContainer();

    const toastId = 'toast-' + Date.now();
    const icon = getNotificationIcon(notification.type);

    const toastHtml = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header">
                ${icon}
                <strong class="me-auto ms-2">${escapeHtml(notification.title)}</strong>
                <small>Just now</small>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${escapeHtml(notification.message)}
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);

    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, { autohide: true, delay: 5000 });
    toast.show();

    // Play sound (optional)
    playNotificationSound();
}

// Create toast container if not exists
function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '1100';
    document.body.appendChild(container);
    return container;
}

// Play notification sound
function playNotificationSound() {
    try {
        const audio = new Audio('/sounds/notification.mp3');
        audio.volume = 0.5;
        audio.play().catch(() => { }); // Ignore if blocked by browser
    } catch (e) {
        // Ignore errors
    }
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    initNotifications();
});