using HospitalManagement.Application.LabResults.Commands.CorrectLabResult;
using HospitalManagement.Application.LabResults.Commands.CreateLabResult;
using HospitalManagement.Application.LabResults.Queries.GetLabResultById;
using HospitalManagement.Application.LabResults.Queries.GetLabResults;
using HospitalManagement.Application.LabResults.Queries.GetMyDoctorLabResults;
using HospitalManagement.Application.LabResults.Queries.GetMyPatientLabResults;
using HospitalManagement.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HospitalManagement.Web.Endpoints;

public class LabResults : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetMyPatientLabResults, "me").RequireAuthorization(Policies.IsPatient);
        groupBuilder.MapGet(GetMyDoctorLabResults, "doctor").RequireAuthorization(Policies.IsDoctor);
        groupBuilder.MapGet(GetLabResults).RequireAuthorization(Policies.IsAdministrator);
        groupBuilder.MapGet(GetLabResultById, "{id}");
        groupBuilder.MapPost(CreateLabResult).RequireAuthorization(Policies.IsAdministrator);
        groupBuilder.MapPut(CorrectLabResult, "{id}").RequireAuthorization(Policies.IsAdministrator);
    }

    public static async Task<Ok<IReadOnlyCollection<LabResultSummaryDto>>> GetLabResults(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetLabResultsQuery()));

    public static async Task<Ok<LabResultDetailsDto>> GetLabResultById(ISender sender, Guid id)
        => TypedResults.Ok(await sender.Send(new GetLabResultByIdQuery(id)));

    public static async Task<Ok<IReadOnlyCollection<LabResultSummaryDto>>> GetMyPatientLabResults(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetMyPatientLabResultsQuery()));

    public static async Task<Ok<IReadOnlyCollection<LabResultSummaryDto>>> GetMyDoctorLabResults(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetMyDoctorLabResultsQuery()));

    public static async Task<Created<Guid>> CreateLabResult(ISender sender, CreateLabResultCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/{nameof(LabResults)}/{id}", id);
    }

    public static async Task<NoContent> CorrectLabResult(ISender sender, Guid id, CorrectLabResultCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }
}
