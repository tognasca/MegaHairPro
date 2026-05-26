using MegaHairPro.Web.Models;

namespace MegaHairPro.Web.Services;

/// <summary>
/// Compatibilidade com o portal Node (status 0–4 no JSON da UI).
/// </summary>
public static class BookingStatusMapper
{
    public static int ToPortal(Agendamento ag)
    {
        if (ag.Cancelado) return 3;
        return ag.Status switch
        {
            StatusAgendamento.Agendado when !ag.SinalPago => 0,
            StatusAgendamento.Confirmado => 1,
            StatusAgendamento.Concluido => 4,
            StatusAgendamento.Cancelado => 3,
            _ => (int)ag.Status
        };
    }

    public static bool BloqueiaHorario(StatusAgendamento status, bool cancelado) =>
        !cancelado && (status == StatusAgendamento.Agendado || status == StatusAgendamento.Confirmado);
}
