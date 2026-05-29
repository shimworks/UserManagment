using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Users.Commands;
using UserManagement.Application.Users.DTOs;
using UserManagement.Application.Users.Queries;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetUserByIdQuery(id), ct));

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await _mediator.Send(new ListUsersQuery(page, pageSize), ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Ok(await _mediator.Send(new UpdateUserCommand(id, request.Name, request.Email), ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteUserCommand(id), ct);
        return NoContent();
    }
}