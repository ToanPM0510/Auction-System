using Microsoft.AspNetCore.SignalR;

namespace AuctionSystem.Hubs
{
    public class AuctionHub : Hub
    {
        public async Task JoinAuction(int auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString());
            await Clients.Caller.SendAsync("JoinConfirmation", $"You have joined auction {auctionId}");
        }

        public async Task LeaveAuction(int auctionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString());
            await Clients.Caller.SendAsync("JoinConfirmation", $"You have left auction {auctionId}");
        }

        public async Task SendPing(int auctionId)
        {
            await Clients.Group(auctionId.ToString()).SendAsync("Ping", $"Ping from auction {auctionId} at {DateTime.UtcNow}");
        }

        public async Task UpdateBid(int auctionId, double newPrice)
        {
            await Clients.Group(auctionId.ToString()).SendAsync("BidUpdated", auctionId, newPrice);
        }
    }
}