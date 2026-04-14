using HospitalManagement.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace HospitalManagement.Domain.UnitTests.Entities;

public class PrescriptionTests
{
    [Test]
    public void ShouldRequirePositiveDuration()
    {
        Should.Throw<ArgumentException>(() => new Prescription(
            Guid.NewGuid(),
            "Amoxicillin",
            "500mg",
            "Twice daily",
            0));
    }
}
