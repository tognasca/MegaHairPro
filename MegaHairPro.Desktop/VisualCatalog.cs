namespace MegaHairPro.Desktop;

/// <summary>Ícones e cores por categoria / forma de pagamento — recepção rápida, sem lista densa.</summary>
public static class VisualCatalog
{
    public static string IconeCategoria(object? categoria)
    {
        var key = (categoria?.ToString() ?? "").Trim();
        return key.ToLowerInvariant() switch
        {
            "cabelo" or "1" => "💇",
            "unha" or "2" => "💅",
            "tratamento" or "3" => "✨",
            "megahair" or "mega hair" or "4" => "👑",
            "maquiagem" or "5" => "💄",
            _ => "◇"
        };
    }

    public static string CorFundoCategoria(object? categoria)
    {
        var key = (categoria?.ToString() ?? "").Trim();
        return key.ToLowerInvariant() switch
        {
            "cabelo" or "1" => "#F8E8EE",
            "unha" or "2" => "#E0F2FE",
            "tratamento" or "3" => "#ECFDF3",
            "megahair" or "mega hair" or "4" => "#FFFBEB",
            "maquiagem" or "5" => "#F3E2D3",
            _ => "#F3EAE7"
        };
    }

    public static string IconePagamento(object? forma)
    {
        var key = (forma?.ToString() ?? "").Trim();
        return key.ToUpperInvariant() switch
        {
            "PIX" => "⚡",
            "DINHEIRO" => "💵",
            "CARTÃO DÉBITO" or "CARTAO DEBITO" or "DÉBITO" or "DEBITO" => "💳",
            "CARTÃO CRÉDITO" or "CARTAO CREDITO" or "CRÉDITO" or "CREDITO" => "💎",
            _ => "●"
        };
    }

    public static string DescricaoPagamento(object? forma)
    {
        var key = (forma?.ToString() ?? "").Trim();
        return key.ToUpperInvariant() switch
        {
            "PIX" => "Na hora, sem taxa",
            "DINHEIRO" => "Espécie no caixa",
            "CARTÃO DÉBITO" or "CARTAO DEBITO" => "Débito na maquininha",
            "CARTÃO CRÉDITO" or "CARTAO CREDITO" => "Crédito na maquininha",
            _ => "Toque para escolher"
        };
    }
}
