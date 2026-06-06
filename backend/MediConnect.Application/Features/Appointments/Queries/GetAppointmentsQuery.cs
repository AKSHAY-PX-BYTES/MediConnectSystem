using MediatR;
using MediConnect.Application.Common.Interfaces;
using MediConnect.Application.Common.Models;
using MediConnect.Application.Features.Appointments.Dtos;
using MediConnect.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Application.Features.Appointments.Queries;

public record GetAppointmentsQuery(
    Guid? DoctorId = null,
    Guid? PatientId = null,
    AppointmentStatus? Status = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<AppointmentDto>>;

public class GetAppointmentsQueryHandler
    : IRequestHandler<GetAppointmentsQuery, PagedResult<AppointmentDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAppointmentsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResult<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken ct)
    {
        // Tenant isolation is enforced automatically by the global query filter.
        var query = _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Department)
            .AsNoTracking()
            .AsQueryable();

        if (request.DoctorId is not null) query = query.Where(a => a.DoctorId == request.DoctorId);
        if (request.PatientId is not null) query = query.Where(a => a.PatientId == request.PatientId);
        if (request.Status is not null) query = query.Where(a => a.Status == request.Status);
        if (request.FromDate is not null) query = query.Where(a => a.AppointmentDate >= request.FromDate);
        if (request.ToDate is not null) query = query.Where(a => a.AppointmentDate <= request.ToDate);

        var total = await query.CountAsync(ct);

        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(a => new AppointmentDto(
                a.Id, a.PatientId, a.Patient!.FirstName + " " + a.Patient.LastName,
                a.DoctorId, a.Doctor!.FirstName + " " + a.Doctor.LastName,
                a.DepartmentId, a.Department != null ? a.Department.Name : null,
                a.Type, a.Status, a.AppointmentDate, a.StartTime, a.EndTime,
                a.Symptoms, a.Notes, a.QueueNumber, a.EstimatedWaitMinutes, a.CreatedAtUtc))
            .ToListAsync(ct);

        return new PagedResult<AppointmentDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = size
        };
    }
}
