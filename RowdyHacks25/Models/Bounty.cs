namespace RowdyHacks25.Models
{
    public class Bounty
    {
        public int Id { get; set; }
        public string TargetName { get; set; } = "";
        public string Planet { get; set; } = "";
        public int Reward { get; set; }
        public string DangerLevel { get; set; } = "";
        public string Status { get; set; } = "Open"; // Open, Claimed, Captured
        public string PostedBy { get; set; } = "";
        public string? ClaimedBy { get; set; }

        public string? ImageUrl { get; set; } = "";

        // Long-form biography/details for the target (optional)
        public string? Bio { get; set; }

        // One-time AI-generated summary of the bounty
        public string? Summary { get; set; }
        
        public string ManagerId { get; set; } = ""; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
