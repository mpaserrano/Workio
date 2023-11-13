const CreateSendMessageBar = () => `
<div class="text-muted d-flex justify-content-start align-items-center pe-3 pt-3 mt-2">
    <img class="rounded-circle" src="/pfp/default.png"
        alt="avatar 3" style="width: 40px; height: 40px;" id="input-avatar-message">
    <input type="text" class="form-control form-control-lg" id="message_to_send" onchange="sendMessage()"
        placeholder="Type message" maxlength="250">
    <!--<a class="ms-1 text-muted" href="#!"><i class="fas fa-paperclip"></i></a>
    <a class="ms-3 text-muted" href="#!"><i class="fas fa-smile"></i></a>-->
    <a class="ms-3 pointer" id="send-message-btn-icon" ontouchend="sendMessage()"><i class="fas fa-paper-plane"></i></a>
</div>
` 