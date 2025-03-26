// Base URL for API
const API_BASE_URL = "https://localhost:5000";
// Hàm decode JWT
function decodeToken(token) {
    if (!token) {
        console.error("Token không tồn tại.");
        return null;
    }

    try {
        // Lấy phần payload (phần thứ 2 của JWT)
        const payload = token.split(".")[1];
        // Decode payload từ Base64
        const decodedPayload = atob(payload);
        // Parse JSON
        const decoded = JSON.parse(decodedPayload);
        return decoded;
    } catch (error) {
        console.error("Lỗi khi decode token:", error);
        return null;
    }
}

// Check authentication status
function checkAuth() {
    const token = localStorage.getItem("token");

    // Decode token để lấy thông tin
    const decodedToken = decodeToken(token);

    if (decodedToken) {
        // Trích xuất userId và role từ các claim
        const userId = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
        const name = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
        const role = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
        document.getElementById("loginLink").style.display = "none";
        document.getElementById("registerLink").style.display = "none";
        document.getElementById("userInfo").style.display = "block";
        document.getElementById("logoutLink").style.display = "block";
        document.getElementById("userName").textContent = `Hello, User ${name}!`;
        if (role === "Seller") {
            document.getElementById("createAuctionLink").style.display = "block";
        }
    } else {
        document.getElementById("loginLink").style.display = "block";
        document.getElementById("registerLink").style.display = "block";
        document.getElementById("userInfo").style.display = "none";
        document.getElementById("logoutLink").style.display = "none";
        document.getElementById("createAuctionLink").style.display = "none";
    }
}

// Login function
function login() {
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;
    const errorMessage = document.getElementById("errorMessage");

    fetch(`${API_BASE_URL}/api/auth/login`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ username, password })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Login failed");
            }
            return response.json();
        })
        .then(data => {
            localStorage.setItem("token", data.token);
            localStorage.setItem("userId", data.userId);
            localStorage.setItem("role", data.role);
            window.location.href = "/index.html";
        })
        .catch(error => {
            errorMessage.textContent = error.message;
            errorMessage.style.display = "block";
        });
}

// Register function
function register() {
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;
    const role = document.getElementById("role").value;
    const errorMessage = document.getElementById("errorMessage");

    fetch(`${API_BASE_URL}/api/auth/register`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ username, password, role })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Registration failed");
            }
            return response.json();
        })
        .then(data => {
            window.location.href = "/login.html";
        })
        .catch(error => {
            errorMessage.textContent = error.message;
            errorMessage.style.display = "block";
        });
}

// Logout function
function logout() {
    localStorage.removeItem("token");
    localStorage.removeItem("userId");
    localStorage.removeItem("role");
    window.location.href = "/index.html";
}

// Load auctions
function loadAuctions() {
    fetch(`${API_BASE_URL}/api/auction`)
        .then(response => response.json())
        .then(data => {
            const auctionList = document.getElementById("auctionList");
            auctionList.innerHTML = "";
            data.forEach(auction => {
                const row = document.createElement("tr");
                row.innerHTML = `
                <td>${auction.id}</td>
                <td>${auction.productName}</td>
                <td>${auction.currentPrice}</td>
                <td>${new Date(auction.endTime).toLocaleString()}</td>
                <td><span class="badge ${auction.isActive ? 'bg-success' : 'bg-danger'}">${auction.isActive ? 'Active' : 'Ended'}</span></td>
                <td><a href="/auction-details.html?id=${auction.id}" class="btn btn-info btn-sm">Details</a></td>
            `;
                auctionList.appendChild(row);
            });
        })
        .catch(error => console.error("Error loading auctions:", error));
}

// Create auction
function createAuction() {
    const productName = document.getElementById("productName").value;
    const startingPrice = parseFloat(document.getElementById("startingPrice").value);
    const reservePrice = parseFloat(document.getElementById("reservePrice").value);
    const auctionType = document.getElementById("auctionType").value;
    const errorMessage = document.getElementById("errorMessage");

    fetch(`${API_BASE_URL}/api/auction/create`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${localStorage.getItem("token")}`
        },
        body: JSON.stringify({ productName, startingPrice, reservePrice, auctionType })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Failed to create auction");
            }
            return response.json();
        })
        .then(data => {
            window.location.href = "/index.html";
        })
        .catch(error => {
            errorMessage.textContent = error.message;
            errorMessage.style.display = "block";
        });
}

// Load auction details
function loadAuctionDetails() {
    const urlParams = new URLSearchParams(window.location.search);
    const auctionId = urlParams.get("id");

    fetch(`${API_BASE_URL}/api/auction/${auctionId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error("Failed to load auction details");
            }
            return response.json();
        })
        .then(data => {
            document.getElementById("auctionId").textContent = data.id;
            document.getElementById("productName").textContent = data.productName;
            document.getElementById("currentPrice").textContent = data.currentPrice;
            document.getElementById("endTime").textContent = new Date(data.endTime).toLocaleString();
            document.getElementById("status").innerHTML = `<span class="badge ${data.isActive ? 'bg-success' : 'bg-danger'}">${data.isActive ? 'Active' : 'Ended'}</span>`;

            const bidHistory = document.getElementById("bidHistory");
            bidHistory.innerHTML = "";
            // Kiểm tra nếu data.Bids tồn tại và là mảng
            if (data.Bids && Array.isArray(data.Bids)) {
                data.Bids.forEach(bid => {
                    const li = document.createElement("li");
                    li.className = "list-group-item";
                    li.textContent = `User ${bid.userId}: ${bid.amount} (Auto: ${bid.isAuto}) at ${new Date(bid.bidTime).toLocaleString()}`;
                    bidHistory.appendChild(li);
                });
            } else {
                bidHistory.innerHTML = "<li class='list-group-item'>No bids yet.</li>";
            }
        })
        .catch(error => {
            const errorMessage = document.getElementById("errorMessage");
            errorMessage.textContent = error.message;
            errorMessage.style.display = "block";
        });
}

// Set Auto-Bid
function setAutoBid(auctionId) {
    const maxAmount = parseFloat(document.getElementById("maxAmount").value);
    const increment = parseFloat(document.getElementById("increment").value);
    const stopPercentage = parseFloat(document.getElementById("stopPercentage").value);
    const errorMessage = document.getElementById("errorMessage");

    fetch(`${API_BASE_URL}/api/autobid`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${localStorage.getItem("token")}`
        },
        body: JSON.stringify({ auctionId: parseInt(auctionId), maxAmount, increment, stopPercentage })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Failed to set auto-bid");
            }
            return response.json();
        })
        .then(data => {
            window.location.href = `/auction-details.html?id=${auctionId}`;
        })
        .catch(error => {
            errorMessage.textContent = error.message;
            errorMessage.style.display = "block";
        });
}

// Setup SignalR
function setupSignalR() {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE_URL}/auctionHub`, {
            accessTokenFactory: () => localStorage.getItem("token")
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on("JoinConfirmation", (message) => {
        const updatesList = document.getElementById("auctionUpdatesList");
        const li = document.createElement("li");
        li.className = "list-group-item";
        li.textContent = typeof message === "string" ? message : JSON.stringify(message);
        updatesList.appendChild(li);
    });

    connection.on("Ping", (message) => {
        const updatesList = document.getElementById("auctionUpdatesList");
        const li = document.createElement("li");
        li.className = "list-group-item";
        li.textContent = typeof message === "string" ? message : JSON.stringify(message);
        updatesList.appendChild(li);
    });

    connection.on("BidUpdated", (auctionId, newPrice) => {
        document.getElementById("currentPrice").textContent = newPrice;
        loadAuctionDetails();
    });

    connection.start()
        .then(() => {
            const urlParams = new URLSearchParams(window.location.search);
            const auctionId = urlParams.get("id");
            document.getElementById("joinButton").addEventListener("click", () => {
                connection.invoke("JoinAuction", parseInt(auctionId))
                    .catch(err => console.error("Error joining auction:", err));
                document.getElementById("bidButton").disabled = false;
            });

            document.getElementById("leaveButton").addEventListener("click", () => {
                connection.invoke("LeaveAuction", parseInt(auctionId))
                    .catch(err => console.error("Error leaving auction:", err));
                document.getElementById("bidButton").disabled = true;
            });

            document.getElementById("bidButton").addEventListener("click", () => {
                const bidAmount = parseFloat(document.getElementById("bidAmountInput").value);
                fetch(`${API_BASE_URL}/api/auction/bid`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${localStorage.getItem("token")}`
                    },
                    body: JSON.stringify({ auctionId: parseInt(auctionId), amount: bidAmount })
                })
                    .then(response => {
                        if (!response.ok) {
                            throw new Error("Failed to place bid");
                        }
                        return response.json();
                    })
                    .catch(error => {
                        const errorMessage = document.getElementById("errorMessage");
                        errorMessage.textContent = error.message;
                        errorMessage.style.display = "block";
                    });
            });
        })
        .catch(err => console.error("Error connecting to SignalR:", err));
}