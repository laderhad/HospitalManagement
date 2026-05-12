using HospitalManagement.Application.Examinations.Commands.CreateExamination;
using HospitalManagement.Application.Examinations.Commands.UpdateExamination;
using HospitalManagement.Application.Examinations.Queries.GetExaminationById;
using HospitalManagement.Application.Examinations.Queries.GetExaminations;
using HospitalManagement.Application.Examinations.Queries.GetMyDoctorExaminations;
using HospitalManagement.Application.Examinations.Queries.GetMyPatientExaminations;
using HospitalManagement.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HospitalManagement.Web.Endpoints;

public class Examinations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetMyPatientExaminations, "me").RequireAuthorization(Policies.IsPatient);
        groupBuilder.MapGet(GetMyDoctorExaminations, "doctor").RequireAuthorization(Policies.IsDoctor);
        groupBuilder.MapGet(GetExaminations).RequireAuthorization(Policies.IsAdministrator);
        groupBuilder.MapGet(GetExaminationById, "{id}");
        groupBuilder.MapPost(CreateExamination);
        groupBuilder.MapPut(UpdateExamination, "{id}");
    }

    public static async Task<Ok<IReadOnlyCollection<ExaminationSummaryDto>>> GetExaminations(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetExaminationsQuery()));

    public static async Task<Ok<ExaminationDetailsDto>> GetExaminationById(ISender sender, Guid id)
        => TypedResults.Ok(await sender.Send(new GetExaminationByIdQuery(id)));

    public static async Task<Ok<IReadOnlyCollection<ExaminationSummaryDto>>> GetMyPatientExaminations(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetMyPatientExaminationsQuery()));

    public static async Task<Ok<IReadOnlyCollection<ExaminationSummaryDto>>> GetMyDoctorExaminations(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetMyDoctorExaminationsQuery()));

    public static async Task<Created<Guid>> CreateExamination(ISender sender, CreateExaminationCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/{nameof(Examinations)}/{id}", id);
    }

    public static async Task<NoContent> UpdateExamination(ISender sender, Guid id, UpdateExaminationCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }
}
