using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace Emmanuel_Client.Controllers
{
    public class ClientController : Controller
    {
        private readonly IHttpClientFactory clientFactory;
        private static string serverAddress;

        public ClientController(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        [HttpGet("AreYouThere")]
        public string AreYouThere()
        {
            return "yes";
        }

        [HttpGet("/joinServer/{*server}")]
        public async Task<IActionResult> JoinServerAsync(string server)
        {
            serverAddress = server;
            var client = clientFactory.CreateClient();
            //string baseUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
            string baseUrl = "http://144.17.48.66:5009";
            var joinRequest = new JoinRequest {
                CallbackBaseAddress = baseUrl,
                Name = "Emmanuel"
            };
            try
            {
                var joinResponse = await client.PostAsJsonAsync($"{serverAddress}/join", joinRequest);
                var content = await joinResponse.Content.ReadAsStringAsync();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody]DeployArmyRequest deployArmyRequest)
        {
            return createDeployResponse(deployArmyRequest);
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            return createAttackResponse(beginAttackRequest);
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

        private BeginAttackResponse createAttackResponse(BeginAttackRequest beginAttack)
        {
            var from = new Location();
            var to = new Location();
            var tempTer = new BoardTerritory();

            //This logic will not grab a neighbour of the territory.
            foreach (var ter in beginAttack.Board)
            {
                if (!(ter.OwnerName is null) && ter.OwnerName == "Emmanuel" && ter.Armies > 1)
                {
                    from = ter.Location;
                    for (int i = ter.Location.Column - 1; i <= ter.Location.Column + 1; i++)
                    {
                        if (i < 0)
                        {
                            continue;
                        }
                        for (int j = ter.Location.Row - 1; j <= ter.Location.Row + 1; j++)
                        {
                            if (j < 0)
                            {
                                continue;
                            }
                            to.Column = i;
                            to.Row = j;
                            tempTer = beginAttack.Board.FirstOrDefault(r => r.Location == to);
                            if (!(tempTer is null) && tempTer.OwnerName != "Emmanuel" && tempTer.Armies > 0)
                            {
                                to = tempTer.Location;
                                return new BeginAttackResponse { From = from, To = to };
                            }
                        }
                    }
                }
            }

            return new BeginAttackResponse { From = from, To = to };
        }

        private DeployArmyResponse createDeployResponse(DeployArmyRequest deployArmyRequest)
        {
            Location location = new Location();
            int ownedTerritories = 0;
            int placedArmies = 0;
            int totalArmies = 0;
            foreach(var ter in deployArmyRequest.Board)
            {
                if(ter.OwnerName is null)
                {
                    location = ter.Location;
                    return new DeployArmyResponse { DesiredLocation = location };
                }
                if (ter.OwnerName == "Emmanuel")
                {
                    ownedTerritories++;
                    placedArmies += ter.Armies;
                }
            }

            totalArmies = deployArmyRequest.ArmiesRemaining + placedArmies;

            foreach (var ter in deployArmyRequest.Board)
            {
                if (ter.OwnerName == "Emmanuel" && ter.Armies < (totalArmies/ownedTerritories) + 1)
                {
                    location = ter.Location;
                    return new DeployArmyResponse { DesiredLocation = location };
                }
            }
            
            return new DeployArmyResponse { DesiredLocation = location };
        }
    }
}
