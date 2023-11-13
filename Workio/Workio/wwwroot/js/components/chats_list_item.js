const createContactHTML = ({
    id,
    name,
    image,
    lastMessageText,
    lastMessageTime,
    isRead,
    translations
}) => `
<li class="p-2 border-bottom">
    <a href="#!" onclick="showConversation('${id}')" class="d-flex justify-content-start" data-id="${id}">
        <div>
            <img src="${image}"
                alt="avatar" class="d-flex align-self-center me-3 rounded-circle" width="60" height="60">
        </div>
        <div class="pt-1 me-auto">
            <p class="fw-bold mb-0">${name}</p>
            <p class="small ${isRead ? `text-muted` : `fw-bold`} last-message">${lastMessageText}</p>
        </div>
        <div class="pt-1">
            <div class="pt-1">
                <p class="small text-muted mb-1 message-time text-end">
                    ${lastMessageTime === null || lastMessageTime === "null"
                        ? '' : `${lastMessageTime}`
                    }
                </p> 
            </div>
            <div class="d-flex" style="${isRead ? `display: none !important;` : ``}">
                <span class="w-100 badge bg-success text-center">${translations.new}</span>
            </div> 
        </div>
    </a>
</li>
`;