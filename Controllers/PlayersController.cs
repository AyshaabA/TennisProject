using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TennisProject.Controllers
{
    [ApiController]
    [Route("api/players")]
    public class PlayersController : ControllerBase
    {
        private readonly List<Player> players; 


        public PlayersController()
        {
            // Chargez les données des joueurs à partir du fichier headtohead.json
            var json = System.IO.File.ReadAllText("headtohead.json");
            players = JsonConvert.DeserializeObject<PlayersData>(json).Players;
        }

        // Tâche n°1

        [HttpGet("players")]
        public IActionResult GetPlayers()
        {
            var sortedPlayers = players.OrderByDescending(p => p.Data.Points).ToList();
            return Ok(sortedPlayers);
        }


        // Tâche n°2

        [HttpGet("players/{id}")]
        public IActionResult GetPlayerById(int id)
        {
            var player = players.FirstOrDefault(p => p.Id == id);
            if (player == null)
                return NotFound();

            return Ok(player);
        }

        // Tâche n°3 
        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            var bestWinRatioCountry = GetBestWinRatioCountry();
            var averageIMC = GetAverageIMC();
            var medianHeight = GetMedianHeight();

            var statistics = new
            {
                BestWinRatioCountry = bestWinRatioCountry,
                AverageIMC = averageIMC,
                MedianHeight = medianHeight
            };

            return Ok(statistics);
        }
        // Cette fonction permet de retourner pays qui a le plus grand ratio de parties gagnées
        private string GetBestWinRatioCountry()
        {
            var countryWinRatios = players.GroupBy(p => p.Country.Code)
                                          .Select(g => new
                                          {
                                              CountryCode = g.Key,
                                              WinRatio = g.Average(p => p.Data.Last.Count(result => result == 1))
                                          })
                                          .OrderByDescending(c => c.WinRatio)
                                          .FirstOrDefault();

            return countryWinRatios?.CountryCode ?? "N/A";
        }

        // Cette fonction permet de retourner IMC moyen de tous les joueurs
        private double GetAverageIMC()
        {
            var totalIMC = players.Sum(p => p.Data.Weight / Math.Pow(p.Data.Height / 100.0, 2));
            var averageIMC = totalIMC / players.Count;

            return averageIMC;
        }

        // Cette fonction permet de retourner la médiane de la taille des joueurs
        private int GetMedianHeight()
        {
            var sortedHeights = players.OrderBy(p => p.Data.Height);
            var middleIndex = players.Count / 2;

            var medianHeight = players.Count % 2 == 0
                ? (sortedHeights.ElementAt(middleIndex - 1).Data.Height + sortedHeights.ElementAt(middleIndex).Data.Height) / 2
                : sortedHeights.ElementAt(middleIndex).Data.Height;

            return medianHeight;
        }


    }
}
