using System.Threading.Tasks; // provides task and async support
using Avalonia.Controls; // contains Avalonia Window and UI controls
using Avalonia.Interactivity; // Provides RoutedEventArgs for UI events

namespace DoorPanels;

public partial class LoginWindow : Window // defines a window class for login functionality
{
    private AppDbContext _db; // database context for persistent storage
    private AccountService _accountService; // handles account validation and creation

    public LoginWindow() // constructor runs when window is created
    {
        InitializeComponent(); // loads XAML UI components
        InitializeServices(); // Sets up database and account services
        Loaded += OnLoaded; // Registers event for when window finishes loading
    }

    private void InitializeServices() // method to configure backend services
    {
        _db?.Dispose(); // dispose old context if it exists
        _db = new AppDbContext(); // create new database context instance
        _accountService = new AccountService(_db, new PasswordHasher()); // create service for login + hashing
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e) // event runs after UI is loaded
    {
        await EnsureDatabaseCreatedWithExampleDataAsync(); // ensure DB exists and seed sample accounts
    }

    private async Task EnsureDatabaseCreatedWithExampleDataAsync() // creates DB on first run if missing
    {
        var created = await _db.Database.EnsureCreatedAsync(); // returns true if database is newly created
        if (created) // only seed sample data on first creation
        {
            await _accountService.NewAccountAsync("admin", "admin", true); // adds a default admin account
            await _accountService.NewAccountAsync("user", "user"); // adds regular user account
        }
    }

    private async void LoginButton_Click(object? sender, RoutedEventArgs e) // event triggered by login button
    {
        var username = UsernameBox.Text ?? string.Empty; // read username input (fallback to empty)
        var password = PasswordBox.Text ?? string.Empty; // read password input (fallback to empty)

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) // check empty input
        {
            LoginStatusText.Text = "Please enter username and password."; // notify user
            return; // stop processing
        }

        if (!await _accountService.UsernameExistsAsync(username)) // check if username exists in DB
        {
            LoginStatusText.Text = "Username does not exist."; // notify missing user
            return;
        }

        if (!await _accountService.CredentialsCorrectAsync(username, password)) // validate password
        {
            LoginStatusText.Text = "Incorrect password."; // notify incorrect credentials
            return;
        }

        // Optional: fetch account and check isAdmin if needed.
        var account = await _accountService.GetAccountAsync(username);

        // Login OK → open robot GUI.
        var robotWindow = new MainWindow(username);
        robotWindow.Show();

        // Close login window.
        Close();
    }
}
