using Somo.Application.DTOs;
using Somo.Domain.Interfaces;

namespace Somo.Application.Features.Appointments.Queries;

public class GetAvailableSlotsQuery
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IVetRepository _vetRepo;

    public GetAvailableSlotsQuery(
        IAppointmentRepository appointmentRepo,
        IVetRepository vetRepo)
    {
        _appointmentRepo = appointmentRepo;
        _vetRepo = vetRepo;
    }

    public async Task<IEnumerable<AvailableSlotDto>> ExecuteAsync(string vetId, DateTime date)
    {
       
        var vet = await _vetRepo.GetByIdAsync(vetId);
        if (vet is null) return Enumerable.Empty<AvailableSlotDto>();

        
        var existingAppointments = await _appointmentRepo.GetAllByVetIdAsync(vetId);
        var bookedSlots = existingAppointments
            .Where(a => a.DateTime.Date == date.Date &&
                        a.Status != Domain.Entities.AppointmentStatus.Cancelled)
            .Select(a => a.DateTime.Hour)
            .ToHashSet();

        
        var slots = new List<AvailableSlotDto>();
        for (int hour = 9; hour < 17; hour++)
        {
            slots.Add(new AvailableSlotDto
            {
                DateTime = date.Date.AddHours(hour),
                IsAvailable = !bookedSlots.Contains(hour)
            });
        }

        return slots;
    }
}