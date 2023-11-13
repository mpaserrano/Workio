"use strict";

import updateTeamRequest from './TeamsRequestsRealtimeUpdate.js';

function formatURL(url) {
    // Remove trailing slashes
    while (url.endsWith('/')) {
        url = url.slice(0, -1);
    }

    // Check for queries
    const queryIndex = url.indexOf('?');
    if (queryIndex !== -1) {
        // Remove queries
        url = url.substring(0, queryIndex);
    }

    return url;
}

var lockResolver;
let userCurrentUrl = formatURL(window.location.href);
console.log(userCurrentUrl);

if (navigator && navigator.locks && navigator.locks.request) {
    const promise = new Promise((res) => {
        lockResolver = res;
    });

    navigator.locks.request('unique_lock_name', { mode: "shared" }, () => {
        return promise;
    });
}

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationsHub")
    .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
            if (retryContext.elapsedMilliseconds < 60000) {
                // If we've been reconnecting for less than 60 seconds so far,
                // wait between 0 and 10 seconds before the next reconnect attempt.
                return Math.random() * 10000;
            } else {
                // If we've been reconnecting for more than 60 seconds so far, stop reconnecting.
                return null;
            }
        }
    })
    .build();

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.onclose(async () => {
    await start();
});

function triggerNotificationsBadges(userUrl, notificationRedirectUrl) {
    if (userUrl == notificationRedirectUrl)
        updateTeamRequest(notificationRedirectUrl);
}

connection.on("ReceiveNotification", (notification) => {

    triggerNotificationsBadges(userCurrentUrl, notification.url);

    toastr.info(`${notification.text}`, "Notification", {
        onclick: function () {
            // handle click event here
            window.location.href = notification.url;
        }
    });

    loadNotificationsCount();
});

// Start the connection.
start();