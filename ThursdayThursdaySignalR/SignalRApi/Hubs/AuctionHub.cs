using Microsoft.AspNetCore.SignalR;

namespace SignalRApi.Hubs;

public class AuctionHub : Hub
{
    private readonly IHubContext<AuctionHub> _hubContext;

    private static System.Timers.Timer? _auctionTimer;
    private static decimal _highestBid = 0;
    private static string _highestBidder = string.Empty;
    private static bool _auctionInProgress = false;
    private static readonly object _lock = new object();

    public AuctionHub(IHubContext<AuctionHub> hubContext)
    {
        _hubContext = hubContext;

        if (_auctionTimer == null)
        {
            _auctionTimer = new System.Timers.Timer(10000); // 10 seconds countdown
            _auctionTimer.Elapsed += async (sender, e) => await EndAuction();
            _auctionTimer.AutoReset = false; // Run the timer only once
        }
    }

    public Task SendBid(string item, string user, decimal bidAmount)
    {
        lock (_lock)
        {
            if (bidAmount > _highestBid)
            {
                _highestBid = bidAmount;
                _highestBidder = user;

                // Notify all clients about the new highest bid
                _ = Clients.All.SendAsync("ReceiveBid", item, user, bidAmount);

                // If this is the first bid, start the auction timer
                if (!_auctionInProgress)
                {
                    _auctionInProgress = true;
                    _auctionTimer?.Start();
                }
            }
        }

        return Task.CompletedTask;
    }

    private async Task EndAuction()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_auctionInProgress)
                {
                    // Notify all clients that the auction has ended using _hubContext
                    _ = _hubContext.Clients.All.SendAsync("AuctionEnded", "ItemName", _highestBidder);

                    // Reset auction state
                    _highestBid = 0;
                    _highestBidder = string.Empty;
                    _auctionInProgress = false;
                }
            }
        });
    }
}
