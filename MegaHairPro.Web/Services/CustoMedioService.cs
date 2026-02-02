
public class CustoMedioService
{
    public decimal Calcular(decimal qtdAtual, decimal custoAtual, decimal novaQtd, decimal novoValor)
    {
        var totalAtual = qtdAtual * custoAtual;
        var totalNovo = totalAtual + novoValor;
        var qtdTotal = qtdAtual + novaQtd;
        return qtdTotal == 0 ? 0 : totalNovo / qtdTotal;
    }
}
