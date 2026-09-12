namespace ApnaRent.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public string UserId { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int Days { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; } = "Cash on Delivery";
        public string? RejectionReason { get; set; }

        public ApplicationUser User { get; set; }
        public Item Item { get; set; }
        public string CNIC { get; set; }
        public string ContactNumber { get; set; }
        public string DeliveryLocation { get; set; }
    }
}