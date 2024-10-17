using System;
using API.Entities;
using API.Interfaces;
using Microsoft.VisualBasic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;


namespace API.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(AppUser user)
    {
         var tokenKey = config["TokenKey"]  ?? throw new Exception("Cannot access TokenKey from appsettings.");

        if (tokenKey.Length < 64) throw new Exception("Your TokenKey needs to be at least 64 characters long.");
            
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.UserName)
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);


        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7), // Token will be valid for 7 days
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        // Create the token based on the descriptor
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);

    }
    
}
