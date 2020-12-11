using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Risk.Shared;

namespace Justin_Client
{
    public class RiskClientController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private static string serverAdress;

        public RiskClientController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet("joinServer/{*server}")]
        public async Task<IActionResult> JoinAsync(string server)
        {
            serverAdress = server;
            var client = httpClientFactory.CreateClient();
            string baseUrl = string.Format("{0}://{1}{2}", Request.Scheme, Request.Host, Request.PathBase);
            var joinRequest = new JoinRequest {
                CallbackBaseAddress = baseUrl,
                Name = "Justin's Client"
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

        [HttpGet("[action]")]
        public string AreYouThere()
        {
            return "yes";
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody] DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse response = new DeployArmyResponse();
            foreach (var t  in deployArmyRequest.Board)
            {
                if(t.OwnerName == null)
                {
                    
                    response.DesiredLocation = t.Location;
                    return response;
                }
                
            }
          
            foreach (var t in deployArmyRequest.Board)
            {
                if(t.OwnerName == "Justin")
                {
                    response.DesiredLocation = t.Location;
                } 
            }
            throw new Exception("could not deploy army");
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody] BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();

            var myControlledTerritories = new List<Location>();
            foreach (var myTerritory in beginAttackRequest.Board.Where(t => t.OwnerName == "Justin" && t.Armies > 1))
            {
                myControlledTerritories.Add(myTerritory.Location);
            }

            var enemyControlledTerritories = new List<Location>();
            foreach (var enemyTerritory in beginAttackRequest.Board.Where(t => t.OwnerName != "Justin"))
            {
                enemyControlledTerritories.Add(enemyTerritory.Location);
            }

            var possibleAttackLocations = new List<Tuple<int, int>>();
           foreach(var x in myControlledTerritories)
            {
                foreach (var y in enemyControlledTerritories)
                {
                    var isValidAttackLocation = isValidLocation(x, y);
                   if (isValidAttackLocation.Item1 == true)
                    {
                       
                    }
                    
                }
            }

            response.From = new Location(1, 1);
            response.To = new Location(1, 2);
            return response;
        }
        

        private Tuple<bool, int, int> isValidLocation(Location from, Location to)
        {
            
            var columnCorrect = false;
            if((to.Column == from.Column-1)||(to.Column == from.Column+1))
            {
                columnCorrect = true;
            }

            var rowCorrect = false;
            if((to.Row == from.Row-1)||(to.Row == from.Row+1))
            {
                rowCorrect = true;
            }
            
            if(rowCorrect == false && columnCorrect == false)
            {
                var tupleTrue = Tuple.Create(true,to.Row, to.Column);
                return tupleTrue;
            }
            else
            {
                var tupleFalse = Tuple.Create(false, to.Row, to.Column);
                return tupleFalse;
            }

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
