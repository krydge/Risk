using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;
using System.Net.Http;
using System.Net.Http.Json;

namespace TannerClient.Controllers
{
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        //private static string serverAdress;
        private GamePlayer gamePlayer;

        public ClientController(IHttpClientFactory httpClientFactory, IPlayer player)
        {
            this.httpClientFactory = httpClientFactory;
            gamePlayer = new GamePlayer { Player = player };
        }

        /*[HttpGet("joinServer/{*server}")]
        public async Task<IActionResult> JoinAsync(string server)
        {
            serverAdress = server;
            var client = httpClientFactory.CreateClient();
            string baseUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
            var joinRequest = new JoinRequest {
                CallbackBaseAddress = baseUrl,
                Name = "Tanner Client"
            };
            try
            {
                var joinResponse = await client.PostAsJsonAsync($"{serverAdress}/join", joinRequest);
                var content = await joinResponse.Content.ReadAsStringAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("joinServer")]
        public async Task<IActionResult> JoinAsync_Post(string server)
        {
            await JoinAsync(server);
            return RedirectToPage("/GameStatus", new { servername = server });
        }
        */
        [HttpGet("[action]")]
        public string AreYouThere()
        {
            return "yes";
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody] DeployArmyRequest deployArmyRequest)
        {
            return gamePlayer.DeployArmy(deployArmyRequest);
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            return gamePlayer.DecideBeginAttack(beginAttackRequest);
        }

        [HttpPost("continueAttacking")]
        public ContinueAttackResponse ContinueAttack([FromBody] ContinueAttackRequest continueAttackRequest)
        {
            return gamePlayer.DecideContinueAttackResponse(continueAttackRequest);
        }

        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }
    }
}
