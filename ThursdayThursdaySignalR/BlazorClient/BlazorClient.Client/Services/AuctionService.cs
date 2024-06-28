using BlazorClient.Client.Services.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;


namespace BlazorClient.Client.Services
{
    public class AuctionService : IAuctionService, IDisposable
    {
        private HubConnection _hubConnection;
        public event Action<string, string, decimal>? BidReceived;
        public event Action<string, string>? AuctionEnded;

        public AuctionService()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7275/auctionhub")
                .Build();

            _hubConnection.On<string, string, decimal>("ReceiveBid", (item, user, bidAmount) =>
            {
                BidReceived?.Invoke(item, user, bidAmount);
            });

            _hubConnection.On<string, string>("AuctionEnded", (item, winner) =>
            {
                AuctionEnded?.Invoke(item, winner);
            });

            _hubConnection.Closed += async (error) =>
            {
                // Implement reconnection logic or notify the user
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await StartAsync();
            };
        }

        public async Task StartAsync()
        {
            await _hubConnection.StartAsync();
        }

        public async Task StopAsync()
        {
            await _hubConnection.StopAsync();
        }

        public async Task SendBid(string item, string user, decimal bidAmount)
        {
            await _hubConnection.InvokeAsync("SendBid", item, user, bidAmount);
        }

        public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

        public void Dispose()
        {
            _hubConnection?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
