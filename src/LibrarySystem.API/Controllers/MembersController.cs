using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Contracts;
using LibrarySystem.Contracts.Requests.Member;
using LibrarySystem.Contracts.Responses.Member;
using LibrarySystem.Services.Services.Interfaces;
using LibrarySystem.Services.Exceptions;

namespace LibrarySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class MembersController(IMemberService memberService, IValidator<CreateMemberDto> createMemberValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MemberResponseDto>>>> GetAll()
    {
        var members = await memberService.GetAllMembersAsync();
        return Ok(ApiResponse<IReadOnlyList<MemberResponseDto>>.SuccessResult(members, "All members fetched successfully."));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MemberResponseDto>>> GetById(int id)
    {
        var member = await memberService.GetMemberByIdAsync(id);
        return Ok(ApiResponse<MemberResponseDto>.SuccessResult(member, "Member fetched successfully."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MemberResponseDto>>> Create([FromBody] CreateMemberDto dto)
    {
        var validationResult = await createMemberValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(ApiResponse<MemberResponseDto>.FailureResult("Validation failed.", errors));
        }

        var createdMember = await memberService.CreateMemberAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdMember.Id }, 
            ApiResponse<MemberResponseDto>.SuccessResult(createdMember, "Member created successfully."));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<MemberResponseDto>>> Update(int id, [FromBody] UpdateMemberDto dto)
    {
        var updatedMember = await memberService.UpdateMemberAsync(id, dto);
        return Ok(ApiResponse<MemberResponseDto>.SuccessResult(updatedMember, "Member updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        await memberService.DeleteMemberAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(null!, "Member deleted successfully."));
    }
}