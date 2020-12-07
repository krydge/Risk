using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Risk.Shared;

namespace Rusty_Client.Controllers
{
    
    public class RiskClientController : Controller
    {
        private readonly IHttpClientFactory clientFactory;
        private static string serverAddress;

        public RiskClientController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        [HttpGet("AreYouThere")]
        public string AreYouThere()
        {
            return "yes";
        }

        [HttpGet("/joinServer/{*server}")]
        public async Task<IActionResult> JoinAsync(string server) 
        {
            serverAddress = server;
            var client = clientFactory.CreateClient();
            string baseUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
            var joinRequest = new JoinRequest {

                CallbackBaseAddress = baseUrl,
                Name = "Rusty"
            };
            try
            {
                var joinResponse = await client.PostAsJsonAsync($"{serverAddress}/join", joinRequest);
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

       

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody] DeployArmyRequest deployArmyRequest)
        {
            return createDeployResponse(deployArmyRequest);
        }

        private DeployArmyResponse createDeployResponse(DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse response = new DeployArmyResponse();
            int owned=0;
            int placed = 0;
            int totalArmies = 0;
            foreach(BoardTerritory space in deployArmyRequest.Board)
            {
                if(space.OwnerName == null)
                {
                    response.DesiredLocation = space.Location;
                    owned++;
                    placed++;
                    return response;   
                }
                if (space.OwnerName == "Rusty")
                {
                    owned++;
                    placed += space.Armies;
                }
            }
            totalArmies = deployArmyRequest.ArmiesRemaining + placed;

            foreach(BoardTerritory space in deployArmyRequest.Board)
            {
                if (space.OwnerName == "Rusty" && (space.Armies <(totalArmies/owned+1)))
                {
                    response.DesiredLocation = space.Location;
                    return response;
                }
            }

            return null;

        }
        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            return createAttackResponse(beginAttackRequest); ;
        }
        private BeginAttackResponse createAttackResponse(BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();
            var attackerLocation = new Location();
            var defenderLocation = new Location();
            var temp = new BoardTerritory();
            //from is the attacker to is the defender
            foreach(BoardTerritory space in beginAttackRequest.Board)
            {
                if (space.OwnerName == "Rusty" && space.Armies>1)
                {
                    attackerLocation = space.Location;
                    //look at the next location to the right, left, up, down, up-right diagonal, 
                    //down-right diagonal, up-left diagonal, down-left diagonal
                    for(int i=space.Location.Column-1; i <= (space.Location.Column + 1); i++)
                    {
                        for(int j=space.Location.Row-1;j<=(space.Location.Row+1); j++)
                        {
                            if (j < 0)
                            {
                                continue;
                            }
                            defenderLocation.Column = i;
                            defenderLocation.Row = j;
                            temp = beginAttackRequest.Board.FirstOrDefault(d => d.Location == defenderLocation);
                            if((temp !=null)&&temp.OwnerName!="Rusty" && temp.Armies > 0)
                            {
                                return new BeginAttackResponse { From = attackerLocation, To = defenderLocation };
                            }
                        }
                    }
                    
                }
            }
            throw new Exception("No valid attack");
        }

        [HttpPost("continueAttacking")]
        public ContinueAttackResponse ContinueAttack([FromBody] ContinueAttackRequest continueAttackRequest)
        {
            ContinueAttackResponse response = new ContinueAttackResponse();
            response.ContinueAttacking = true;

            return response;
        }

        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }
    }
}
