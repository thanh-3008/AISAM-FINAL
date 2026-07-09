using System;
using System.Security.Cryptography;
using System.Text;

var passwordHash = "UerdQ3VtiYZ4QTm6hRz+eOmid9LnWaURY30Rxe7vVwDQT07ZVvPYFNfFc86F00bEMnxuaZ6wO9hNxLuiLWvVag==";
var passwordSalt = "0YzN6SLaBxlvEmaum9P7ct2gISTgBFv+Iyc8zutGzQKn0lbvJi9D0oH39mwVloTQ0R94qhCKVaarTgAz302y0rlUGrc3A1Q//Q2VEsbQ8I1//pbbWClzhaNQ5rO9bes/uJJ/zX66xrlGfTPaAJJFZByiSXnj5x6XVBA4heUJkJY=";

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

