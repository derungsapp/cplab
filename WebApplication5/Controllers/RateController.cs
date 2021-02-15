using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using WebApplication5.Models;
using WebApplication5.Services;

namespace WebApplication5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RateController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public RateController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        // POST api/<RateController>
        [HttpPost]
        public async Task<Statistics> Post()
        {
            var user = User?.Claims?.FirstOrDefault(p => p.Type == "cognito:username")?.Value;
            var correct = Convert.ToBoolean(Request.Form["correct"]);
            var label = Request.Form["label"].ToString();

            await _statisticsService.RateAsync(label, correct, user);

            return await _statisticsService.GetStatisticsAsync(user);
        }

    }
}
