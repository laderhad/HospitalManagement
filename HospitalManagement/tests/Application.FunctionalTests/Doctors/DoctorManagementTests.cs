using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Departments.Commands.CreateDepartment;
using HospitalManagement.Application.Doctors.Commands.CreateDoctor;
using HospitalManagement.Application.Doctors.Commands.SetDoctorActiveState;
using HospitalManagement.Application.Doctors.Commands.UpdateDoctor;
using HospitalManagement.Application.Doctors.Queries.GetDoctors;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.FunctionalTests.Doctors;

public class DoctorManagementTests : TestBase
{
    [Test]
    public async Task ShouldCreateDoctorAndAssignDoctorRole()
    {
        await TestApp.RunAsAdministratorAsync();

        var departmentId = await TestApp.SendAsync(new CreateDepartmentCommand
        {
            Name = "Cardiology"
        });

        var doctorId = await TestApp.SendAsync(new CreateDoctorCommand
        {
            Email = "doctor.new@local",
            Password = "Doctor1234!",
            FirstName = "Demo",
            LastName = "Doctor",
            DepartmentId = departmentId,
            ContactNumber = "555-0199",
            Specialty = "Cardiology",
            Title = "Specialist"
        });

        var doctor = await TestApp.FindAsync<Doctor>(doctorId);

        doctor.ShouldNotBeNull();
        doctor!.DepartmentId.ShouldBe(departmentId);
        doctor.ApplicationUserId.ShouldNotBeNullOrWhiteSpace();

        var isDoctorRoleAssigned = await TestApp.IsInRoleAsync(doctor.ApplicationUserId, Roles.Doctor);
        isDoctorRoleAssigned.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldRejectInvalidDepartmentId()
    {
        await TestApp.RunAsAdministratorAsync();

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new CreateDoctorCommand
        {
            Email = "doctor.invalid.department@local",
            Password = "Doctor1234!",
            FirstName = "Demo",
            LastName = "Doctor",
            DepartmentId = Guid.NewGuid(),
            ContactNumber = "555-0100"
        }));
    }

    [Test]
    public async Task ShouldRejectDuplicateDoctorEmail()
    {
        await TestApp.RunAsAdministratorAsync();

        var departmentId = await TestApp.SendAsync(new CreateDepartmentCommand
        {
            Name = "Neurology"
        });

        await TestApp.SendAsync(new CreateDoctorCommand
        {
            Email = "doctor.duplicate@local",
            Password = "Doctor1234!",
            FirstName = "Demo",
            LastName = "Doctor",
            DepartmentId = departmentId,
            ContactNumber = "555-0101"
        });

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new CreateDoctorCommand
        {
            Email = "doctor.duplicate@local",
            Password = "Doctor1234!",
            FirstName = "Another",
            LastName = "Doctor",
            DepartmentId = departmentId,
            ContactNumber = "555-0102"
        }));
    }

    [Test]
    public async Task ShouldUpdateDoctorProfile()
    {
        await TestApp.RunAsAdministratorAsync();

        var departmentId = await TestApp.SendAsync(new CreateDepartmentCommand
        {
            Name = "Emergency"
        });

        var updatedDepartmentId = await TestApp.SendAsync(new CreateDepartmentCommand
        {
            Name = "Oncology"
        });

        var doctorId = await TestApp.SendAsync(new CreateDoctorCommand
        {
            Email = "doctor.update@local",
            Password = "Doctor1234!",
            FirstName = "Initial",
            LastName = "Doctor",
            DepartmentId = departmentId,
            ContactNumber = "555-0103",
            Specialty = "Emergency",
            Title = "Resident"
        });

        await TestApp.SendAsync(new UpdateDoctorCommand
        {
            Id = doctorId,
            FirstName = "Updated",
            LastName = "Doctor",
            DepartmentId = updatedDepartmentId,
            ContactNumber = "555-0104",
            Specialty = "Oncology",
            Title = "Specialist"
        });

        var doctor = await TestApp.FindAsync<Doctor>(doctorId);

        doctor.ShouldNotBeNull();
        doctor!.FirstName.ShouldBe("Updated");
        doctor.DepartmentId.ShouldBe(updatedDepartmentId);
        doctor.ContactNumber.Value.ShouldBe("555-0104");
        doctor.Specialty.ShouldBe("Oncology");
        doctor.Title.ShouldBe("Specialist");
    }

    [Test]
    public async Task ShouldSetDoctorActiveState()
    {
        await TestApp.RunAsAdministratorAsync();

        var departmentId = await TestApp.SendAsync(new CreateDepartmentCommand
        {
            Name = "Pediatrics"
        });

        var doctorId = await TestApp.SendAsync(new CreateDoctorCommand
        {
            Email = "doctor.active@local",
            Password = "Doctor1234!",
            FirstName = "Active",
            LastName = "Doctor",
            DepartmentId = departmentId,
            ContactNumber = "555-0105"
        });

        await TestApp.SendAsync(new SetDoctorActiveStateCommand
        {
            Id = doctorId,
            IsActive = false
        });

        var doctor = await TestApp.FindAsync<Doctor>(doctorId);

        doctor.ShouldNotBeNull();
        doctor!.IsActive.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldAllowOnlyAdministratorToManageDoctors()
    {
        await TestApp.RunAsUserAsync("doctor-management@local", "Doctor1234!", [Roles.Doctor]);

        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GetDoctorsQuery()));

        await TestApp.RunAsUserAsync("patient-management@local", "Patient1234!", [Roles.Patient]);

        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GetDoctorsQuery()));
    }
}