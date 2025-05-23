namespace Server.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Upc { get; set; }    // or SKU
        public decimal Price { get; set; }
        public double Vat { get; set; }    // e.g. 21 for 21%
    }
}
