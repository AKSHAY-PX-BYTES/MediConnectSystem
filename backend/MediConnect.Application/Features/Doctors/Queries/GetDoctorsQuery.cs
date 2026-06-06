using MediatR;
using MediConnect.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Application.Features.Doctors.Queries;

public record DoctorListItemDto(
    Guid Id,
    string FullName,
    string? Specialization,
    Guid? DepartmentId,
    string? DepartmentName,
    decimal ConsultationFee,
    int ExperienceYears,
    string? PhotoUrl,
    bool IsAcceptingAppointments);

public record GetDoctorsQuery(Guid? DepartmentId = null, string? Search = null)
    : IRequest<IReadOnlyList<DoctorListItemDto>>;

public class GetDoctorsQueryHandler
    : IRequestHandler<GetDoctorsQuery, IReadOnlyList<DoctorListItemDto>>
{
    private readonly IApplicationDbContext _db;

    public GetDoctorsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<DoctorListItemDto>> Handle(GetDoctorsQuery request, CancellationToken ct)
    {
        var query = _db.Doctors
            .Include(d => d.Department)
            .AsNoTracking()
            .Where(d => d.IsActive);

        if (request.DepartmentId is not null)
            query = query.Where(d => d.DepartmentId == request.DepartmentId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            query = query.Where(d =>
                d.FirstName.ToLower().Contains(s) ||
                d.LastName.ToLower().Contains(s) ||
                (d.Specialization != null && d.Specialization.ToLower().Contains(s)));
        }

        return await query
            .OrderBy(d => d.FirstName)
            .Select(d => new DoctorListItemDto(
                d.Id, d.FirstName + " " + d.LastName, d.Specialization,
                d.DepartmentId, d.Department != null ? d.Department.Name : null,
                d.ConsultationFee, d.ExperienceYears, d.PhotoUrl, d.IsAcceptingAppointments))
            .ToListAsync(ct);
    }
}
