const createBubbleHTML = async ({
    id,
    name,
    sender_id,
    profilePicture,
    messageTime,
    text,
    isMine
}) => {
    const messageDate = new Date(messageTime);
    const options = { year: 'numeric', month: 'numeric', day: 'numeric', hour: 'numeric', minute: 'numeric' };
    const messageTimeFormatted = messageDate.toLocaleString(options);
    const pattern = /^((http|https):\/\/)/;
    const isLink = pattern.test(text);
    const messageText = isLink ? `<a href="${text}" style="${isMine ? "color: white" : "color: black"}" target="_blank">${text}</a>` : text;
    let previewHTML = '';
    if (isLink) {
        let linkData = {};
        try {
            const response = await fetch(`/Chat/GetMetaTags?url=${text}`);
            linkData = await response.json();
        } catch (error) {
            console.error(error);
        }
        if (linkData != {}) {
            previewHTML = `
                <div class="card" style="width: 23rem; ${isMine ? '' : `margin-left: 15px`}">
                    <div class="card-body">
                        <a href="${linkData.url}"><h5 class="card-title">${linkData.title}</h5></a>
                        <p class="card-text">
                            ${linkData.description}
                        </p>
                    </div>
                    ${linkData.image ? `<img src="${linkData.image}" class="card-img-top" alt="Article Image"/>` : ''}
                </div>
            `;
        }
    }
    return `
        <div class="d-flex flex-row justify-content-${isMine ? 'end' : 'start'}">
            ${isMine ? '' : `<img class="rounded-circle" src="/pfp/${profilePicture}" alt="avatar 1" style="width: 45px; height: 45px;">`}
            <div class="d-flex flex-column justify-content-center align-items-${isMine ? 'end' : 'start'}">
                <p class="small p-2 ms-3 mb-1 rounded-3 bubble-text" style="background-color: ${isMine ? '#0084ff' : '#f5f6f7'}; color: ${isMine ? '#fff' : '#000'};">
                    ${isMine ? '' : `<a href="User/Index/${sender_id}" class="bubble-name">${name}</a>`}
                    ${messageText}
                    ${previewHTML}
                    <p class="small ms-3 mb-3 rounded-3 text-muted float-end">${messageTimeFormatted}</p>
                </p>
            </div>
            ${isMine ? `<img class="rounded-circle ms-3" src="/pfp/${profilePicture}" alt="avatar 1" style="width: 45px; height: 45px;">` : ''}
        </div>
    `;
};