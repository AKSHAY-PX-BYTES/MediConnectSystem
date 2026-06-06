using MediConnect.Application.Features.Auth.Commands;
using MediConnect.Application.Features.Auth.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediConnect.WebApi.Controllers;

[AllowAnonymous]
public class AuthController : BaseApiController
{
    /// <summary>Self-service clinic sign-up. Creates a tenant + first Clinic Admin.</summary>
    [HttpPost("register-clinic")]
    public async Task<ActionResult<AuthResponseDto>> RegisterClinic(RegisterClinicCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenCommand command)
        => Ok(await Mediator.Send(command));
}
