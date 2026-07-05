using System;
using System.Security.Cryptography;
using System.Text;

var passwordHash = "ezbsYCnaHQFB3i2hTxLMyriAWmWFpfljIiYjz6bTjInYp/tbJd+5yX6UYEpHBDIoDPl6PZQSKFd+0iN5LCmipA==";
var passwordSalt = "ogj9QceE0qO+BFbltp3UHXSIDc56ZyL+YGuDXWIrMISPmhjiqrkE6SKdqgGXTGQLl2jVfLAmILxIlhGbesgl1F1Og7dVJ1RjjIVrmdWSey8/c39agLKPJ/UGIEYliPs+fSCD3NS3OyATO/rB6EVNwOzkUyWnTzgmKhUxR/CnN2E=";

bool VerifyPassword(string password, string storedHash, string storedSalt)
{
    var saltBytes = Convert.FromBase64String(storedSalt);
    using var hmac = new HMACSHA512(saltBytes);
    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    var computedHashString = Convert.ToBase64String(computedHash);
    return computedHashString == storedHash;
}

string[] candidates = {
    "Admin@123",
    "admin123",
    "Admin123!",
    "Admin@1234",
    "password",
    "admin",
    "Aisam@123",
    "Aisam@2024",
    "Admin@2024",
    "admin@aisam.com",
    "SuperAdmin@123",
    "Admin#123",
    "AisamAdmin@123",
    "123456",
    "admin@123",
    "Admin123",
    "Admin1",
    "AisamAdmin1",
    "Admin@aisam123",
};

foreach (var pwd in candidates)
{
    if (VerifyPassword(pwd, passwordHash, passwordSalt))
    {
        Console.WriteLine($"MATCH: Password is '{pwd}'");
        break;
    }
}

Console.WriteLine("Verification complete.");
