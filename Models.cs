using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DoorPanels;

public class AccountService(AppDbContext db, PasswordHasher hasher) // service class handling account logic, uses primary constructor for Dependency Injection
{
    public async Task NewAccountAsync(string username, string password, bool isAdmin = false) // creates a new user account asynchronously
    {
        var (salt, saltedPasswordHash) = hasher.Hash(password); // generate salt and salted hash from the plain password
        db.Add(new Account // add a new account entity
        {
            Username = username, // set username for the new account
            Salt = salt, // store generated salt
            SaltedPasswordHash = saltedPasswordHash, // store generated salted password hash
            isAdmin = isAdmin // store admin flag (true = admin, false = normal user)
        });
        await db.SaveChangesAsync(); // persist changes to the database asynchronously 
    }

    public Task<bool> UsernameExistsAsync(string username) // checks whether a username already exists in the database
    {
        return db.Accounts.AnyAsync(a => a.Username == username); // returns true if any account has the given username
    }

    public async Task<bool> CredentialsCorrectAsync(string username, string password) // validates username + password combination
    {
        var account = await db.Accounts.FirstAsync(a => a.Username == username); // retrieve account for given username
        return hasher.PasswordCorrect(password, account.Salt, account.SaltedPasswordHash); // verify password against stored salt + hash
    }

    public Task<bool> UserIsAdminAsync(string username) // checks if the given user is an admin
    {
        return db.Accounts.Where(a => a.Username == username).Select(a => a.isAdmin).FirstAsync(); // start query on accounts table
    }

    public Task<Account> GetAccountAsync(string username) // gets full account object for a given username
    {
        return db.Accounts.FirstAsync(a => a.Username == username); // return the first account with matching username
    }
}

public class PasswordHasher( // class responsible for hashing and verifying passwords
    int saltLength = 128 / 8, // length of the random salt in bytes
    int hashIterations = 600_000 // work factor for hashing
)
{
    public bool PasswordCorrect(string password, byte[] salt, byte[] saltedPasswordHash) // verifies if provided password matches stored hash
    {
        return CryptographicOperations.FixedTimeEquals(Hash(salt, password), saltedPasswordHash); // compare in constant time to avoid timing attacks
    }

    private byte[] Hash(byte[] salt, string password) // internal helper that derives a hash from salt + password
    {
        return Rfc2898DeriveBytes.Pbkdf2( // key derivation
            password,
            salt,
            hashIterations,
            HashAlgorithmName.SHA256,
            256 / 8
        );
    }

    public (byte[] Salt, byte[] Hash) Hash(string password) // creates a new salt and hash pair from a plain password
    {
        var salt = RandomNumberGenerator.GetBytes(saltLength); // generate cryptographcally secure random salt
        return (salt, Hash(salt, password)); // return tuple containing salt and resulting hash
    }
}

public class AppDbContext(string dbPath = "database.sqlite") : DbContext // EF Core Dbcontext for the application, using primary constructor for DB path
{
    public DbSet<Account> Accounts { get; set; } // DbSet representing the accounts table in the database
    public DbSet<OrderLog> OrderLogs { get; set; } // DbSet representing the order time table in the database

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) // configures the DbContext options
    {
        optionsBuilder.UseSqlite($"Data Source={dbPath}"); // configure EF core to use SQLite with the given database file path
    }
}

public class Account // entity model representing a user account in the database
{
    [Key] public string Username { get; set; } // primary key for the table, using username as unique identifier

    public byte[] Salt { get; set; } // random salt used for hashing this user's password
    public byte[] SaltedPasswordHash { get; set; } // hashed password derived from salt + password 
    public bool isAdmin { get; set; } // indicates whether the account belongs to an admin user
}

public class OrderLog
{
    [Key] public int Id { get; set; } // primary key

    public string Username { get; set; } // operator who handled the order
    public string DoorSize { get; set; } // Small / Medium / Large

    public DateTime StartedAt { get; set; } // when brake was pressed
    public DateTime? FinishedAt { get; set; } // null until operator finishes
}

public static class DatabaseSeeder // helper class used to seed the database with initial accounts
{
    public static async Task AddProjectAccountsAsync() // adds predefined project accounts if they do not exist
    {
        using var db = new AppDbContext(); // create a ew DbContext instance (disposed automatically after method ends)

        // IMPORTANT: creates DB + table if missing
        await db.Database.EnsureCreatedAsync();

        var service = new AccountService(db, new PasswordHasher()); // create AccountService using current DbContext and a new PasswordHasher

        async Task EnsureUser(string username, bool isAdmin) // local helper to create a user if not already in the database
        {
            if (!await service.UsernameExistsAsync(username)) // check if user exists
            {
                // Password is same as username
                await service.NewAccountAsync(username, username, isAdmin);
            }
        }

        // Two admins:
        await EnsureUser("Laerke", true);
        await EnsureUser("Sophie", true);

        // Two normal users:
        await EnsureUser("Sofie", false);
        await EnsureUser("Ida", false);
    }
}


