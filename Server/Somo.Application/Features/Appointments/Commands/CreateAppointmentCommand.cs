using Somo.Application.DTOs;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;

namespace Somo.Application.Features.Appointments.Commands;

public class CreateAppointmentCommand
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IPetRepository _petRepo;
    private readonly IVetRepository _vetRepo;

    public CreateAppointmentCommand(
        IAppointmentRepository appointmentRepo,
        IPetRepository petRepo,
        IVetRepository vetRepo)
    {
        _appointmentRepo = appointmentRepo;
        _petRepo = petRepo;
        _vetRepo = vetRepo;
    }

    public async Task<(bool Success, string Error)> ExecuteAsync(
        CreateAppointmentDto dto, string ownerId)
    {
        
        if (dto.DateTime <= DateTime.UtcNow)
            return (false, "Data programării trebuie să fie în viitor.");

        
        var pet = await _petRepo.GetByIdAsync(dto.PetId);
        if (pet is null || pet.OwnerId != ownerId)
            return (false, "Animalul nu a fost găsit sau nu vă aparține.");

        
        var vet = await _vetRepo.GetByIdAsync(dto.VetId);
        if (vet is null)
            return (false, "Medicul nu a fost găsit.");

        
        var existingAppointments = await _appointmentRepo.GetAllByVetIdAsync(dto.VetId);
        var conflict = existingAppointments.Any(a =>
    a.DateTime.ToUniversalTime() == dto.DateTime.ToUniversalTime() &&
    a.Status != AppointmentStatus.Cancelled);

        if (conflict)
            return (false, "Medicul are deja o programare la ora respectivă.");

        
        var appointment = new Appointment
        {
            PetId = dto.PetId,
            OwnerId = ownerId,
            VetId = dto.VetId,
            ClinicId = dto.ClinicId,
            DateTime = dto.DateTime,
            Reason = dto.Reason,
            Status = AppointmentStatus.Pending
        };

        await _appointmentRepo.CreateAsync(appointment);
        return (true, string.Empty);
    }
}