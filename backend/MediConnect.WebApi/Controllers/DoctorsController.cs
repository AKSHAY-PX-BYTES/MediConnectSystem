using MediConnect.Application.Features.Doctors.Commands;
using MediConnect.Application.Features.Doctors.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediConnect.WebApi.Controllers;

[Authorize]
public class DoctorsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DoctorListItemDto>>> Get(
        [FromQuery] Guid? departmentId, [FromQuery] string? search)
        => Ok(await Mediator.Send(new GetDoctorsQuery(departmentId, search)));

    [HttpPost]
    [Authorize(Roles = "ClinicAdmin")]
    public async Task<ActionResult<Guid>> Create(CreateDoctorCommand command)
        => Ok(await Mediator.Send(command));
}
