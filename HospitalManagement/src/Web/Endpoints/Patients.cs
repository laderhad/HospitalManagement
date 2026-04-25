using HospitalManagement.Application.Patients.Commands.CreatePatientByAdmin;
using HospitalManagement.Application.Patients.Commands.UpdateMyPatientProfile;
using HospitalManagement.Application.Patients.Commands.UpdatePatient;
using HospitalManagement.Application.Patients.Queries.GetMyPatientProfile;
using HospitalManagement.Application.Patients.Queries.GetPatientById;
using HospitalManagement.Application.Patients.Queries.GetPatients;
using HospitalManagement.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HospitalManagement.Web.Endpoints;

public class Patients : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetMyPatientProfile, "me").RequireAuthorization(Policies.IsPatient);
        groupBuilder.MapPut(UpdateMyPatientProfile, "me").RequireAuthorization(Policies.IsPatient);

        groupBuilder.MapGet(GetPatients).RequireAuthorization(Policies.IsAdministrator);
        groupBuilder.MapGet(GetPatientById, "{id}").RequireAuthorization(Policies.IsAdministrator);
        groupBuilder.MapPost(CreatePatient).RequireAuthorization(Policies.IsAdministrator);
        groupBuilder.MapPut(UpdatePatient, "{id}").RequireAuthorization(Policies.IsAdministrator);
    }

    [EndpointSummary("Get all Patients")]
    [EndpointDescription("Retrieves all patient profiles.")]
    public static async Task<Ok<IReadOnlyCollection<PatientSummaryDto>>> GetPatients(ISender sender)
    {
        var patients = await sender.Send(new GetPatientsQuery());

        return TypedResults.Ok(patients);
    }

    [EndpointSummary("Get Patient by ID")]
    [EndpointDescription("Retrieves a patient profile by ID.")]
    public static async Task<Ok<PatientDetailsDto>> GetPatientById(ISender sender, Guid id)
    {
        var patient = await sender.Send(new GetPatientByIdQuery(id));

        return TypedResults.Ok(patient);
    }

    [EndpointSummary("Create Patient")]
    [EndpointDescription("Creates a patient profile for an existing identity user.")]
    public static async Task<Created<Guid>> CreatePatient(ISender sender, CreatePatientByAdminCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/{nameof(Patients)}/{id}", id);
    }

    [EndpointSummary("Update Patient")]
    [EndpointDescription("Updates a patient profile by ID.")]
    public static async Task<NoContent> UpdatePatient(ISender sender, Guid id, UpdatePatientCommand command)
    {
        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Get My Patient Profile")]
    [EndpointDescription("Retrieves the current signed-in patient's own profile.")]
    public static async Task<Ok<MyPatientProfileDto>> GetMyPatientProfile(ISender sender)
    {
        var patient = await sender.Send(new GetMyPatientProfileQuery());

        return TypedResults.Ok(patient);
    }

    [EndpointSummary("Update My Patient Profile")]
    [EndpointDescription("Updates the current signed-in patient's own profile.")]
    public static async Task<NoContent> UpdateMyPatientProfile(ISender sender, UpdateMyPatientProfileCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }
}