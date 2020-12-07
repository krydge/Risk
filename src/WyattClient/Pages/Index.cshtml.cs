using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WyattClient.Controllers;
using System.Net.Http.Json;
using Risk.Shared;
using Microsoft.Extensions.Configuration;

namespace WyattClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            ServerName = "http://risk.api";
        }

        [BindProperty(SupportsGet = true)]
        public string ServerName { get; set; }

        public async Task OnGetAsync()
        {
            var client = httpClientFactory.CreateClient();
            await refreshStatus(client);
        }

        private async Task refreshStatus(HttpClient client)
        {
            Status = await client.GetFromJsonAsync<GameStatus>($"{ServerName ?? configuration["GameServer"]}/status");
        }

        public GameStatus Status { get; set; }

        public async Task OnPostStartGameAsync(string server, string secretCode)
        {
            var client = httpClientFactory.CreateClient();
            await client.PostAsJsonAsync($"{server}/startgame", new StartGameRequest { SecretCode = secretCode });
            ServerName = server;
            await refreshStatus(client);
        }
    }
}
