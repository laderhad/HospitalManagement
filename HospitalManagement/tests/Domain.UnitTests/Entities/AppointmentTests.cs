using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace HospitalManagement.Domain.UnitTests.Entities;

public class AppointmentTests
{
    [Test]
    public void ShouldStartAsScheduled()
    {
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        appointment.Status.ShouldBe(AppointmentStatus.Scheduled);
    }

    [Test]
    public void ShouldNotCancelCompletedAppointment()
    {
        var appointment = new Appointment(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        appointment.Complete();

        Should.Throw<InvalidOperationException>(() => appointment.Cancel());
    }
}
