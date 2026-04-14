using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<Patient> Patients { get; }

    DbSet<Doctor> Doctors { get; }

    DbSet<Department> Departments { get; }

    DbSet<Appointment> Appointments { get; }

    DbSet<Examination> Examinations { get; }

    DbSet<Prescription> Prescriptions { get; }

    DbSet<LabRequest> LabRequests { get; }

    DbSet<LabRequestItem> LabRequestItems { get; }

    DbSet<LabResult> LabResults { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
