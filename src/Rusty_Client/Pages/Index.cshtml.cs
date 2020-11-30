using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Risk.Shared;

namespace Rusty_Client.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration config;

        public IndexModel (IHttpClientFactory httpClient, IConfiguration configuration)
        {
            this.httpClientFactory = httpClient;
            this.config = configuration;
        }
        public GameStatus Status { get; set; }
        public int rows { get; set; }
        public int columns { get; set; }
        public async Task OnGetAsync()
        {
            Status = await httpClientFactory.CreateClient().GetFromJsonAsync<GameStatus>($"{ config["GameServer"]}/status");
            rows = Status.Board.Max(r => r.Location.Row);
            columns = Status.Board.Max(r => r.Location.Column);
        }

        public async Task<IActionResult> OnPostStartGameAsync()
        {
            var client = httpClientFactory.CreateClient();
            Task.Run(() => client.PostAsJsonAsync($"{config["GameServer"]}/startgame", new StartGameRequest { SecretCode = config["secretCode"] }));
            return new RedirectToPageResult("Index");
        }
       
    }
}
