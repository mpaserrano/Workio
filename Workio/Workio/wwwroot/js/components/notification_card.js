const createNotificationHTML = ({
    id,
    text,
    url,
    isRead,
    seeMoreText,
    newText,
    statusText
}) => `
<li class="list-group-item d-flex justify-content-start align-items-start" data data-id="${id}">
    <div class="badge-div ms-3 mt-auto mb-auto ${!isRead ? 'unread-badge' : 'read-badge'}">
        <span class="badge rounded-pill badge-primary">${newText}</span>
    </div>
    <div class="ms-3">
        <p class="${!isRead ? 'fw-bold' : ''}">${text}</p>
        <span><a href="${url}" onclick="changeStatus()" data-url="${url}" data-id="${id}" data-status="${isRead}">${seeMoreText}</a><a class="ms-2 pointer" onclick="changeStatus()" data-id="${id}" data-status="${isRead}">${statusText}</a></span>
    </div>
</li>
`;

function changeStatus() {
    // Read the data attributes from the anchor tag
    const link = event.target.getAttribute('data-url');
    const id = event.target.getAttribute('data-id');
    var status = event.target.getAttribute('data-status');

    if (link != null && status == "true") {
        console.log("oook123")
    }
    else {
        const xhr = new XMLHttpRequest();
        const url = `/Notifications/ChangeStatus?notificationId=${id}&status=${status}`;

        xhr.open('POST', url);

        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    if (link == null) {
                        var showRead = document.getElementById("showRead");
                        var showUnRead = document.getElementById("showUnread");
                        const item = document.querySelector(`#notificationsList [data-id="${id}"]`);

                        if (showRead.checked || showUnRead.checked) {
                            // Remove the clicked item from the list
                            if (item) {
                                item.remove();
                            }
                        }
                        else {
                            const item = document.querySelector(`#notificationsList [data-id="${id}"]`);
                            var links = item.querySelectorAll("a");
                            const title = item.querySelector("p");

                            if (status == "true") {
                                links[1].innerHTML = translations.unreadStatus;
                                links[0].setAttribute('data-status', "false");
                                links[1].setAttribute('data-status', "false");

                                if (title) {
                                    title.classList.add('fw-bold');
                                }
                            }
                            else {
                                links[1].innerHTML = translations.readStatus;
                                links[0].setAttribute('data-status', "true");
                                links[1].setAttribute('data-status', "true");

                                if (title) {
                                    title.classList.remove('fw-bold');
                                }
                            }

                            const badge_div = item.querySelector(".badge-div");
                            if (badge_div) {
                                badge_div.classList.remove(status == "false" ? 'unread-badge' : 'read-badge');
                                badge_div.classList.add(status == "false" ? 'read-badge' : 'unread-badge');
                            }
                            
                        }
                    }

                    loadNotificationsCount();
                } else {
                    toastr.error(`Error updating message status`, "Error");
                }
            }
        };

        xhr.setRequestHeader('Content-Type', 'application/json');

        xhr.send();
    }
}