using HospitalManagement.Application.Appointments.Commands.CreateAppointment;
using HospitalManagement.Application.Departments.Commands.CreateDepartment;
using HospitalManagement.Application.Doctors.Commands.CreateDoctor;
using HospitalManagement.Application.Examinations.Commands.CreateExamination;
using HospitalManagement.Application.Patients.Commands.CreatePatientByAdmin;
using HospitalManagement.Application.Prescriptions.Commands.CreatePrescription;
using HospitalManagement.Application.Prescriptions.Queries.GetMyDoctorPrescriptions;
using HospitalManagement.Application.Prescriptions.Queries.GetMyPatientPrescriptions;
using HospitalManagement.Domain.Constants;
using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;

namespace HospitalManagement.Application.FunctionalTests.Prescriptions;

public class PrescriptionManagementTests : TestBase
{
    [Test]
    public async Task DoctorShouldCreatePrescriptionForOwnExamination()
    {
        var (doctorId, doctorUserId) = await CreateDoctorProfileAsync("prescription.doctor@local");
        var patientId = await CreatePatientProfileAsync("prescription.patient@local");

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
            Diagnosis = "Migraine",
            Treatment = "Rest"
        });

        var prescriptionId = await TestApp.SendAsync(new CreatePrescriptionCommand
        {
            ExaminationId = examinationId,
            MedicationName = "Paracetamol",
            Dosage = "500mg",
            Frequency = "Twice daily",
            DurationDays = 5,
            Notes = "After meals"
        });

        var prescription = await TestApp.FindAsync<Prescription>(prescriptionId);
        prescription.ShouldNotBeNull();
        prescription!.MedicationName.ShouldBe("Paracetamol");
    }

    [Test]
    public async Task PatientAndDoctorShouldSeeOwnPrescriptions()
    {
        var (doctorId, doctorUserId) = await CreateDoctorProfileAsync("prescription.access.doctor@local");
        var patientUserId = await TestApp.RunAsUserAsync("prescription.patient.user@local", "Patient1234!", []);

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
            Diagnosis = "Diagnosis",
            Treatment = "Treatment"
        });

        await TestApp.SendAsync(new CreatePrescriptionCommand
        {
            ExaminationId = examinationId,
            MedicationName = "Medicine",
            Dosage = "1 tablet",
            Frequency = "Daily",
            DurationDays = 7
        });

        var doctorPrescriptions = await TestApp.SendAsync(new GetMyDoctorPrescriptionsQuery());
        doctorPrescriptions.Count.ShouldBe(1);

        await TestApp.ImpersonateAsync(patientUserId, [Roles.Patient]);
        var patientPrescriptions = await TestApp.SendAsync(new GetMyPatientPrescriptionsQuery());
        patientPrescriptions.Count.ShouldBe(1);
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
            ContactNumber = "555-0590"
        });

        var doctor = await TestApp.FindAsync<Doctor>(doctorId);
        return (doctorId, doctor!.ApplicationUserId);
    }

    private static async Task<Guid> CreatePatientProfileAsync(string email)
    {
        var userId = await TestApp.RunAsUserAsync(email, "Patient1234!", []);
        return await CreatePatientProfileAsync(userId);
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
            ContactNumber = "555-0591",
            Address = "Address"
        });
    }
}
