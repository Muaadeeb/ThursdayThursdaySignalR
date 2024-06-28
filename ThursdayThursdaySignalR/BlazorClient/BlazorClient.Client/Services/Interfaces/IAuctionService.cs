namespace BlazorClient.Client.Services.Interfaces;

public interface IAuctionService
{
    event Action<string, string, decimal>? BidReceived;
    event Action<string, string>? AuctionEnded;

    Task StartAsync(); // Starts the auction service, e.g., connecting to SignalR
    Task StopAsync(); // Stops the auction service
    Task SendBid(string item, string user, decimal bidAmount); // Sends a bid
    bool IsConnected { get; }
}
