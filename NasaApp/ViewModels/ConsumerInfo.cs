using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NasaApp.Database;
using NasaApp.Models;

namespace NasaApp.ViewModels
{
    public class ConsumerInfo
    {
        public double TotalUsage { get; private set; }
        public double ConsumerCount { get; private set; }

        public ConsumerInfo(double totalUsage, double consumerCount)
        {
            TotalUsage = totalUsage;
            ConsumerCount = consumerCount;
        }

        public static async Task<ConsumerInfo> FromDBAsync(DatabaseHelper db)
        {
            IEnumerable<Consumer> consumers = await db.Consumers.SelectAllAsync();
            return new ConsumerInfo(
                totalUsage: consumers.Sum(x => x.EnergyConsumation * x.Count),
                consumerCount: consumers.Select(x => x.Count).Sum()
            );
        }
    }
}