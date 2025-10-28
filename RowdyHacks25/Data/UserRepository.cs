using RowdyHacks25.Models;

namespace RowdyHacks25.Data
{
    public interface IUserRepository
    {
        List<User> GetAll();
        User? GetById(int id);
        void Add(User user);
        void Update(User user);
        void Remove(int id);
    }

    public sealed class UserRepository : IUserRepository
    {
        private readonly JsonFileDatabase _db;
        public UserRepository(JsonFileDatabase db) => _db = db;

        public List<User> GetAll() => _db.Users;
        public User? GetById(int id) => _db.GetUserById(id);
        public void Add(User user) => _db.UpsertUser(user);

        public void Update(User user)
        {
            if (user.Id == 0) throw new ArgumentException("User.Id required for update.");
            _db.UpsertUser(user);
        }

        public void Remove(int id) => _db.RemoveUser(id);
    }
}