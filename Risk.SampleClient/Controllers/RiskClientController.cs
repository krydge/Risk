using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Risk.Shared;

namespace Risk.SampleClient.Controllers
{
    public class RiskClientController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration config;
        private static string serverAdress;

        public RiskClientController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            this.httpClientFactory = httpClientFactory;
            this.config = config;
        }

        [HttpGet("[action]")]
        public string AreYouThere()
        {
            return "yes";
        }

        public record DeployArmyRequest
        {
            public IEnumerable<BoardTerritory> Board { get; internal set; }
        }
        public record DeployArmyResponse
        {
            public Location DesiredLocation { get; internal set; }
        }

        [HttpPost("deployArmy")]
        public DeployArmyResponse DeployArmy([FromBody] DeployArmyRequest deployArmyRequest)
        {
            DeployArmyResponse response = new DeployArmyResponse();
            var myTerritory = deployArmyRequest.Board.FirstOrDefault(t => t.OwnerName == config["PlayerName"]) ??  deployArmyRequest.Board.Skip(deployArmyRequest.Board.Count() / 2).First(t => t.OwnerName == null);
            response.DesiredLocation = myTerritory.Location;
            return response;
        }

        private IEnumerable<BoardTerritory> GetNeighbors(BoardTerritory territory, IEnumerable<BoardTerritory> board)
        {
            var l = territory.Location;
            var neighborLocations = new[] {
                new Location(l.Row+1, l.Column-1),
                new Location(l.Row+1, l.Column),
                new Location(l.Row+1, l.Column+1),
                new Location(l.Row, l.Column-1),
                new Location(l.Row, l.Column+1),
                new Location(l.Row-1, l.Column-1),
                new Location(l.Row-1, l.Column),
                new Location(l.Row-1, l.Column+1),
            };
            return board.Where(t => neighborLocations.Contains(t.Location));
        }

        public record BeginAttackResponse
        {
            public Location From { get; internal set; }
            public Location To { get; internal set; }
        }
        public record BeginAttackRequest
        {
            public IEnumerable<BoardTerritory> Board { get; internal set; }
        }

        [HttpPost("beginAttack")]
        public BeginAttackResponse BeginAttack([FromBody]BeginAttackRequest beginAttackRequest)
        {
            BeginAttackResponse response = new BeginAttackResponse();

            foreach(var myTerritory in beginAttackRequest.Board.Where(t => t.OwnerName == config["PlayerName"]).OrderByDescending(t => t.Armies))
            {
                var myNeighbors = GetNeighbors(myTerritory, beginAttackRequest.Board);
                var destination = myNeighbors.Where(t => t.OwnerName != config["PlayerName"]).OrderBy(t => t.Armies).FirstOrDefault();
                if(destination != null)
                {
                    response.From = myTerritory.Location;
                    response.To = destination.Location;
                    return response;
                }
            }
            throw new Exception("No territory I can attack");
        }

        public record ContinueAttackResponse
        {
            public bool ContinueAttacking { get; internal set; }
        }
        public record ContinueAttackRequest { }

        [HttpPost("continueAttacking")]
        public ContinueAttackResponse ContinueAttack([FromBody]ContinueAttackRequest continueAttackRequest)
        {
            ContinueAttackResponse response = new ContinueAttackResponse();
            response.ContinueAttacking = true;

            return response;
        }

        public record GameOverRequest { }

        [HttpPost("gameOver")]
        public IActionResult GameOver([FromBody] GameOverRequest gameOverRequest)
        {
            return Ok(gameOverRequest);
        }

    }
}
