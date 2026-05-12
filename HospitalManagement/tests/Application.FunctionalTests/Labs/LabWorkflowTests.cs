using HospitalManagement.Application.Appointments.Commands.CreateAppointment;
using HospitalManagement.Application.Departments.Commands.CreateDepartment;
using HospitalManagement.Application.Doctors.Commands.CreateDoctor;
using HospitalManagement.Application.Examinations.Commands.CreateExamination;
using HospitalManagement.Application.LabRequests.Commands.CreateLabRequest;
using HospitalManagement.Application.LabRequests.Queries.GetLabRequestById;
using HospitalManagement.Application.LabResults.Commands.CorrectLabResult;
using HospitalManagement.Application.LabResults.Commands.CreateLabResult;
using HospitalManagement.Application.LabResults.Queries.GetMyDoctorLabResults;
using HospitalManagement.Application.LabResults.Queries.GetMyPatientLabResults;
using HospitalManagement.Application.Patients.Commands.CreatePatientByAdmin;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.FunctionalTests.Labs;

public class LabWorkflowTests : TestBase
{
    [Test]
    public async Task DoctorShouldCreateLabRequestAndAdministratorShouldRecordResult()
    {
        var (doctorId, doctorUserId) = await CreateDoctorProfileAsync("lab.doctor@local");
        var patientUserId = await TestApp.RunAsUserAsync("lab.patient@local", "Patient1234!", []);

        await TestApp.RunAsAdministratorAsync();
        var patientId = await CreatePatientProfileAsync(patientUserId);
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
            Diagnosis = "Needs lab",
            Treatment = "Await results"
        });

        var labRequestId = await TestApp.SendAsync(new CreateLabRequestCommand
        {
            ExaminationId = examinationId,
            TestNames = ["CBC", "CRP"]
        });

        await TestApp.RunAsAdministratorAsync();
        var labRequest = await TestApp.SendAsync(new GetLabRequestByIdQuery(labRequestId));
        var labRequestItemId = labRequest.Items.First().Id;

        var labResultId = await TestApp.SendAsync(new CreateLabResultCommand
        {
            LabRequestItemId = labRequestItemId,
            ResultValue = "12.1",
            Units = "g/dL",
            ReferenceRange = "11-15",
            Notes = "Normal"
        });

        await TestApp.SendAsync(new CorrectLabResultCommand
        {
            Id = labResultId,
            ResultValue = "12.4",
            Units = "g/dL",
            ReferenceRange = "11-15",
            Notes = "Verified"
        });

        var result = await TestApp.FindAsync<LabResult>(labResultId);
        result.ShouldNotBeNull();
        result!.ResultValue.ShouldBe("12.4");
        result.Notes.ShouldBe("Verified");
    }

    [Test]
    public async Task PatientAndDoctorShouldSeeOwnLabResults()
    {
        var (doctorId, doctorUserId) = await CreateDoctorProfileAsync("lab.access.doctor@local");
        var patientUserId = await TestApp.RunAsUserAsync("lab.access.patient@local", "Patient1234!", []);

        await TestApp.RunAsAdministratorAsync();
        var patientId = await CreatePatientProfileAsync(patientUserId);
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
            Diagnosis = "Lab",
            Treatment = "Observe"
        });

        var labRequestId = await TestApp.SendAsync(new CreateLabRequestCommand
        {
            ExaminationId = examinationId,
            TestNames = ["Glucose"]
        });

        await TestApp.RunAsAdministratorAsync();
        var labRequest = await TestApp.SendAsync(new GetLabRequestByIdQuery(labRequestId));

        await TestApp.SendAsync(new CreateLabResultCommand
        {
            LabRequestItemId = labRequest.Items.Single().Id,
            ResultValue = "94",
            Units = "mg/dL",
            ReferenceRange = "70-100"
        });

        await TestApp.ImpersonateAsync(doctorUserId, [Roles.Doctor]);
        var doctorResults = await TestApp.SendAsync(new GetMyDoctorLabResultsQuery());
        doctorResults.Count.ShouldBe(1);

        await TestApp.ImpersonateAsync(patientUserId, [Roles.Patient]);
        var patientResults = await TestApp.SendAsync(new GetMyPatientLabResultsQuery());
        patientResults.Count.ShouldBe(1);
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
            ContactNumber = "555-0690"
        });

        var doctor = await TestApp.FindAsync<Doctor>(doctorId);
        return (doctorId, doctor!.ApplicationUserId);
    }

    private static async Task<Guid> CreatePatientProfileAsync(string userId)
    {
        await TestApp.RunAsAdministratorAsync();

        return await TestApp.SendAsync(new CreatePatientByAdminCommand
        {
            ApplicationUserId = userId,
            FirstName = "Demo",
            LastName = "Patient",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Gender = Gender.Unknown,
            ContactNumber = "555-0691",
            Address = "Address"
        });
    }
}
