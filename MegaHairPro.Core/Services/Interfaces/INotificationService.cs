using System.Threading.Tasks;

namespace MegaHairPro.Web.Services.Interfaces;

public interface INotificationService
{
    Task EnviarEmailAsync(string destino, string assunto, string mensagemHtml);
    Task EnviarSmsAsync(string telefone, string mensagem);
}

