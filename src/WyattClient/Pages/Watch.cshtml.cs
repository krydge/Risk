using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Risk.Shared;

namespace WyattClient.Pages
{
    public class VisualizeModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        public GameStatus Status { get; private set; }
        public int MaxRow { get; private set; }
        public int MaxCol { get; private set; }

        public VisualizeModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        public async Task OnGet()
        {
            Status = await httpClientFactory
                .CreateClient()
                .GetFromJsonAsync<GameStatus>($"{configuration["GameServer"]}/status");
            MaxRow = Status.Board.Max(t => t.Location.Row);
            MaxCol = Status.Board.Max(t => t.Location.Column);
        }
    }
}
