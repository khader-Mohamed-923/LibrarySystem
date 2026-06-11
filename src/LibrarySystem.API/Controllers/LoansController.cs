using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Contracts;
using LibrarySystem.Contracts.Requests.Loan;
using LibrarySystem.Contracts.Responses.Loan;
using LibrarySystem.Services.Interfaces;

namespace LibrarySystem.API.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LoanResponse>>> LoanBook([FromBody] LoanBookRequest request)
    {
        var response = await _loanService.LoanBookAsync(request);
        return CreatedAtAction(nameof(LoanBook), new { id = response.Id },
            ApiResponse<LoanResponse>.SuccessResult(response, "Book loaned successfully."));
    }

    [HttpPut("{id:int}/return")]
    public async Task<ActionResult<ApiResponse<LoanResponse>>> ReturnBook(int id)
    {
        var response = await _loanService.ReturnBookAsync(id);
        return Ok(ApiResponse<LoanResponse>.SuccessResult(response, "Book returned successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<LoanResponse>>>> GetLoans([FromQuery] int memberId)
    {
        if (memberId <= 0)
        {
            return BadRequest(ApiResponse<IEnumerable<LoanResponse>>.FailureResult("memberId must be provided and greater than 0."));
        }

        var response = await _loanService.GetLoansByMemberAsync(memberId);
        return Ok(ApiResponse<IEnumerable<LoanResponse>>.SuccessResult(response, "Open loans fetched successfully."));
    }
}
