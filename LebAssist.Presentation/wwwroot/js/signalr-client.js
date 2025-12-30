// ============================================
// SignalR Client Connections
// ============================================

var notificationConnection = null;
var emergencyConnection = null;
var availabilityConnection = null;
var bookingConnection = null;

// ============================================
// Initialize all connections
// ============================================
document.addEventListener('DOMContentLoaded', function () {
    initializeSignalR();
});

function initializeSignalR() {
    // Only connect if user is authenticated
    var isAuthenticated = document.body.getAttribute('data-authenticated') === 'true' ||
        document.querySelector('[data-is-provider]') !== null;

    if (!isAuthenticated) {
        console.log('User not authenticated, skipping SignalR');
        return;
    }

    initNotificationHub();
    initEmergencyHub();
    initAvailabilityHub();
    initBookingHub();
}

// ============================================
// Notification Hub
// ============================================
function initNotificationHub() {
    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notification")
        .withAutomaticReconnect()
        .build();

    // Receive notification
    notificationConnection.on("ReceiveNotification", function (notification) {
        console.log("Notification received:", notification);
        showToast(notification.message || "New notification", notification.type || "info");
        updateNotificationBadge(1);
        addNotificationToDropdown(notification);
    });

    // Notification marked as read
    notificationConnection.on("NotificationMarkedAsRead", function (notificationId) {
        console.log("Notification marked as read:", notificationId);
        removeNotificationFromDropdown(notificationId);
    });

    // Unread count updated
    notificationConnection.on("UnreadCountUpdated", function (count) {
        setNotificationBadge(count);
    });

    // Start connection
    notificationConnection.start()
        .then(function () {
            console.log("Notification Hub connected");
        })
        .catch(function (err) {
            console.error("Notification Hub error:", err);
        });
}

// ============================================
// Emergency Hub
// ============================================
function initEmergencyHub() {
    var isProvider = document.body.getAttribute('data-is-provider') === 'true';

    emergencyConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/emergency")
        .withAutomaticReconnect()
        .build();

    // Emergency received (for providers)
    emergencyConnection.on("OnEmergencyReceived", function (emergency) {
        console.log("Emergency received:", emergency);
        if (isProvider) {
            showEmergencyAlert(emergency);
            playAlertSound();
        }
    });

    // Emergency removed (accepted by another provider)
    emergencyConnection.on("OnEmergencyRemoved", function (emergencyId) {
        console.log("Emergency removed:", emergencyId);
        removeEmergencyFromList(emergencyId);
    });

    // Emergency accepted notification
    emergencyConnection.on("OnEmergencyAccepted", function (emergencyId, providerName) {
        console.log("Emergency accepted:", emergencyId, "by", providerName);
        showToast("Emergency #" + emergencyId + " accepted by " + providerName, "info");
    });

    // Emergency status changed (for clients)
    emergencyConnection.on("OnEmergencyStatusChanged", function (data) {
        console.log("Emergency status changed:", data);
        showToast("Emergency #" + data.emergencyId + " status: " + data.status, "info");
    });

    // Start connection
    emergencyConnection.start()
        .then(function () {
            console.log("Emergency Hub connected");
            if (isProvider) {
                emergencyConnection.invoke("JoinProviders");
            }
        })
        .catch(function (err) {
            console.error("Emergency Hub error:", err);
        });
}

// ============================================
// Availability Hub
// ============================================
function initAvailabilityHub() {
    availabilityConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/availability")
        .withAutomaticReconnect()
        .build();

    // Provider availability changed
    availabilityConnection.on("OnProviderAvailabilityChanged", function (providerId, isAvailable) {
        console.log("Provider", providerId, "availability:", isAvailable);
        updateProviderStatusIndicator(providerId, isAvailable);
    });

    // Provider location changed
    availabilityConnection.on("OnProviderLocationChanged", function (providerId, latitude, longitude) {
        console.log("Provider", providerId, "location:", latitude, longitude);
        // Update map marker if applicable
    });

    // Start connection
    availabilityConnection.start()
        .then(function () {
            console.log("Availability Hub connected");
        })
        .catch(function (err) {
            console.error("Availability Hub error:", err);
        });
}

// ============================================
// Booking Hub
// ============================================
function initBookingHub() {
    bookingConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/booking")
        .withAutomaticReconnect()
        .build();

    // Booking status changed
    bookingConnection.on("OnBookingStatusChanged", function (data) {
        console.log("Booking status changed:", data);
        showToast("Booking #" + data.bookingId + " status: " + data.status, "info");
        updateBookingStatus(data.bookingId, data.status);
    });

    // New booking request (for providers)
    bookingConnection.on("OnNewBookingRequest", function (booking) {
        console.log("New booking request:", booking);
        showToast("New booking request received!", "warning");
        playNotificationSound();
    });

    // Booking accepted
    bookingConnection.on("OnBookingAccepted", function (data) {
        console.log("Booking accepted:", data);
        showToast(data.message, "success");
    });

    // Booking rejected
    bookingConnection.on("OnBookingRejected", function (data) {
        console.log("Booking rejected:", data);
        showToast(data.message, "danger");
    });

    // Booking started
    bookingConnection.on("OnBookingStarted", function (data) {
        console.log("Booking started:", data);
        showToast(data.message, "info");
    });

    // Booking completed
    bookingConnection.on("OnBookingCompleted", function (data) {
        console.log("Booking completed:", data);
        showToast(data.message + " Please leave a review.", "success");
    });

    // Start connection
    bookingConnection.start()
        .then(function () {
            console.log("Booking Hub connected");
        })
        .catch(function (err) {
            console.error("Booking Hub error:", err);
        });
}

// ============================================
// Helper Functions
// ============================================

// Show toast notification
function showToast(message, type) {
    type = type || 'info';

    var toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        toastContainer.style.zIndex = '1100';
        document.body.appendChild(toastContainer);
    }

    var toastId = 'toast-' + Date.now();
    var bgClass = 'bg-' + type;

    var toastHtml =
        '<div id="' + toastId + '" class="toast ' + bgClass + ' text-white" role="alert">' +
        '<div class="toast-header ' + bgClass + ' text-white">' +
        '<strong class="me-auto">LebAssist</strong>' +
        '<small>Just now</small>' +
        '<button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>' +
        '</div>' +
        '<div class="toast-body">' + message + '</div>' +
        '</div>';

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);

    var toastElement = document.getElementById(toastId);
    var toast = new bootstrap.Toast(toastElement, { delay: 5000 });
    toast.show();

    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });
}

// Update notification badge
function updateNotificationBadge(increment) {
    var badge = document.getElementById('notification-badge');
    if (badge) {
        var count = parseInt(badge.textContent) || 0;
        count += increment;
        badge.textContent = count;
        badge.style.display = count > 0 ? 'inline' : 'none';
    }
}

// Set notification badge count
function setNotificationBadge(count) {
    var badge = document.getElementById('notification-badge');
    if (badge) {
        badge.textContent = count;
        badge.style.display = count > 0 ? 'inline' : 'none';
    }
}

// Add notification to dropdown
function addNotificationToDropdown(notification) {
    var list = document.getElementById('notification-list');
    if (list) {
        var emptyMessage = list.querySelector('.text-muted');
        if (emptyMessage) {
            emptyMessage.remove();
        }

        var item = document.createElement('li');
        item.innerHTML =
            '<a class="dropdown-item" href="#">' +
            '<small class="text-muted">' + new Date().toLocaleTimeString() + '</small><br>' +
            (notification.message || 'New notification') +
            '</a>';
        list.insertBefore(item, list.firstChild);
    }
}

// Remove notification from dropdown
function removeNotificationFromDropdown(notificationId) {
    var item = document.querySelector('[data-notification-id="' + notificationId + '"]');
    if (item) {
        item.remove();
    }
}

// Update provider status indicator
function updateProviderStatusIndicator(providerId, isAvailable) {
    var indicator = document.querySelector('[data-provider-id="' + providerId + '"] .status-indicator');
    if (indicator) {
        indicator.classList.remove('bg-success', 'bg-secondary');
        indicator.classList.add(isAvailable ? 'bg-success' : 'bg-secondary');
    }
}

// Update booking status
function updateBookingStatus(bookingId, status) {
    var element = document.querySelector('[data-booking-id="' + bookingId + '"]');
    if (element) {
        var badge = element.querySelector('.badge');
        if (badge) {
            badge.textContent = status;
        }
    }
}

// Show emergency alert modal
function showEmergencyAlert(emergency) {
    var modalHtml =
        '<div class="modal fade" id="emergencyModal" tabindex="-1">' +
        '<div class="modal-dialog">' +
        '<div class="modal-content border-danger">' +
        '<div class="modal-header bg-danger text-white">' +
        '<h5 class="modal-title"><i class="bi bi-exclamation-triangle"></i> Emergency Request!</h5>' +
        '<button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>' +
        '</div>' +
        '<div class="modal-body">' +
        '<p><strong>Service:</strong> ' + (emergency.serviceName || 'Unknown') + '</p>' +
        '<p><strong>Description:</strong> ' + (emergency.description || 'No description') + '</p>' +
        '<p><strong>Location:</strong> ' + emergency.latitude + ', ' + emergency.longitude + '</p>' +
        '</div>' +
        '<div class="modal-footer">' +
        '<a href="/Emergency/Pending" class="btn btn-danger">View All Emergencies</a>' +
        '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>';

    var existingModal = document.getElementById('emergencyModal');
    if (existingModal) {
        existingModal.remove();
    }

    document.body.insertAdjacentHTML('beforeend', modalHtml);
    var modal = new bootstrap.Modal(document.getElementById('emergencyModal'));
    modal.show();
}

// Remove emergency from list
function removeEmergencyFromList(emergencyId) {
    var element = document.querySelector('[data-emergency-id="' + emergencyId + '"]');
    if (element) {
        element.remove();
    }
}

// Play alert sound
function playAlertSound() {
    try {
        var audio = new Audio('/sounds/alert.mp3');
        audio.play().catch(function (e) { console.log('Audio play failed:', e); });
    } catch (e) {
        console.log('Audio not supported');
    }
}

// Play notification sound
function playNotificationSound() {
    try {
        var audio = new Audio('/sounds/notification.mp3');
        audio.play().catch(function (e) { console.log('Audio play failed:', e); });
    } catch (e) {
        console.log('Audio not supported');
    }
}