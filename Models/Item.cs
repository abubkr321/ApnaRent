using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApnaRent.Models
{
    public class Item
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PricePerDay { get; set; }

        public int AvailabilityCount { get; set; }
        public bool IsAvailable { get; set; }

        public string Location { get; set; }
        public int? CategoryId { get; set; }

        [ValidateNever]
        public Category Category { get; set; }
        public string? ImagePath { get; set; }

        public string? OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        [ValidateNever]
        public ApplicationUser? Owner { get; set; }

        // ✅ NEW: listing moderation
        public string ApprovalStatus { get; set; } = "Approved"; // Pending, Approved, Rejected
        public string? ItemRejectionReason { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}