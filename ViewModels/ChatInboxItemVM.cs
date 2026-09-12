namespace ApnaRent.ViewModels
{
    public class ChatInboxItemVM
    {
        public int ItemId { get; set; }
        public string OtherUserId { get; set; }
        public string OtherUserName { get; set; }
        public string ItemName { get; set; }
        public string LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
    }
}