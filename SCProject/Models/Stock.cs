namespace SCProject.Models
{
    public class Stock
    {
        public int ID { get; set; }      
        public int? PRODUCT_ID { get; set; }
        public double? QUANTITY { get; set; }
        public DateTime? DATE { get; set; }
    }
}
