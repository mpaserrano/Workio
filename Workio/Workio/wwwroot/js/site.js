// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function loadNotificationsCount() {
    const xhttp = new XMLHttpRequest();
    xhttp.onload = function () {
        var response = this.responseText;
        if (response < 1) {
            document.getElementById("notificationsCounter").style.display = "none";
        } else {
            document.getElementById("notificationsCounter").style.display = "initial";
            document.getElementById("notificationsCounter").innerHTML = response;
        }
        
    }

    xhttp.open("GET", "/Notifications/GetNotificationsCount", true);

    xhttp.send();
}

function loadChatCount() {
    const xhttp = new XMLHttpRequest();
    xhttp.onload = function () {
        var response = this.responseText;
        if (response < 1) {
            document.getElementById("chatCounter").style.display = "none";
        } else {
            document.getElementById("chatCounter").style.display = "initial";
            document.getElementById("chatCounter").innerHTML = response;
        }

    }

    xhttp.open("GET", "/Chat/GetChatCount", true);

    xhttp.send();
}