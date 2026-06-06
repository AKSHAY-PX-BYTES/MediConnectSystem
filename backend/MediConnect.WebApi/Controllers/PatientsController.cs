using MediConnect.Application.Features.Patients.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediConnect.WebApi.Controllers;

[Authorize(Roles = "ClinicAdmin,Doctor,Receptionist")]
public class PatientsController : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreatePatientCommand command)
        => Ok(await Mediator.Send(command));
}
