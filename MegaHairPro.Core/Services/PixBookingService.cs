using Microsoft.Extensions.Configuration;
using System;
using System.Text;
namespace MegaHairPro.Web.Services;

public class PixBookingService
{
    private readonly IConfiguration _config;

    public PixBookingService(IConfiguration config) => _config = config;

    public PagamentoPixResult CriarPagamento(decimal valor, string descricao, string txid)
    {
        var chave = _config["Booking:PixChave"] ?? _config["PIX_CHAVE"];
        var nome = _config["Booking:PixNome"] ?? _config["PIX_NOME"] ?? "Tati Mega Hair";
        var cidade = _config["Booking:PixCidade"] ?? _config["PIX_CIDADE"] ?? "SAO PAULO";

        if (string.IsNullOrWhiteSpace(chave))
        {
            return new PagamentoPixResult
            {
                Tipo = "pix_manual",
                Mensagem = "PIX não configurado. Configure Booking:PixChave no appsettings.",
                Chave = null
            };
        }

        var payload = GerarPayloadEmv(valor, descricao, txid, chave, nome, cidade);
        var qrUrl = "https://api.qrserver.com/v1/create-qr-code/?size=220x220&data=" +
                    Uri.EscapeDataString(payload);

        return new PagamentoPixResult
        {
            Tipo = "pix_manual",
            PixCopiaECola = payload,
            QrCodeUrl = qrUrl,
            Chave = chave,
            Mensagem = "Pagamento via PIX"
        };
    }

    private static string GerarPayloadEmv(decimal valor, string descricao, string txid,
        string chave, string nome, string cidade)
    {
        string F(string id, string v) => id + (v.Length < 10 ? "0" + v.Length : v.Length.ToString()) + v;

        var sb = new StringBuilder();
        sb.Append(F("00", "01"));
        sb.Append(F("26",
            F("00", "br.gov.bcb.pix") +
            F("01", chave)));
        sb.Append(F("52", "0000"));
        sb.Append(F("53", "986"));
        sb.Append(F("54", valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)));
        sb.Append(F("58", "BR"));
        sb.Append(F("59", nome.Length > 25 ? nome[..25] : nome));
        sb.Append(F("60", cidade.Length > 15 ? cidade[..15] : cidade));
        if (!string.IsNullOrEmpty(txid))
            sb.Append(F("62", F("05", txid.Length > 25 ? txid[..25] : txid)));

        var semCrc = sb.ToString() + "6304";
        var crc = Crc16(semCrc);
        return semCrc + crc;
    }

    private static string Crc16(string payload)
    {
        ushort crc = 0xFFFF;
        foreach (var c in payload)
        {
            crc ^= (ushort)(c << 8);
            for (int i = 0; i < 8; i++)
                crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
        }
        return (crc & 0xFFFF).ToString("X4");
    }
}

public class PagamentoPixResult
{
    public string Tipo { get; set; } = "pix_manual";
    public string? PixCopiaECola { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? Chave { get; set; }
    public string? Mensagem { get; set; }
}
