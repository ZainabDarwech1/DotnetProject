// ============================================
// SignalR Client Connections
// ============================================

var notificationConnection = null;

// ============================================
// Initialize all connections
// ============================================
document.addEventListener('DOMContentLoaded', function () {
    initializeSignalR();
});

function initializeSignalR() {
    // Only connect if user is authenticated
    var isAuthenticated = document.body.getAttribute('data-authenticated') === 'true';

    if (!isAuthenticated) {
        console.log('User not authenticated, skipping SignalR');
        return;
    }

    initNotificationHub();
}

// Helper to build options with transport fallback
function hubUrlOptions() {
    if (typeof signalR === 'undefined' || !signalR.HttpTransportType) return undefined;
    return { transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling };
}

// ============================================
// Notification Hub
// ============================================
function initNotificationHub() {
    var options = hubUrlOptions();
    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notification", options)
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // connection lifecycle
    notificationConnection.onreconnecting(function (err) {
        console.warn('Notification Hub reconnecting', err);
    });
    notificationConnection.onreconnected(function (id) {
        console.log('Notification Hub reconnected. connectionId=', id);
        // re-initialize badge after reconnect
        updateBadgeCount();
    });
    notificationConnection.onclose(function (err) {
        console.error('Notification Hub connection closed', err);
    });

    // Receive notification
    notificationConnection.on("ReceiveNotification", function (notification) {
        console.log("ReceiveNotification payload:", notification);

        try {
            showToast(notification.message || "New notification", notification.type || "info");

            if (notification.unreadCount !== undefined) {
                setNotificationBadge(notification.unreadCount);
            } else {
                updateBadgeCount();
            }

            addNotificationToDropdown(notification);

            injectNotificationIntoPage(notification);

        } catch (e) {
            console.error('Error handling ReceiveNotification payload', e);
        }
    });

    function injectNotificationIntoPage(notification) {
        // Only run if notifications page is open
        const list = document.querySelector('.notifications-list');
        if (!list) return;

        const emptyState = document.querySelector('.empty-state');
        if (emptyState) {
            emptyState.remove();
        }

        const card = document.createElement('div');
        card.className = 'notification-card unread';
        card.setAttribute('data-notification-id', notification.notificationId);

        card.innerHTML = `
        <div class="notification-icon-wrapper">
            <div class="notification-icon icon-${(notification.type || 'default').toLowerCase()}">
                <i class="bi bi-bell-fill"></i>
            </div>
        </div>

        <div class="notification-content">
            <div class="notification-header">
                <h5 class="notification-title">
                    ${escapeHtml(notification.title || 'Notification')}
                    <span class="new-badge">New</span>
                </h5>
                <div class="notification-meta">
                    <span class="notification-date">
                        <i class="bi bi-clock"></i> Just now
                    </span>
                </div>
            </div>
            <p class="notification-message">
                ${escapeHtml(notification.message || '')}
            </p>
        </div>

        <div class="notification-actions">
            <button class="btn-action btn-read"
                onclick="markAsRead(${notification.notificationId})"
                title="Mark as Read">
                <i class="bi bi-check2"></i>
            </button>
            <button class="btn-action btn-delete"
                onclick="deleteNotification(${notification.notificationId})"
                title="Delete">
                <i class="bi bi-trash"></i>
            </button>
        </div>
    `;

        // Animate in
        card.style.opacity = '0';
        card.style.transform = 'translateY(-10px)';
        list.prepend(card);

        requestAnimationFrame(() => {
            card.style.transition = 'all 0.3s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        });
    }


    // Notification marked as read
    notificationConnection.on("NotificationMarkedAsRead", function (notificationId) {
        console.log("NotificationMarkedAsRead:", notificationId);
        removeNotificationFromDropdown(notificationId);
        updateBadgeCount();
    });

    // Unread count updated
    notificationConnection.on("UnreadCountUpdated", function (count) {
        console.log('UnreadCountUpdated event:', count);
        setNotificationBadge(count);
    });

    // Start connection
    notificationConnection.start()
        .then(function () {
            console.log("Notification Hub connected");
            // fetch current unread count to initialize badge
            updateBadgeCount();
        })
        .catch(function (err) {
            console.error("Notification Hub error:", err);
        });
}

// ============================================
// Utility Functions
// ============================================

function showToast(message, type) {
    if (typeof Swal !== 'undefined') {
        const iconMap = {
            'info': 'info',
            'success': 'success',
            'warning': 'warning',
            'error': 'error',
            1: 'info',        // Booking
            2: 'warning',     // Emergency
            3: 'info',        // System
            4: 'info',        // Admin
            5: 'success'      // Review
        };

        Swal.fire({
            icon: iconMap[type] || 'info',
            title: message,
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 5000,
            timerProgressBar: true
        });
    }
}

function setNotificationBadge(count) {
    const badge = document.getElementById('notification-badge');
    if (badge) {
        badge.textContent = count > 99 ? '99+' : count;
        if (count > 0) {
            badge.style.display = 'inline';
            badge.classList.add('notification-badge');
        } else {
            badge.style.display = 'none';
        }
    }
}

function updateBadgeCount() {
    fetch('/Notification/GetUnreadCount')
        .then(function (response) { return response.json(); })
        .then(function (data) { setNotificationBadge(data.count); })
        .catch(function (err) { console.error('Failed to get unread count:', err); });
}

function addNotificationToDropdown(notification) {
    const notificationList = document.getElementById('notification-list');
    if (!notificationList) return;

    // Remove "no notifications" message if present
    const noNotifications = notificationList.querySelector('.text-center.py-3');
    if (noNotifications) {
        noNotifications.remove();
    }

    const icon = getNotificationIcon(notification.type);
    const notificationHtml = `
        <a href="#" class="dropdown-item d-flex align-items-start py-2 bg-light" 
           onclick="handleNotificationClick(${notification.notificationId}, ${notification.referenceId}, '${notification.typeName || 'System'}')">
            <span class="me-2">${icon}</span>
            <div class="flex-grow-1">
                <div class="fw-semibold small">${escapeHtml(notification.title || 'Notification')}</div>
                <div class="text-muted small text-truncate" style="max-width: 250px;">${escapeHtml(notification.message || '')}</div>
                <div class="text-muted small">Just now</div>
            </div>
        </a>
    `;

    notificationList.insertAdjacentHTML('afterbegin', notificationHtml);

    // Keep only last 10 notifications in dropdown
    const items = notificationList.querySelectorAll('.dropdown-item');
    if (items.length > 10) {
        items[items.length - 1].remove();
    }
}

function removeNotificationFromDropdown(notificationId) {
    const notificationList = document.getElementById('notification-list');
    if (!notificationList) return;

    const items = notificationList.querySelectorAll('.dropdown-item');
    items.forEach(item => {
        const onclick = item.getAttribute('onclick');
        if (onclick && onclick.includes(`(${notificationId},`)) {
            item.remove();
        }
    });

    // Show "no notifications" if empty
    if (notificationList.children.length === 0) {
        notificationList.innerHTML = '<div class="text-center py-3 text-muted"><i class="bi bi-bell-slash"></i> No notifications</div>';
    }
}

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

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}