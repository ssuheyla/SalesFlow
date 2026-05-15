namespace SCProject.Models
{
    public class Purchase
    {
        public int ID { get; set; }    
        public int? PRODUCT_ID { get; set; }
        public double? QUANTITY { get; set; }
        public double? PRICE { get; set; }
        public double? AMOUNT { get; set; }
        public DateTime? DATE { get; set; }
        public int? CUSTOMER_ID { get; set; }
    }
}
