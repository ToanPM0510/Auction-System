"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/auctionHub").build();

// Disable buttons until connection is established
document.getElementById("joinButton").disabled = true;
document.getElementById("leaveButton").disabled = true;
document.getElementById("bidButton").disabled = true;

// Handle Connected event
connection.on("Connected", function (data) {
    console.log("Connected:", data);
    document.getElementById("joinButton").disabled = false;
    var li = document.createElement("li");
    document.getElementById("auctionUpdatesList").appendChild(li);
    li.textContent = `${data.message} (ID: ${data.connectionId})`;
});

// Handle JoinConfirmation event
connection.on("JoinConfirmation", function (data) {
    console.log("JoinConfirmation:", data);
    document.getElementById("joinButton").disabled = true;
    document.getElementById("leaveButton").disabled = false;
    document.getElementById("bidButton").disabled = false;
    var li = document.createElement("li");
    document.getElementById("auctionUpdatesList").appendChild(li);
    li.textContent = `${data.message} (Auction ID: ${data.auctionId})`;
});

// Handle LeaveConfirmation event
connection.on("LeaveConfirmation", function (data) {
    console.log("LeaveConfirmation:", data);
    document.getElementById("joinButton").disabled = false;
    document.getElementById("leaveButton").disabled = true;
    document.getElementById("bidButton").disabled = true;
    var li = document.createElement("li");
    document.getElementById("auctionUpdatesList").appendChild(li);
    li.textContent = `${data.message} (Auction ID: ${data.auctionId})`;
});

// Handle Ping event
connection.on("Ping", function (data) {
    console.log("Ping:", data);
});

// Handle BidUpdated event
connection.on("BidUpdated", function (data) {
    console.log("BidUpdated:", data);
    document.getElementById("currentPrice").textContent = data.currentPrice;
    var li = document.createElement("li");
    document.getElementById("auctionUpdatesList").appendChild(li);
    li.textContent = `Auction ${data.auctionId}: Current Price ${data.currentPrice} by User ${data.bidderId} (Auto: ${data.isAuto}) at ${data.bidTime}`;
});

// Start connection
connection.start().then(function () {
    document.getElementById("joinButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

// Join Auction button
document.getElementById("joinButton").addEventListener("click", function (event) {
    var auctionId = @Model.Auction.Id;
    connection.invoke("JoinAuction", auctionId).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

// Leave Auction button
document.getElementById("leaveButton").addEventListener("click", function (event) {
    var auctionId = @Model.Auction.Id;
    connection.invoke("LeaveAuction", auctionId).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

// Place Bid button
document.getElementById("bidButton").addEventListener("click", function (event) {
    var auctionId = @Model.Auction.Id;
    var amount = parseFloat(document.getElementById("bidAmountInput").value);

    // Gửi bid qua form POST
    var form = document.createElement("form");
    form.method = "POST";
    form.action = `/Auctions/Details/PlaceBid?id=${auctionId}`;
    var input = document.createElement("input");
    input.type = "hidden";
    input.name = "BidAmount";
    input.value = amount;
    form.appendChild(input);
    document.body.appendChild(form);
    form.submit();

    event.preventDefault();
});