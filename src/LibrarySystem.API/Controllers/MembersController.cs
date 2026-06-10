using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Contracts;
using LibrarySystem.Contracts.Requests.Member;
using LibrarySystem.Contracts.Responses.Member;
using LibrarySystem.Services.Implementations;
using LibrarySystem.Services.Exceptions;

namespace LibrarySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController(MemberService memberService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemberResponseDto>>>> GetAll()
    {
        var members = await memberService.GetAllMembersAsync();
        return Ok(ApiResponse<IReadOnlyList<MemberResponseDto>>.SuccessResult(members));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MemberResponseDto>>> GetById(int id)
    {
        try
        {
            var member = await memberService.GetMemberByIdAsync(id);
            return Ok(ApiResponse<MemberResponseDto>.SuccessResult(member));
        }
        catch (LibraryException ex)
        {
            return StatusCode(ex.HttpStatusCode, new { code = ex.ErrorCode.GetCode(), message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MemberResponseDto>>> Create([FromBody] CreateMemberDto dto)
    {
        try
        {
            var createdMember = await memberService.CreateMemberAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdMember.Id }, 
                ApiResponse<MemberResponseDto>.SuccessResult(createdMember));
        }
        catch (LibraryException ex)
        {
            return StatusCode(ex.HttpStatusCode, new { code = ex.ErrorCode.GetCode(), message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<MemberResponseDto>>> Update(int id, [FromBody] UpdateMemberDto dto)
    {
        try
        {
            var updatedMember = await memberService.UpdateMemberAsync(id, dto);
            return Ok(ApiResponse<MemberResponseDto>.SuccessResult(updatedMember, "Member updated successfully."));
        }
        catch (LibraryException ex)
        {
            return StatusCode(ex.HttpStatusCode, new { code = ex.ErrorCode.GetCode(), message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await memberService.DeleteMemberAsync(id);
            return Ok(ApiResponse<object>.SuccessResult(null!, "Member deleted successfully."));
        }
        catch (LibraryException ex)
        {
            return StatusCode(ex.HttpStatusCode, new { code = ex.ErrorCode.GetCode(), message = ex.Message });
        }
    }
}
