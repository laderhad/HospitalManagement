using HospitalManagement.Application.Departments.Commands.CreateDepartment;
using HospitalManagement.Application.Departments.Commands.SetDepartmentActiveState;
using HospitalManagement.Application.Departments.Commands.UpdateDepartment;
using HospitalManagement.Application.Departments.Queries.GetDepartmentById;
using HospitalManagement.Application.Departments.Queries.GetDepartments;
using HospitalManagement.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HospitalManagement.Web.Endpoints;

public class Departments : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization(Policies.IsAdministrator);

        groupBuilder.MapGet(GetDepartments);
        groupBuilder.MapGet(GetDepartmentById, "{id}");
        groupBuilder.MapPost(CreateDepartment);
        groupBuilder.MapPut(UpdateDepartment, "{id}");
        groupBuilder.MapPatch(SetDepartmentActiveState, "{id}/status");
    }

    [EndpointSummary("Get all Departments")]
    [EndpointDescription("Retrieves all departments.")]
    public static async Task<Ok<IReadOnlyCollection<DepartmentSummaryDto>>> GetDepartments(ISender sender)
    {
        var departments = await sender.Send(new GetDepartmentsQuery());

        return TypedResults.Ok(departments);
    }

    [EndpointSummary("Get Department by ID")]
    [EndpointDescription("Retrieves a department by its ID.")]
    public static async Task<Ok<DepartmentDetailsDto>> GetDepartmentById(ISender sender, Guid id)
    {
        var department = await sender.Send(new GetDepartmentByIdQuery(id));

        return TypedResults.Ok(department);
    }

    [EndpointSummary("Create Department")]
    [EndpointDescription("Creates a new department and returns its ID.")]
    public static async Task<Created<Guid>> CreateDepartment(ISender sender, CreateDepartmentCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/{nameof(Departments)}/{id}", id);
    }

    [EndpointSummary("Update Department")]
    [EndpointDescription("Updates an existing department.")]
    public static async Task<NoContent> UpdateDepartment(ISender sender, Guid id, UpdateDepartmentCommand command)
    {
        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Set Department Active State")]
    [EndpointDescription("Activates or deactivates a department.")]
    public static async Task<NoContent> SetDepartmentActiveState(ISender sender, Guid id, SetDepartmentActiveStateCommand command)
    {
        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }
}