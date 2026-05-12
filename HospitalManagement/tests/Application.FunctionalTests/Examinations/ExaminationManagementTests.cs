using HospitalManagement.Application.Appointments.Commands.CreateAppointment;
using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Departments.Commands.CreateDepartment;
using HospitalManagement.Application.Doctors.Commands.CreateDoctor;
using HospitalManagement.Application.Examinations.Commands.CreateExamination;
using HospitalManagement.Application.Examinations.Commands.UpdateExamination;
using HospitalManagement.Application.Examinations.Queries.GetMyDoctorExaminations;
using HospitalManagement.Application.Examinations.Queries.GetMyPatientExaminations;
using HospitalManagement.Application.Patients.Commands.CreatePatientByAdmin;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.FunctionalTests.Examinations;

public class ExaminationManagementTests : TestBase
{
    [Test]
    public async Task DoctorShouldCreateAndUpdateOwnExamination()
    {
        var (doctorId, doctorUserId) = await CreateDoctorProfileAsync("exam.doctor@local");
        var patientId = await CreatePatientProfileAsync("exam.patient@local");

        await TestApp.RunAsAdministratorAsync();
        var appointmentId = await TestApp.SendAsync(new CreateAppointmentCommand
        {
            DoctorId = doctorId,
            PatientId = patientId,
            AppointmentDate = DateTime.UtcNow.AddDays(1)
        });

        await TestApp.ImpersonateAsync(doctorUserId, [Roles.Doctor]);

        var examinationId = await TestApp.SendAsync(new CreateExaminationCommand
        {
            AppointmentId = appointmentId,
            Diagnosis = "Hypertension",
            Treatment = "Monitor blood pressure"
        });

        await TestApp.SendAsync(new UpdateExaminationCommand
        {
            Id = examinationId,
            Diagnosis = "Stage 1 Hypertension",
            Treatment = "Lifestyle changes"
        });

        var examination = await TestApp.FindAsync<Examination>(examinationId);

        examination.ShouldNotBeNull();
        examination!.Diagnosis.ShouldBe("Stage 1 Hypertension");
        examination.Treatment.ShouldBe("Lifestyle changes");
    }

    [Test]
    public async Task PatientShouldReadOnlyOwnExaminations()
    {
        var (doctorId, _) = await CreateDoctorProfileAsync("exam.access.doctor@local");
        var patientOneUserId = await TestApp.RunAsUserAsync("exam.patient.one@local", "Patient1234!", []);
        var patientTwoUserId = await TestApp.RunAsUserAsync("exam.patient.two@local", "Patient1234!", []);

        await TestApp.RunAsAdministratorAsync();

        var patientOneId = await CreatePatientProfileAsync(patientOneUserId, "Patient", "One", "555-0401");
        var patientTwoId = await CreatePatientProfileAsync(patientTwoUserId, "Patient", "Two", "555-0402");

        var appointmentOneId = await TestApp.SendAsync(new CreateAppointmentCommand
        {
            DoctorId = doctorId,
            PatientId = patientOneId,
            AppointmentDate = DateTime.UtcNow.AddDays(1)
        });

        var appointmentTwoId = await TestApp.SendAsync(new CreateAppointmentCommand
        {
            DoctorId = doctorId,
            PatientId = patientTwoId,
            AppointmentDate = DateTime.UtcNow.AddDays(2)
        });

        await TestApp.SendAsync(new CreateExaminationCommand
        {
            AppointmentId = appointmentOneId,
            Diagnosis = "Condition A",
            Treatment = "Treatment A"
        });

        await TestApp.SendAsync(new CreateExaminationCommand
        {
            AppointmentId = appointmentTwoId,
            Diagnosis = "Condition B",
            Treatment = "Treatment B"
        });

        await TestApp.ImpersonateAsync(patientOneUserId, [Roles.Patient]);
        var myExaminations = await TestApp.SendAsync(new GetMyPatientExaminationsQuery());
        myExaminations.Count.ShouldBe(1);
        myExaminations.Single().PatientId.ShouldBe(patientOneId);

        await TestApp.ImpersonateAsync((await GetDoctorApplicationUserIdAsync(doctorId))!, [Roles.Doctor]);
        var doctorExaminations = await TestApp.SendAsync(new GetMyDoctorExaminationsQuery());
        doctorExaminations.Count.ShouldBe(2);
    }

    [Test]
    public async Task ShouldRejectDuplicateExaminationForAppointment()
    {
        var (doctorId, doctorUserId) = await CreateDoctorProfileAsync("exam.duplicate.doctor@local");
        var patientId = await CreatePatientProfileAsync("exam.duplicate.patient@local");

        await TestApp.RunAsAdministratorAsync();
        var appointmentId = await TestApp.SendAsync(new CreateAppointmentCommand
        {
            DoctorId = doctorId,
            PatientId = patientId,
            AppointmentDate = DateTime.UtcNow.AddDays(1)
        });

        await TestApp.ImpersonateAsync(doctorUserId, [Roles.Doctor]);

        await TestApp.SendAsync(new CreateExaminationCommand
        {
            AppointmentId = appointmentId,
            Diagnosis = "Condition",
            Treatment = "Treatment"
        });

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new CreateExaminationCommand
        {
            AppointmentId = appointmentId,
            Diagnosis = "Duplicate",
            Treatment = "Duplicate"
        }));
    }

    private static async Task<(Guid DoctorId, string ApplicationUserId)> CreateDoctorProfileAsync(string email)
    {
        await TestApp.RunAsAdministratorAsync();

        var departmentId = await TestApp.SendAsync(new CreateDepartmentCommand { Name = $"Department-{Guid.NewGuid():N}" });
        var doctorId = await TestApp.SendAsync(new CreateDoctorCommand
        {
            Email = email,
            Password = "Doctor1234!",
            FirstName = "Demo",
            LastName = "Doctor",
            DepartmentId = departmentId,
            ContactNumber = "555-0490"
        });

        return (doctorId, (await GetDoctorApplicationUserIdAsync(doctorId))!);
    }

    private static async Task<Guid> CreatePatientProfileAsync(string email)
    {
        var userId = await TestApp.RunAsUserAsync(email, "Patient1234!", []);
        return await CreatePatientProfileAsync(userId, "Demo", "Patient", "555-0491");
    }

    private static async Task<Guid> CreatePatientProfileAsync(string userId, string firstName, string lastName, string contactNumber)
    {
        await TestApp.RunAsAdministratorAsync();

        return await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = userId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Unknown,
            ContactNumber = contactNumber,
            Address = "Address"
        });
    }

    private static async Task<string?> GetDoctorApplicationUserIdAsync(Guid doctorId)
    {
        var doctor = await TestApp.FindAsync<Doctor>(doctorId);
        return doctor?.ApplicationUserId;
    }
}
