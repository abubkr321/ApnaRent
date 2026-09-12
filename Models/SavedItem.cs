namespace ApnaRent.Models
{
    public class SavedItem
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public DateTime SavedOn { get; set; } = DateTime.Now;
    }
}