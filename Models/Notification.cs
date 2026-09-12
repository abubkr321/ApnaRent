namespace ApnaRent.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public int? RelatedBookingId { get; set; }
        public int? RelatedItemId { get; set; }
        public string? RelatedOtherUserId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}