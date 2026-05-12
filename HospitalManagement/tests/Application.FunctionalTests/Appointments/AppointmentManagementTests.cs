using HospitalManagement.Application.Appointments.Commands.CancelAppointment;
using HospitalManagement.Application.Appointments.Commands.CompleteAppointment;
using HospitalManagement.Application.Appointments.Commands.CreateAppointment;
using HospitalManagement.Application.Appointments.Commands.CreateMyAppointment;
using HospitalManagement.Application.Appointments.Commands.RescheduleAppointment;
using HospitalManagement.Application.Appointments.Queries.GetAppointmentById;
using HospitalManagement.Application.Appointments.Queries.GetAppointments;
using HospitalManagement.Application.Appointments.Queries.GetMyDoctorAppointments;
using HospitalManagement.Application.Appointments.Queries.GetMyPatientAppointments;
using HospitalManagement.Application.Common.Exceptions;
using HospitalManagement.Application.Departments.Commands.CreateDepartment;
using HospitalManagement.Application.Doctors.Commands.CreateDoctor;
using HospitalManagement.Application.Patients.Commands.CreatePatientByAdmin;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.FunctionalTests.Appointments;

public class AppointmentManagementTests : TestBase
{
    [Test]
    public async Task AdministratorShouldCreateAppointment()
    {
        var doctorId = await CreateDoctorProfileAsync("appointments.doctor@local");
        var patientId = await CreatePatientProfileAsync("appointments.patient@local");

        await TestApp.RunAsAdministratorAsync();

        var appointmentId = await TestApp.SendAsync(new CreateAppointmentCommand
        {
            DoctorId = doctorId,
            PatientId = patientId,
            AppointmentDate = DateTime.UtcNow.AddDays(1)
        });

        var appointment = await TestApp.FindAsync<Appointment>(appointmentId);

        appointment.ShouldNotBeNull();
        appointment!.DoctorId.ShouldBe(doctorId);
        appointment.PatientId.ShouldBe(patientId);
        appointment.Status.ShouldBe(AppointmentStatus.Scheduled);
    }

    [Test]
    public async Task PatientShouldCreateOwnAppointment()
    {
        var doctorId = await CreateDoctorProfileAsync("my.appointments.doctor@local");
        var patientUserId = await TestApp.RunAsUserAsync("my.appointments.patient@local", "Patient1234!", []);

        await TestApp.RunAsAdministratorAsync();
        var patientId = await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = patientUserId,
            FirstName = "Demo",
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Unknown,
            ContactNumber = "555-0301",
            Address = "Address"
        });

        await TestApp.ImpersonateAsync(patientUserId, [Roles.Patient]);

        var appointmentId = await TestApp.SendAsync(new CreateMyAppointmentCommand
        {
            DoctorId = doctorId,
            AppointmentDate = DateTime.UtcNow.AddDays(2)
        });

        var appointment = await TestApp.FindAsync<Appointment>(appointmentId);

        appointment.ShouldNotBeNull();
        appointment!.PatientId.ShouldBe(patientId);
    }

    [Test]
    public async Task DoctorAndPatientShouldSeeOnlyOwnAppointments()
    {
        var doctorId = await CreateDoctorProfileAsync("access.doctor@local");
        var patientOneUserId = await TestApp.RunAsUserAsync("access.patient.one@local", "Patient1234!", []);
        var patientTwoUserId = await TestApp.RunAsUserAsync("access.patient.two@local", "Patient1234!", []);

        await TestApp.RunAsAdministratorAsync();

        var patientOneId = await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = patientOneUserId,
            FirstName = "First",
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Unknown,
            ContactNumber = "555-0302",
            Address = "Address 1"
        });

        var patientTwoId = await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = patientTwoUserId,
            FirstName = "Second",
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 2, 2, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Unknown,
            ContactNumber = "555-0303",
            Address = "Address 2"
        });

        var appointmentOneId = await TestApp.SendAsync(new CreateAppointmentCommand
        {
            DoctorId = doctorId,
            PatientId = patientOneId,
            AppointmentDate = DateTime.UtcNow.AddDays(1)
        });

        await TestApp.SendAsync(new CreateAppointmentCommand
        {
            DoctorId = doctorId,
            PatientId = patientTwoId,
            AppointmentDate = DateTime.UtcNow.AddDays(2)
        });

        await TestApp.ImpersonateAsync(patientOneUserId, [Roles.Patient]);
        var myAppointments = await TestApp.SendAsync(new GetMyPatientAppointmentsQuery());
        myAppointments.Count.ShouldBe(1);
        myAppointments.Single().Id.ShouldBe(appointmentOneId);

        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GetAppointmentsQuery()));

        await TestApp.ImpersonateAsync((await GetDoctorApplicationUserIdAsync(doctorId))!, [Roles.Doctor]);
        var doctorAppointments = await TestApp.SendAsync(new GetMyDoctorAppointmentsQuery());
        doctorAppointments.Count.ShouldBe(2);
    }

    [Test]
    public async Task ShouldRescheduleAndCompleteAppointment()
    {
        var doctorId = await CreateDoctorProfileAsync("workflow.doctor@local");
        var patientId = await CreatePatientProfileAsync("workflow.patient@local");

        await TestApp.RunAsAdministratorAsync();
        var appointmentId = await TestApp.SendAsync(new CreateAppointmentCommand
        {
            DoctorId = doctorId,
            PatientId = patientId,
            AppointmentDate = DateTime.UtcNow.AddDays(1)
        });

        await TestApp.SendAsync(new RescheduleAppointmentCommand
        {
            Id = appointmentId,
            AppointmentDate = DateTime.UtcNow.AddDays(3)
        });

        var doctorUserId = await GetDoctorApplicationUserIdAsync(doctorId);
        await TestApp.ImpersonateAsync(doctorUserId!, [Roles.Doctor]);
        await TestApp.SendAsync(new CompleteAppointmentCommand(appointmentId));

        var appointment = await TestApp.FindAsync<Appointment>(appointmentId);
        appointment.ShouldNotBeNull();
        appointment!.Status.ShouldBe(AppointmentStatus.Completed);
    }

    [Test]
    public async Task ShouldRejectConflictingDoctorSchedule()
    {
        var doctorId = await CreateDoctorProfileAsync("conflict.doctor@local");
        var patientOneId = await CreatePatientProfileAsync("conflict.patient.one@local");
        var patientTwoId = await CreatePatientProfileAsync("conflict.patient.two@local");

        await TestApp.RunAsAdministratorAsync();
        var appointmentDate = DateTime.UtcNow.AddDays(1);

        await TestApp.SendAsync(new CreateAppointmentCommand
        {
            DoctorId = doctorId,
            PatientId = patientOneId,
            AppointmentDate = appointmentDate
        });

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new CreateAppointmentCommand
        {
            DoctorId = doctorId,
            PatientId = patientTwoId,
            AppointmentDate = appointmentDate
        }));
    }

    private static async Task<Guid> CreateDoctorProfileAsync(string email)
    {
        await TestApp.RunAsAdministratorAsync();

        var departmentId = await TestApp.SendAsync(new CreateDepartmentCommand { Name = $"Department-{Guid.NewGuid():N}" });

        return await TestApp.SendAsync(new CreateDoctorCommand
        {
            Email = email,
            Password = "Doctor1234!",
            FirstName = "Demo",
            LastName = "Doctor",
            DepartmentId = departmentId,
            ContactNumber = "555-0390"
        });
    }

    private static async Task<Guid> CreatePatientProfileAsync(string email)
    {
        var userId = await TestApp.RunAsUserAsync(email, "Patient1234!", []);
        await TestApp.RunAsAdministratorAsync();

        return await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = userId,
            FirstName = "Demo",
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Unknown,
            ContactNumber = "555-0391",
            Address = "Address"
        });
    }

    private static async Task<string?> GetDoctorApplicationUserIdAsync(Guid doctorId)
    {
        var doctor = await TestApp.FindAsync<Doctor>(doctorId);
        return doctor?.ApplicationUserId;
    }
}
