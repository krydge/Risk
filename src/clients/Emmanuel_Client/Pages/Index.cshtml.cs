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

namespace Emmanuel_Client.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        public GameStatus Status { get; set; }
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }

        public async Task OnGet()
        {
            Status = await httpClientFactory.CreateClient().GetFromJsonAsync<GameStatus>($"{ configuration["GameServer"]}/status");
            NumRows = Status.Board.Max(r => r.Location.Row);
            NumCols = Status.Board.Max(c => c.Location.Column);
        }
    }
}
