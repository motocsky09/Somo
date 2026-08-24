namespace Somo.Application.Interfaces;

public record UserContact(
    string Id,
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? ProfilePhotoUrl)
{
    /// <summary>
    /// Numele complet, sau username-ul când persoana nu și-a completat profilul.
    /// </summary>
    public string FullName
    {
        get
        {
            var name = string.Join(' ', new[] { FirstName, LastName }
                .Where(n => !string.IsNullOrWhiteSpace(n)));
            return string.IsNullOrWhiteSpace(name) ? Username : name;
        }
    }
}

/// <summary>
/// Acces la datele de contact ale conturilor. Identity trăiește în stratul API,
/// așa că Application îl vede doar prin această interfață.
/// </summary>
public interface IUserDirectory
{
    Task<UserContact?> GetContactAsync(string userId);
}
