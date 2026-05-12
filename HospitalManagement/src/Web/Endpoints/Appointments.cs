using HospitalManagement.Application.Appointments.Commands.CancelAppointment;
using HospitalManagement.Application.Appointments.Commands.CompleteAppointment;
using HospitalManagement.Application.Appointments.Commands.CreateAppointment;
using HospitalManagement.Application.Appointments.Commands.CreateMyAppointment;
using HospitalManagement.Application.Appointments.Commands.RescheduleAppointment;
using HospitalManagement.Application.Appointments.Queries.GetAppointmentById;
using HospitalManagement.Application.Appointments.Queries.GetAppointments;
using HospitalManagement.Application.Appointments.Queries.GetMyDoctorAppointments;
using HospitalManagement.Application.Appointments.Queries.GetMyPatientAppointments;
using HospitalManagement.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HospitalManagement.Web.Endpoints;

public class Appointments : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetAppointments).RequireAuthorization(Policies.IsAdministrator);
        groupBuilder.MapGet(GetAppointmentById, "{id}");
        groupBuilder.MapGet(GetMyPatientAppointments, "me").RequireAuthorization(Policies.IsPatient);
        groupBuilder.MapGet(GetMyDoctorAppointments, "doctor").RequireAuthorization(Policies.IsDoctor);
        groupBuilder.MapPost(CreateAppointment).RequireAuthorization(Policies.IsAdministrator);
        groupBuilder.MapPost(CreateMyAppointment, "me").RequireAuthorization(Policies.IsPatient);
        groupBuilder.MapPatch(RescheduleAppointment, "{id}/reschedule");
        groupBuilder.MapPatch(CancelAppointment, "{id}/cancel");
        groupBuilder.MapPatch(CompleteAppointment, "{id}/complete");
    }

    public static async Task<Ok<IReadOnlyCollection<AppointmentSummaryDto>>> GetAppointments(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetAppointmentsQuery()));

    public static async Task<Ok<AppointmentDetailsDto>> GetAppointmentById(ISender sender, Guid id)
        => TypedResults.Ok(await sender.Send(new GetAppointmentByIdQuery(id)));

    public static async Task<Ok<IReadOnlyCollection<AppointmentSummaryDto>>> GetMyPatientAppointments(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetMyPatientAppointmentsQuery()));

    public static async Task<Ok<IReadOnlyCollection<AppointmentSummaryDto>>> GetMyDoctorAppointments(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetMyDoctorAppointmentsQuery()));

    public static async Task<Created<Guid>> CreateAppointment(ISender sender, CreateAppointmentCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/{nameof(Appointments)}/{id}", id);
    }

    public static async Task<Created<Guid>> CreateMyAppointment(ISender sender, CreateMyAppointmentCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/{nameof(Appointments)}/{id}", id);
    }

    public static async Task<NoContent> RescheduleAppointment(ISender sender, Guid id, RescheduleAppointmentCommand command)
    {
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CancelAppointment(ISender sender, Guid id)
    {
        await sender.Send(new CancelAppointmentCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> CompleteAppointment(ISender sender, Guid id)
    {
        await sender.Send(new CompleteAppointmentCommand(id));
        return TypedResults.NoContent();
    }
}
