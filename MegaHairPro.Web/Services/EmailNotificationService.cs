using MegaHairPro.Web.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MegaHairPro.Web.Services;

public class EmailNotificationService : INotificationService
{
    private readonly IConfiguration _config;

    public EmailNotificationService(IConfiguration config)
    {
        _config = config;
    }

    public async Task EnviarEmailAsync(string destino, string assunto, string mensagemHtml)
    {
        if (string.IsNullOrWhiteSpace(destino))
            return;

        var host = _config["Email:Smtp:Host"];
        var porta = int.TryParse(_config["Email:Smtp:Porta"], out var p) ? p : 587;
        var usuario = _config["Email:Smtp:Usuario"];
        var senha = _config["Email:Smtp:Senha"];
        var remetente = _config["Email:Remetente"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha))
            return;

        using var smtp = new SmtpClient(host, porta)
        {
            Credentials = new NetworkCredential(usuario, senha),
            EnableSsl = true
        };

        using var mail = new MailMessage(remetente ?? usuario, destino, assunto, mensagemHtml)
        {
            IsBodyHtml = true
        };

        await smtp.SendMailAsync(mail);
    }

    public Task EnviarSmsAsync(string telefone, string mensagem)
    {
        // Implementação fake inicial: aqui poderíamos integrar futuramente com um gateway.
        // No momento, apenas retornar Task.CompletedTask para não quebrar o fluxo.
        return Task.CompletedTask;
    }
}

