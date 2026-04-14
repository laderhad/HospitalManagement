using System.Reflection;
using HospitalManagement.Application.Common.Interfaces;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagement.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    
    public DbSet<Patient> Patients => Set<Patient>();
    
    public DbSet<Doctor> Doctors => Set<Doctor>();
    
    public DbSet<Department> Departments => Set<Department>();
    
    public DbSet<Appointment> Appointments => Set<Appointment>();
    
    public DbSet<Examination> Examinations => Set<Examination>();
    
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    
    public DbSet<LabRequest> LabRequests => Set<LabRequest>();
    
    public DbSet<LabRequestItem> LabRequestItems => Set<LabRequestItem>();
    
    public DbSet<LabResult> LabResults => Set<LabResult>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
