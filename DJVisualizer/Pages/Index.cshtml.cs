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

namespace DJClient.Pages
{
    public class IndexModel : PageModel
    {
        private IHttpClientFactory httpClientFactory;
        private IConfiguration configuration;

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            
        }

        public GameStatus GameStatus { get; set; }
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }

      


        public async Task OnGetAsync()
        {
            GameStatus = await httpClientFactory.CreateClient()
                                                .GetFromJsonAsync<GameStatus>($"{configuration["GameServerAddress"]}/status");
            NumRows = GameStatus.Board.Max(t => t.Location.Row);
            NumCols = GameStatus.Board.Max(t => t.Location.Column);
        }
    }
}
