namespace RowdyHacks25.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; }= string.Empty; // Will hash later
        //Each user can post their own bounties.
        public List<Bounty> PostedBounties { get; private set; } = new() { };
    }
}
