using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace RowdyHacks25.Data
{
    public class JsonStore
    {
        public List<Models.Bounty> Bounties { get; set; } = new();
        public List<Models.User> Users { get; set; } = new();
    }

    /// Minimal thread-safe JSON file store with atomic writes.
    public sealed class JsonFileDatabase
    {
        private readonly string _dbPath;
        private readonly JsonSerializerOptions _opts;
        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
        private JsonStore _store = new();

        public JsonFileDatabase(string dbPath)
        {
            _dbPath = dbPath;
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);

            _opts = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            LoadFromDiskOrSeed();
        }

        public List<Models.Bounty> Bounties
        {
            get { _lock.EnterReadLock(); try { return _store.Bounties.ToList(); } finally { _lock.ExitReadLock(); } }
        }

        public List<Models.User> Users
        {
            get { _lock.EnterReadLock(); try { return _store.Users.ToList(); } finally { _lock.ExitReadLock(); } }
        }

        public Models.Bounty? GetBountyById(int id)
        {
            _lock.EnterReadLock();
            try { return _store.Bounties.FirstOrDefault(b => b.Id == id); }
            finally { _lock.ExitReadLock(); }
        }

        public Models.User? GetUserById(int id)
        {
            _lock.EnterReadLock();
            try { return _store.Users.FirstOrDefault(u => u.Id == id); }
            finally { _lock.ExitReadLock(); }
        }

        public void UpsertBounty(Models.Bounty bounty)
        {
            _lock.EnterWriteLock();
            try
            {
                if (bounty.Id == 0)
                {
                    bounty.Id = _store.Bounties.Count == 0 ? 1 : _store.Bounties.Max(b => b.Id) + 1;
                    _store.Bounties.Add(bounty);
                }
                else
                {
                    var idx = _store.Bounties.FindIndex(b => b.Id == bounty.Id);
                    if (idx >= 0) _store.Bounties[idx] = bounty;
                    else _store.Bounties.Add(bounty);
                }
                SaveToDisk();
            }
            finally { _lock.ExitWriteLock(); }
        }

        public bool RemoveBounty(int id)
        {
            _lock.EnterWriteLock();
            try
            {
                var removed = _store.Bounties.RemoveAll(b => b.Id == id) > 0;
                if (removed) SaveToDisk();
                return removed;
            }
            finally { _lock.ExitWriteLock(); }
        }

        public void UpsertUser(Models.User user)
        {
            _lock.EnterWriteLock();
            try
            {
                if (user.Id == 0)
                {
                    user.Id = _store.Users.Count == 0 ? 1 : _store.Users.Max(u => u.Id) + 1;
                    _store.Users.Add(user);
                }
                else
                {
                    var idx = _store.Users.FindIndex(u => u.Id == user.Id);
                    if (idx >= 0) _store.Users[idx] = user;
                    else _store.Users.Add(user);
                }
                SaveToDisk();
            }
            finally { _lock.ExitWriteLock(); }
        }

        public bool RemoveUser(int id)
        {
            _lock.EnterWriteLock();
            try
            {
                var removed = _store.Users.RemoveAll(u => u.Id == id) > 0;
                if (removed) SaveToDisk();
                return removed;
            }
            finally { _lock.ExitWriteLock(); }
        }

        private void LoadFromDiskOrSeed()
        {
            _lock.EnterWriteLock();
            try
            {
                if (File.Exists(_dbPath))
                {
                    var json = File.ReadAllText(_dbPath);
                    _store = JsonSerializer.Deserialize<JsonStore>(json, _opts) ?? new JsonStore();
                }
                else
                {
                    _store = new JsonStore
                    {
                        Bounties = new List<Models.Bounty>
                        {
                            new()
                            {
                                Id = 1,
                                TargetName = "Monster",
                                Planet = "MonsterPlanet",
                                Reward = 1000,
                                DangerLevel = "High",
                                Status = "Open",
                                PostedBy = "Admin",
                                ClaimedBy = null,
                                ImageUrl = "",
                                Bio = "Ancient scourge of MonsterPlanet. Suspected in multiple plasma heists.\nSeen last near the sulfur dunes."
                            }
                        },
                        Users = new List<Models.User>()
                    };
                    SaveToDisk();
                }
            }
            finally { _lock.ExitWriteLock(); }
        }

        private void SaveToDisk()
        {
            var tmp = _dbPath + ".tmp";
            var json = JsonSerializer.Serialize(_store, _opts);
            File.WriteAllText(tmp, json);
            File.Copy(tmp, _dbPath, overwrite: true);
            File.Delete(tmp);
        }
    }
}
