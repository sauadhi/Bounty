namespace RowdyHacks25.Models
{
    public class BountyRecord
    {
        private static readonly List<Bounty> _bounties = new()
        {
            new Bounty{ Id =1, TargetName = "Monster", Planet = "MonsterPlanet", Reward = 1000, DangerLevel = "High"}

        };

        public static List<Bounty> GetAll()
        {

            return _bounties;

        }

        public static void Add(Bounty bounty)
        {
            bounty.Id = _bounties.Count > 0 ? _bounties[^1].Id + 1 : 1;
            _bounties.Add(bounty);

        }

        public static Bounty? GetById(int id)
        {
            return _bounties.FirstOrDefault(b => b.Id == id);
        }
        public static void Remove(int id)
        {
            var bounty = GetById(id);
            if(bounty != null)
            {
                _bounties.Remove(bounty);
            }
        }

    }
}
