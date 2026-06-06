using MediConnect.Application.Common.Models;
using MediConnect.Application.Features.Appointments.Commands;
using MediConnect.Application.Features.Appointments.Dtos;
using MediConnect.Application.Features.Appointments.Queries;
using MediConnect.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediConnect.WebApi.Controllers;

[Authorize]
public class AppointmentsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AppointmentDto>>> Get(
        [FromQuery] Guid? doctorId,
        [FromQuery] Guid? patientId,
        [FromQuery] AppointmentStatus? status,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await Mediator.Send(new GetAppointmentsQuery(
            doctorId, patientId, status, fromDate, toDate, page, pageSize)));

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Book(BookAppointmentCommand command)
        => Ok(await Mediator.Send(command));

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "ClinicAdmin,Doctor,Receptionist")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateAppointmentStatusRequest request)
    {
        await Mediator.Send(new UpdateAppointmentStatusCommand(
            id, request.NewStatus, request.Reason, request.NewDate, request.NewStartTime));
        return NoContent();
    }
}

public record UpdateAppointmentStatusRequest(
    AppointmentStatus NewStatus,
    string? Reason,
    DateOnly? NewDate,
    TimeOnly? NewStartTime);
