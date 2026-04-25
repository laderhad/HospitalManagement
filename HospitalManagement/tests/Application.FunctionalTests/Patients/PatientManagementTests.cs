using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Patients.Commands.CreatePatientByAdmin;
using HospitalManagement.Application.Patients.Commands.UpdateMyPatientProfile;
using HospitalManagement.Application.Patients.Queries.GetMyPatientProfile;
using HospitalManagement.Application.Patients.Queries.GetPatientById;
using HospitalManagement.Application.Patients.Queries.GetPatients;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.FunctionalTests.Patients;

public class PatientManagementTests : TestBase
{
    [Test]
    public async Task ShouldCreatePatientByAdminAndAssignPatientRole()
    {
        var applicationUserId = await TestApp.RunAsUserAsync("patient.create@local", "Patient1234!", []);
        await TestApp.RunAsAdministratorAsync();

        var patientId = await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = applicationUserId,
            FirstName = "Demo",
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Unknown,
            ContactNumber = "555-0201",
            Address = "Demo address"
        });

        var patient = await TestApp.FindAsync<Patient>(patientId);

        patient.ShouldNotBeNull();
        patient!.ApplicationUserId.ShouldBe(applicationUserId);

        var hasPatientRole = await TestApp.IsInRoleAsync(applicationUserId, Roles.Patient);
        hasPatientRole.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldRejectDuplicatePatientProfileForSameUser()
    {
        var applicationUserId = await TestApp.RunAsUserAsync("patient.duplicate@local", "Patient1234!", []);
        await TestApp.RunAsAdministratorAsync();

        await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = applicationUserId,
            FirstName = "First",
            LastName = "Patient",
            DateOfBirth = new DateTime(1985, 5, 5, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female,
            ContactNumber = "555-0202",
            Address = "Address 1"
        });

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = applicationUserId,
            FirstName = "Second",
            LastName = "Patient",
            DateOfBirth = new DateTime(1988, 8, 8, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            ContactNumber = "555-0203",
            Address = "Address 2"
        }));
    }

    [Test]
    public async Task ShouldAllowPatientToReadAndUpdateOnlyOwnProfile()
    {
        var patientUserOneId = await TestApp.RunAsUserAsync("patient.one@local", "Patient1234!", []);
        var patientUserTwoId = await TestApp.RunAsUserAsync("patient.two@local", "Patient1234!", []);

        await TestApp.RunAsAdministratorAsync();

        var patientOneProfileId = await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = patientUserOneId,
            FirstName = "First",
            LastName = "Patient",
            DateOfBirth = new DateTime(1992, 2, 2, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Female,
            ContactNumber = "555-0204",
            Address = "Address 1"
        });

        var patientTwoProfileId = await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = patientUserTwoId,
            FirstName = "Second",
            LastName = "Patient",
            DateOfBirth = new DateTime(1994, 4, 4, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Male,
            ContactNumber = "555-0205",
            Address = "Address 2"
        });

        await TestApp.ImpersonateAsync(patientUserOneId, [Roles.Patient]);

        var myProfile = await TestApp.SendAsync(new GetMyPatientProfileQuery());

        myProfile.Id.ShouldBe(patientOneProfileId);
        myProfile.FirstName.ShouldBe("First");

        await TestApp.SendAsync(new UpdateMyPatientProfileCommand
        {
            FirstName = "Updated",
            LastName = "Patient",
            DateOfBirth = new DateTime(1992, 2, 2, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Other,
            ContactNumber = "555-0210",
            Address = "Updated address"
        });

        var updatedOwnProfile = await TestApp.FindAsync<Patient>(patientOneProfileId);
        var otherProfile = await TestApp.FindAsync<Patient>(patientTwoProfileId);

        updatedOwnProfile.ShouldNotBeNull();
        updatedOwnProfile!.FirstName.ShouldBe("Updated");
        updatedOwnProfile.ContactNumber.Value.ShouldBe("555-0210");
        updatedOwnProfile.Address.Value.ShouldBe("Updated address");

        otherProfile.ShouldNotBeNull();
        otherProfile!.FirstName.ShouldBe("Second");

        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GetPatientsQuery()));
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GetPatientByIdQuery(patientTwoProfileId)));
    }

    [Test]
    public async Task AdministratorShouldSeeAllPatients()
    {
        var patientUserOneId = await TestApp.RunAsUserAsync("patient.list.one@local", "Patient1234!", []);
        var patientUserTwoId = await TestApp.RunAsUserAsync("patient.list.two@local", "Patient1234!", []);

        await TestApp.RunAsAdministratorAsync();

        await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = patientUserOneId,
            FirstName = "First",
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 10, 10, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Unknown,
            ContactNumber = "555-0211",
            Address = "Address 1"
        });

        await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = patientUserTwoId,
            FirstName = "Second",
            LastName = "Patient",
            DateOfBirth = new DateTime(1991, 11, 11, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Unknown,
            ContactNumber = "555-0212",
            Address = "Address 2"
        });

        var patients = await TestApp.SendAsync(new GetPatientsQuery());

        patients.Count.ShouldBe(2);
    }
}