using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Controllers;
using WebApplication5.Models;

namespace WebApplication5.Services
{
    public interface IStatisticsService
    {
        public Task RateAsync(string label, bool correct, string user);

        public Task<Statistics> GetStatisticsAsync(string user = null);
    }
}
