using LibrarySystem.Contracts.Requests.Member;
using LibrarySystem.Contracts.Responses.Member;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Interfaces;
using LibrarySystem.Services.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Services.Implementations;

public class MemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MemberService(IMemberRepository memberRepository, IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MemberResponseDto> CreateMemberAsync(CreateMemberDto dto)
    {
        var isUnique = await _memberRepository.IsEmailUniqueAsync(dto.Email);
        if (!isUnique)
        {
            throw new DuplicateResourceException(ErrorCode.DUPLICATE_EMAIL);
        }

        var member = new Member
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            MembershipDate = DateTime.UtcNow
        };

        await _memberRepository.AddAsync(member);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(member);
    }

    public async Task<MemberResponseDto> GetMemberByIdAsync(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member is null)
        {
            throw new ResourceNotFoundException(ErrorCode.MEMBER_NOT_FOUND);
        }

        return MapToDto(member);
    }

    public async Task<IReadOnlyList<MemberResponseDto>> GetAllMembersAsync()
    {
        var members = await _memberRepository.GetAllAsync();
        return members.Select(MapToDto).ToList();
    }

    public async Task<MemberResponseDto> UpdateMemberAsync(int id, UpdateMemberDto dto)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member is null)
        {
            throw new ResourceNotFoundException(ErrorCode.MEMBER_NOT_FOUND);
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var isUnique = await _memberRepository.IsEmailUniqueAsync(dto.Email, id);
            if (!isUnique)
            {
                throw new DuplicateResourceException(ErrorCode.DUPLICATE_EMAIL);
            }
            member.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.FirstName))
        {
            member.FirstName = dto.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(dto.LastName))
        {
            member.LastName = dto.LastName;
        }

        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            member.Phone = dto.Phone;
        }

        _memberRepository.Update(member, dto.RowVersion);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException(ErrorCode.CONCURRENCY_CONFLICT);
        }
        catch (DbUpdateException)
        {
            throw;
        }

        return MapToDto(member);
    }

    public async Task DeleteMemberAsync(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member is null)
        {
            throw new ResourceNotFoundException(ErrorCode.MEMBER_NOT_FOUND);
        }

        _memberRepository.Delete(member);
        await _unitOfWork.SaveChangesAsync();
    }

    private static MemberResponseDto MapToDto(Member member)
    {
        return new MemberResponseDto
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Email = member.Email,
            Phone = member.Phone,
            MembershipDate = member.MembershipDate,
            RowVersion = member.RowVersion
        };
    }
}
