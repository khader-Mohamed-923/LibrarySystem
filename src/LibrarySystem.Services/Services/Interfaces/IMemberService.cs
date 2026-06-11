using LibrarySystem.Contracts.Requests.Member;
using LibrarySystem.Contracts.Responses.Member;

namespace LibrarySystem.Services.Services.Interfaces;

public interface IMemberService
{
    Task<MemberResponseDto> CreateMemberAsync(CreateMemberDto dto);
    Task<MemberResponseDto> GetMemberByIdAsync(int id);
    Task<IReadOnlyList<MemberResponseDto>> GetAllMembersAsync();
    Task<MemberResponseDto> UpdateMemberAsync(int id, UpdateMemberDto dto);
    Task DeleteMemberAsync(int id);
}