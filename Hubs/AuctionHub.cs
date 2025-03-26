using Microsoft.AspNetCore.SignalR;

namespace AuctionSystem.Hubs
{
    public class AuctionHub : Hub
    {
        public async Task JoinAuction(int auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString());
            Console.WriteLine($"Client {Context.ConnectionId} joined Auction {auctionId}");
        }

        public async Task LeaveAuction(int auctionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString());
            Console.WriteLine($"Client {Context.ConnectionId} left Auction {auctionId}");
        }
    }
}