using HospitalManagement.Application.Doctors.Commands.CreateDoctor;
using HospitalManagement.Application.Doctors.Commands.SetDoctorActiveState;
using HospitalManagement.Application.Doctors.Commands.UpdateDoctor;
using HospitalManagement.Application.Doctors.Queries.GetDoctorById;
using HospitalManagement.Application.Doctors.Queries.GetDoctors;
using HospitalManagement.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HospitalManagement.Web.Endpoints;

public class Doctors : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization(Policies.IsAdministrator);

        groupBuilder.MapGet(GetDoctors);
        groupBuilder.MapGet(GetDoctorById, "{id}");
        groupBuilder.MapPost(CreateDoctor);
        groupBuilder.MapPut(UpdateDoctor, "{id}");
        groupBuilder.MapPatch(SetDoctorActiveState, "{id}/status");
    }

    [EndpointSummary("Get all Doctors")]
    [EndpointDescription("Retrieves all doctors.")]
    public static async Task<Ok<IReadOnlyCollection<DoctorSummaryDto>>> GetDoctors(ISender sender)
    {
        var doctors = await sender.Send(new GetDoctorsQuery());

        return TypedResults.Ok(doctors);
    }

    [EndpointSummary("Get Doctor by ID")]
    [EndpointDescription("Retrieves a doctor by ID.")]
    public static async Task<Ok<DoctorDetailsDto>> GetDoctorById(ISender sender, Guid id)
    {
        var doctor = await sender.Send(new GetDoctorByIdQuery(id));

        return TypedResults.Ok(doctor);
    }

    [EndpointSummary("Create Doctor")]
    [EndpointDescription("Creates a doctor identity user and profile, and returns the profile ID.")]
    public static async Task<Created<Guid>> CreateDoctor(ISender sender, CreateDoctorCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/{nameof(Doctors)}/{id}", id);
    }

    [EndpointSummary("Update Doctor")]
    [EndpointDescription("Updates an existing doctor profile.")]
    public static async Task<NoContent> UpdateDoctor(ISender sender, Guid id, UpdateDoctorCommand command)
    {
        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Set Doctor Active State")]
    [EndpointDescription("Activates or deactivates a doctor profile.")]
    public static async Task<NoContent> SetDoctorActiveState(ISender sender, Guid id, SetDoctorActiveStateCommand command)
    {
        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }
}