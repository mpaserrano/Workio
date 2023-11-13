const createUserTopbarHTML = ({
    id,
    userId,
    name,
    image
}) => `
<div class="p-3 d-flex flex-row justify-content-start align-items-center chat-topbar">
    <img src="${image}"
        alt="avatar 1" class="align-self-start rounded-circle" style="width: 45px; height: 45px;">
    <h5 class="ms-3 me-auto"><a href="User/Index/${userId}">${name}</a></h5>
    </div>
</div>
`;

const createTeamTopbarHTML = ({
    id,
    teamId,
    name,
    image,
    translation
}) => `
<div class="p-3 d-flex flex-row justify-content-start align-items-center chat-topbar">
    <img src="${image}"
        alt="avatar 1" class="align-self-start rounded-circle" style="width: 45px; height: 45px;">
    <h5 class="ms-3 me-auto"><a href="Teams/Details/${teamId}">${name}</a></h5>
    <div style="margin-left: 10px;" class="dropdown">
        <a class="btn btn-floating shadow-none text-reset dropdown-toggle hidden-arrow"
           href="#"
           id="navbarDropdownMenuLink"
           role="button"
           data-mdb-toggle="dropdown"
           aria-expanded="false"
        >
            <i style="font-size: 34px;" class="fa-solid fa-ellipsis-vertical"></i>
        </a>
        <ul class="dropdown-menu dropdown-menu-end"
            aria-labelledby="navbarDropdownMenuLink"
        >
            <li>
                <a class="dropdown-item" href="#" onclick="leaveChatConfirmation()">
                    <i class="fa-solid fa-ban"></i> <span>${translation.leaveChat}</span>
                </a>
           </li>
        </ul>
    </div>
</div>
`;

