namespace NasaApp.Models
{
    public class Consumer
    {
        public int ID { get; set; }
        public int NetworkID { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        // The energy consumation of the device in kW/h.
        public double EnergyConsumation { get; set; }
        // The total number of hours the device is used per day.
        public double UsageHours { get; set; }
        // The number of consumers of this kind.
        public int Count { get; set; }
    }
}