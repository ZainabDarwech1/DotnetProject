// ============================================
// Location Utilities for LebAssist
// ============================================

/**
 * Calculate distance between two points using Haversine formula
 */
function calculateDistance(lat1, lon1, lat2, lon2) {
    var R = 6371; // Earth radius in km
    var dLat = toRad(lat2 - lat1);
    var dLon = toRad(lon2 - lon1);
    var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) *
        Math.sin(dLon / 2) * Math.sin(dLon / 2);
    var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return R * c;
}

function toRad(degrees) {
    return degrees * Math.PI / 180;
}

/**
 * Format distance for display
 */
function formatDistance(distanceKm) {
    if (distanceKm < 1) {
        return Math.round(distanceKm * 1000) + ' m';
    }
    return distanceKm.toFixed(1) + ' km';
}

/**
 * Check if point is within radius
 */
function isWithinRadius(lat1, lon1, lat2, lon2, radiusKm) {
    return calculateDistance(lat1, lon1, lat2, lon2) <= radiusKm;
}

/**
 * Get current GPS location
 */
function getCurrentLocation(successCallback, errorCallback) {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            function (position) {
                successCallback({
                    lat: position.coords.latitude,
                    lng: position.coords.longitude
                });
            },
            function (error) {
                if (errorCallback) {
                    errorCallback(error);
                } else {
                    console.error('Geolocation error:', error);
                }
            },
            { enableHighAccuracy: true, timeout: 10000, maximumAge: 300000 }
        );
    } else {
        if (errorCallback) {
            errorCallback({ message: 'Geolocation not supported' });
        }
    }
}

/**
 * Open Google Maps with directions
 */
function openDirections(destLat, destLng) {
    var url = 'https://www.google.com/maps/dir/?api=1&destination=' + destLat + ',' + destLng;
    window.open(url, '_blank');
}

/**
 * Open Google Maps at location
 */
function openMapLocation(lat, lng) {
    var url = 'https://www.google.com/maps?q=' + lat + ',' + lng;
    window.open(url, '_blank');
}

/**
 * Lebanese cities with coordinates
 */
var lebaneseCities = [
    { name: 'Beirut', lat: 33.8938, lng: 35.5018 },
    { name: 'Tripoli', lat: 34.4367, lng: 35.8497 },
    { name: 'Sidon (Saida)', lat: 33.2705, lng: 35.2038 },
    { name: 'Tyre (Sour)', lat: 33.2721, lng: 35.1964 },
    { name: 'Zahle', lat: 33.8547, lng: 35.8623 },
    { name: 'Baalbek', lat: 34.0047, lng: 36.2110 },
    { name: 'Nabatieh', lat: 33.5608, lng: 35.3733 },
    { name: 'Jounieh', lat: 33.8172, lng: 35.5361 },
    { name: 'Byblos (Jbeil)', lat: 34.1236, lng: 35.6511 },
    { name: 'Baabda', lat: 33.9000, lng: 35.5500 }
];

/**
 * Get nearest city name from coordinates
 */
function getNearestCity(lat, lng) {
    var nearest = null;
    var minDistance = Infinity;

    lebaneseCities.forEach(function (city) {
        var distance = calculateDistance(lat, lng, city.lat, city.lng);
        if (distance < minDistance) {
            minDistance = distance;
            nearest = city;
        }
    });

    return nearest ? nearest.name : 'Unknown';
}