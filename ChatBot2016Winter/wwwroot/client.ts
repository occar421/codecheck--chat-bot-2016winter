/// <reference path="../../node_modules/@types/jquery/index.d.ts"/> 

let host = window.location.host;
let ws = new WebSocket('ws://' + host + '/');

$(function () {
    $('form').submit(() => {
        let value: string = $('#m').val();
        if (value.match("\\S")) { // contains non-whitespace
            let packet = {
                "text": $('#m').val()
            };
            ws.send(JSON.stringify(packet));
            $('#m').val('');
        }
        return false;
    });

    // shortcut keys
    $(document).on('keydown', '#m', (e) => {
        if (e.ctrlKey) {
            if (e.keyCode === 13) { // Ctrl + Enter key
                $('form').submit();
            }
        }
    });

    ws.onmessage = (msg) => {
        let returnObject = JSON.parse(msg.data);
        let text: string = returnObject.text;
        let isBot: boolean = returnObject.type === "bot";
        let isSuccess: boolean = returnObject.success;
        var content = text.split('\n').join('<br />');
        $('#messages').append($('<li class="' + (isBot ? 'bot' : 'user') + ' ' + (isSuccess ? 'success' : 'fail') + '">')
            .append($('<span class="clientMessage">' + content + '<span />')));

        $('#messages').animate({ scrollTop: $(document).height() }, 0);
    };

    ws.onerror = (err) => {
        console.log("err", err);
    };

    ws.onclose = () => {
        console.log('disconnected');
    };
});