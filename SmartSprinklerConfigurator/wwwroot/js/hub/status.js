"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/hubs/status").build();


connection.on("StatusUpdateAsync", function (status, message) {
    document.getElementById("sprinkler_control_status").innerHTML = message;
});

connection.start();