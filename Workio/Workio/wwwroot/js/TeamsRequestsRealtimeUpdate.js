"use strict";

function getPendingRequests(teamId, callback) {
    const xhttp = new XMLHttpRequest();

    xhttp.onload = function () {
        var response = this.responseText;

        callback(response);
    }

    xhttp.open("GET", "/Teams/GetPendingListViewComponentResult?teamId=" + teamId + "", true);
    xhttp.send();
}

export default function updateTeamRequest(url) {
    let requestsOption = document.getElementById("teams-details-join-requests-tab-badge");

    if (requestsOption == null || requestsOption == undefined) return;

    let teamRequestsAmount = document.getElementsByClassName("team-details-requests-pending-amount");
    let teamRequestTableList = document.getElementById("teams-details-pending-request-table-content");
    /* remove optional end / */
    var lastSeg = url.substr(url.lastIndexOf('/') + 1);
    let teamId = lastSeg;
    console.log("ples");
    console.log(teamId);

    getPendingRequests(teamId, function (response) {
        teamRequestTableList.innerHTML = response;
        requestsOption.innerHTML = teamRequestsAmount[1].innerHTML;
        requestsOption.style.display = "initial";
    });
}