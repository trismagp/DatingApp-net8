using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using API.Interfaces;
using System.Security.Claims;
using API.Extensions;
using API.Entities;
using SQLitePCL;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IMapper mapper,IPhotoService photoService) : BaseApiController
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
       var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

        mapper.Map(memberUpdateDto, user);
        
        if (await userRepository.SaveAllAsync()) return NoContent();
        return BadRequest("Failed to update the user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return BadRequest("Cannot update user");

        var result = await photoService.AddPhotoAsync(file);
        // if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            //  Url = result.SecureUrl.AbsoluteUri,
            //  PublicId = result.PublicId

            Url = "https://randomuser.me/api/portraits/women/15.jpg",
            PublicId = "21321321322sd"
        };

        user.Photos.Add(photo);

        if (await userRepository.SaveAllAsync()) 
            return CreatedAtAction(nameof(GetUser), 
            new {username = user.UserName}, mapper.Map<PhotoDto>(photo));

        return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return BadRequest("Could not find user");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null || photo.IsMain) return BadRequest("Cannot use this as main photo");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;
        photo.IsMain = true;

        if (await userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Problem setting main photo");
    }


}
