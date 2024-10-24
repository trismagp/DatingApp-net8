
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using API.Interfaces;
using System.Security.Claims;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await userRepository.GetMembersAsync();
        return Ok(users);
    }

    
    [HttpGet("{username}")]  // /api/users/2
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await userRepository.GetMemberAsync(username);
        if (user == null) return NotFound(); 
        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {

        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if(username == null ) return BadRequest("no username found in token");
        var user = await userRepository.GetUserByUsernameAsync(username);

        // var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        // if (user == null) return BadRequest("Could not find user");

        mapper.Map(memberUpdateDto, user);
        
        // if (await unitOfWork.Complete()) return NoContent();

        if (await userRepository.SaveAllAsync()) return NoContent();
        return BadRequest("Failed to update the user");
    }
}
