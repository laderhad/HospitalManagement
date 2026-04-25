using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Departments.Commands.CreateDepartment;
using HospitalManagement.Application.Departments.Commands.SetDepartmentActiveState;
using HospitalManagement.Application.Departments.Commands.UpdateDepartment;
using HospitalManagement.Application.Departments.Queries.GetDepartments;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;

namespace HospitalManagement.Application.FunctionalTests.Departments;

public class DepartmentManagementTests : TestBase
{
    [Test]
    public async Task ShouldCreateDepartmentAsAdministrator()
    {
        await TestApp.RunAsAdministratorAsync();

        var id = await TestApp.SendAsync(new CreateDepartmentCommand
        {
            Name = "Radiology"
        });

        var department = await TestApp.FindAsync<Department>(id);

        department.ShouldNotBeNull();
        department!.Name.ShouldBe("Radiology");
        department.IsActive.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldUpdateDepartmentName()
    {
        await TestApp.RunAsAdministratorAsync();

        var id = await TestApp.SendAsync(new CreateDepartmentCommand
        {
            Name = "Radiology"
        });

        await TestApp.SendAsync(new UpdateDepartmentCommand
        {
            Id = id,
            Name = "Advanced Radiology"
        });

        var department = await TestApp.FindAsync<Department>(id);

        department.ShouldNotBeNull();
        department!.Name.ShouldBe("Advanced Radiology");
    }

    [Test]
    public async Task ShouldSetDepartmentActiveState()
    {
        await TestApp.RunAsAdministratorAsync();

        var id = await TestApp.SendAsync(new CreateDepartmentCommand
        {
            Name = "Laboratory"
        });

        await TestApp.SendAsync(new SetDepartmentActiveStateCommand
        {
            Id = id,
            IsActive = false
        });

        var department = await TestApp.FindAsync<Department>(id);

        department.ShouldNotBeNull();
        department!.IsActive.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldRejectDuplicateDepartmentNames()
    {
        await TestApp.RunAsAdministratorAsync();

        await TestApp.SendAsync(new CreateDepartmentCommand
        {
            Name = "Cardiology"
        });

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new CreateDepartmentCommand
        {
            Name = " cardiology "
        }));
    }

    [Test]
    public async Task ShouldAllowOnlyAdministratorToManageDepartments()
    {
        await TestApp.RunAsUserAsync("dept-doctor@local", "Doctor1234!", [Roles.Doctor]);

        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GetDepartmentsQuery()));

        await TestApp.RunAsUserAsync("dept-patient@local", "Patient1234!", [Roles.Patient]);

        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GetDepartmentsQuery()));
    }
}