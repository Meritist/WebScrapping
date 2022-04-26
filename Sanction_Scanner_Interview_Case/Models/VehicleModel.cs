namespace Sanction_Scanner_Interview_Case.Models
{
    internal class VehicleModel
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public IDictionary<string, string> Properties { get; set; }
        public string Link { get; set; }
    }
}
