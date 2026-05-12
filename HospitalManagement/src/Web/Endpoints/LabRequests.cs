using HospitalManagement.Application.LabRequests.Commands.CreateLabRequest;
using HospitalManagement.Application.LabRequests.Queries.GetLabRequestById;
using HospitalManagement.Application.LabRequests.Queries.GetLabRequests;
using HospitalManagement.Application.LabRequests.Queries.GetMyDoctorLabRequests;
using HospitalManagement.Application.LabRequests.Queries.GetMyPatientLabRequests;
using HospitalManagement.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HospitalManagement.Web.Endpoints;

public class LabRequests : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetMyPatientLabRequests, "me").RequireAuthorization(Policies.IsPatient);
        groupBuilder.MapGet(GetMyDoctorLabRequests, "doctor").RequireAuthorization(Policies.IsDoctor);
        groupBuilder.MapGet(GetLabRequests).RequireAuthorization(Policies.IsAdministrator);
        groupBuilder.MapGet(GetLabRequestById, "{id}");
        groupBuilder.MapPost(CreateLabRequest);
    }

    public static async Task<Ok<IReadOnlyCollection<LabRequestSummaryDto>>> GetLabRequests(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetLabRequestsQuery()));

    public static async Task<Ok<LabRequestDetailsDto>> GetLabRequestById(ISender sender, Guid id)
        => TypedResults.Ok(await sender.Send(new GetLabRequestByIdQuery(id)));

    public static async Task<Ok<IReadOnlyCollection<LabRequestSummaryDto>>> GetMyPatientLabRequests(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetMyPatientLabRequestsQuery()));

    public static async Task<Ok<IReadOnlyCollection<LabRequestSummaryDto>>> GetMyDoctorLabRequests(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetMyDoctorLabRequestsQuery()));

    public static async Task<Created<Guid>> CreateLabRequest(ISender sender, CreateLabRequestCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/{nameof(LabRequests)}/{id}", id);
    }
}
