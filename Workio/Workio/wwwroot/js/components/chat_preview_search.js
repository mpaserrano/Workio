const createShareHTML = ({
    id,
    name,
    image,
    url
}) => `
<li class="list-group-item d-flex justify-content-between align-items-center" data-id="${id}">
        <div class="d-flex align-items-center">
            <div>
                <img src="${image}"
                    alt="avatar" class="d-flex align-self-center me-3 rounded-circle" width="60">
            </div>
            <div class="ms-3">
                <p class="fw-bold mb-1">${name}</p>
            </div>
        </div>
        <a class="btn btn-link btn-rounded btn-sm" onclick="shareInChat('${id}', '${url}')">Share</a>
        </a>
</li>
`;