using HospitalManagement.Application.Prescriptions.Commands.CreatePrescription;
using HospitalManagement.Application.Prescriptions.Queries.GetMyDoctorPrescriptions;
using HospitalManagement.Application.Prescriptions.Queries.GetMyPatientPrescriptions;
using HospitalManagement.Application.Prescriptions.Queries.GetPrescriptionById;
using HospitalManagement.Application.Prescriptions.Queries.GetPrescriptions;
using HospitalManagement.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HospitalManagement.Web.Endpoints;

public class Prescriptions : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetMyPatientPrescriptions, "me").RequireAuthorization(Policies.IsPatient);
        groupBuilder.MapGet(GetMyDoctorPrescriptions, "doctor").RequireAuthorization(Policies.IsDoctor);
        groupBuilder.MapGet(GetPrescriptions).RequireAuthorization(Policies.IsAdministrator);
        groupBuilder.MapGet(GetPrescriptionById, "{id}");
        groupBuilder.MapPost(CreatePrescription);
    }

    public static async Task<Ok<IReadOnlyCollection<PrescriptionSummaryDto>>> GetPrescriptions(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetPrescriptionsQuery()));

    public static async Task<Ok<PrescriptionDetailsDto>> GetPrescriptionById(ISender sender, Guid id)
        => TypedResults.Ok(await sender.Send(new GetPrescriptionByIdQuery(id)));

    public static async Task<Ok<IReadOnlyCollection<PrescriptionSummaryDto>>> GetMyPatientPrescriptions(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetMyPatientPrescriptionsQuery()));

    public static async Task<Ok<IReadOnlyCollection<PrescriptionSummaryDto>>> GetMyDoctorPrescriptions(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetMyDoctorPrescriptionsQuery()));

    public static async Task<Created<Guid>> CreatePrescription(ISender sender, CreatePrescriptionCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/{nameof(Prescriptions)}/{id}", id);
    }
}
