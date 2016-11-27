/// <reference path="../../node_modules/@types/jquery/index.d.ts"/> 
var host = window.location.host;
var ws = new WebSocket('ws://' + host + '/');
$(function () {
    $('form').submit(function () {
        var value = $('#m').val();
        if (value.match("\\S")) {
            var packet = {
                "text": $('#m').val()
            };
            ws.send(JSON.stringify(packet));
            $('#m').val('');
        }
        return false;
    });
    // shortcut keys
    $(document).on('keydown', '#m', function (e) {
        if (e.ctrlKey) {
            if (e.keyCode === 13) {
                $('form').submit();
            }
        }
    });
    ws.onmessage = function (msg) {
        var returnObject = JSON.parse(msg.data);
        var text = returnObject.text;
        var isBot = returnObject.type === "bot";
        var isSuccess = returnObject.success;
        var content = text.split('\n').join('<br />');
        $('#messages').append($('<li class="' + (isBot ? 'bot' : 'user') + ' ' + (isSuccess ? 'success' : 'fail') + '">')
            .append($('<span class="clientMessage">' + content + '<span />')));
        $('#messages').animate({ scrollTop: $(document).height() }, 0);
    };
    ws.onerror = function (err) {
        console.log("err", err);
    };
    ws.onclose = function () {
        console.log('disconnected');
    };
});
