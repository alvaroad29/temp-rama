// Controllers/ReadingHistoryController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.Repositories;
using backend.Models.Dtos;
using backend.Repository.IRepository;
using AutoMapper;
using backend.Models.Dtos.BookQueries;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/history")]
    [Authorize]
    public class ReadingHistoryController : ControllerBase
    {
        private readonly IReadingHistoryRepository _repository;
         private readonly IMapper _mapper;
        public ReadingHistoryController(
        IReadingHistoryRepository repository,
        IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyHistory([FromQuery] HistoryQueryParameters parameters)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var pagedHistory = await _repository.GetHistoryByUserIdAsync(userId, parameters);

            var dto = _mapper.Map<List<ReadingHistoryDto>>(pagedHistory.Data);

            var response = new PagedResponse<ReadingHistoryDto>(
                dto,
                pagedHistory.PageNumber,
                pagedHistory.PageSize,
                pagedHistory.TotalRecords);

            return Ok(response);
        }
    }
}