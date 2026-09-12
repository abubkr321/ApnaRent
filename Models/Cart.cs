namespace ApnaRent.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public string UserId { get; set; }

        public Item Item { get; set; }
        public ApplicationUser User { get; set; }
    }
}
