using RowdyHacks25.Models;

namespace RowdyHacks25.Data
{
    public interface IBountyRepository
    {
        List<Bounty> GetAll();
        Bounty? GetById(int id);
        void Add(Bounty bounty);
        void Update(Bounty bounty);
        void Remove(int id);
    }

    public sealed class BountyRepository : IBountyRepository
    {
        private readonly JsonFileDatabase _db;
        public BountyRepository(JsonFileDatabase db) => _db = db;

        public List<Bounty> GetAll() => _db.Bounties;
        public Bounty? GetById(int id) => _db.GetBountyById(id);
        public void Add(Bounty bounty) => _db.UpsertBounty(bounty);

        public void Update(Bounty bounty)
        {
            if (bounty.Id == 0) throw new ArgumentException("Bounty.Id required for update.");
            _db.UpsertBounty(bounty);
        }

        public void Remove(int id) => _db.RemoveBounty(id);
    }
}