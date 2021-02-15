using System.Collections.Generic;
using WebApplication5.Controllers;

namespace WebApplication5.Models
{
    public class Statistics
    {
        public List<LabelStats> UserStats { get; set; }
        public List<LabelStats> GlobalStats { get; set; }
    }
}