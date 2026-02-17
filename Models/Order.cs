
    namespace Authentication.Models
    {
        public class Order
        {
            public int Id { get; set; }

            public string UserId { get; set; }

            public decimal TotalAmount { get; set; }

            public string PaymentId { get; set; }

            public DateTime OrderDate { get; set; }
        }
    }



