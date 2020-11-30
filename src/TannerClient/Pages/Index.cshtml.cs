using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Risk.Shared;

namespace TannerClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration config;

        public IndexModel(IHttpClientFactory httpClient, IConfiguration configuration)
        {
            httpClientFactory = httpClient;
            config = configuration;
        }

        public GameStatus Status { get; set; }
        public int rows { get; set; }
        public int columns { get; set; }
        public string ServerName { get; set; }


        public async Task OnGet()
        {
            Status = await httpClientFactory.CreateClient().GetFromJsonAsync<GameStatus>($"{ config["GameServer"]}/status");
            rows = Status.Board.Max(r => r.Location.Row);
            columns = Status.Board.Max(c => c.Location.Column);
        }

        public async Task<IActionResult> OnPostStartGameAsync(string server, string secretCode)
        {
            var client = httpClientFactory.CreateClient();
            await client.PostAsJsonAsync($"{server}/startgame", new StartGameRequest { SecretCode = secretCode });
            ServerName = server;
            return new RedirectToPageResult("Index");
        }
    }
}
