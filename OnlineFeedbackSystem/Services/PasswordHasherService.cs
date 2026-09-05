using Microsoft.AspNetCore.Identity;
using OnlineFeedbackSystem.Models.Entities;

namespace OnlineFeedbackSystem.Services
{
    // Java comparison: this plays the same role as Spring Security's
    // PasswordEncoder (e.g. BCryptPasswordEncoder). We're borrowing just
    // the hashing algorithm from ASP.NET Core Identity (PasswordHasher<T>
    // uses PBKDF2 with a random salt per password) without pulling in the
    // rest of the Identity framework — you don't need Identity's own user
    // store since you're managing Users yourself via ADO.NET.
    public class PasswordHasherService
    {
        private readonly PasswordHasher<Users> _hasher = new();

        // Call this once when creating a user, store the result in PasswordHash.
        public string HashPassword(Users user, string plainTextPassword)
        {
            return _hasher.HashPassword(user, plainTextPassword);
        }

        // Call this on login: compares the password the user typed against
        // the stored hash. Returns true/false — the plain text password is
        // never stored or compared directly, only ever hashed and checked.
        public bool VerifyPassword(Users user, string plainTextPassword)
        {
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, plainTextPassword);

            // PasswordVerificationResult has a third value, "SuccessRehashNeeded",
            // for when the hashing algorithm's parameters have been upgraded
            // since the password was last hashed. Treating it as success is
            // correct — just know that in a production system you'd also
            // re-hash and save the password here to pick up the upgrade.
            return result == PasswordVerificationResult.Success
                || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
