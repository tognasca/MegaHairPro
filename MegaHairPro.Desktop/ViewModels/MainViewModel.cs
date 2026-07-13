using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MegaHairPro.Desktop.ViewModels;

public class ServicoAtendimentoItem : INotifyPropertyChanged
{
    public int ServicoId { get; set; }
    public string Nome { get; set; } = "";
    public string Categoria { get; set; } = "";
    public decimal Valor { get; set; }

    private int? _profissionalId;
    public int? ProfissionalId
    {
        get => _profissionalId;
        set { _profissionalId = value; OnPropertyChanged(); }
    }

    private bool _realizado;
    public bool Realizado
    {
        get => _realizado;
        set { _realizado = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public AtendimentoServicoDto ToDto() => new()
    {
        ServicoId = ServicoId,
        Nome = Nome,
        Categoria = Categoria,
        Valor = Valor,
        ProfissionalId = ProfissionalId,
        Realizado = Realizado
    };
}

public class AgendaItem
{
    public int AgendamentoId { get; set; }
    public string ClienteNome { get; set; } = "";
    public string Horario { get; set; } = "";
    public string Status { get; set; } = "";
    public string ServicosResumo { get; set; } = "";
    public string CorStatus { get; set; } = "#f59e0b";
}

public class MainViewModel : INotifyPropertyChanged
{
    private readonly AtendimentoService _atendimento;

    public ObservableCollection<AgendaItem> AgendaHoje { get; } = [];
    public ObservableCollection<ServicoAtendimentoItem> ServicosAtendimento { get; } = [];
    public ObservableCollection<Cliente> Clientes { get; } = [];
    public ObservableCollection<Servico> ServicosDisponiveis { get; } = [];
    public ObservableCollection<Profissional> Profissionais { get; } = [];

    public string[] FormasPagamento { get; } = ["Dinheiro", "PIX", "Cartão Débito", "Cartão Crédito"];

    private string _busca = "";
    public string Busca
    {
        get => _busca;
        set { _busca = value; OnPropertyChanged(); }
    }

    private string _clienteNome = "";
    public string ClienteNome
    {
        get => _clienteNome;
        set { _clienteNome = value; OnPropertyChanged(); }
    }

    private string _clienteTelefone = "";
    public string ClienteTelefone
    {
        get => _clienteTelefone;
        set { _clienteTelefone = value; OnPropertyChanged(); }
    }

    private string _horario = "";
    public string Horario
    {
        get => _horario;
        set { _horario = value; OnPropertyChanged(); }
    }

    private string _status = "";
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    private decimal _valorTotal;
    public decimal ValorTotal
    {
        get => _valorTotal;
        set { _valorTotal = value; OnPropertyChanged(); }
    }

    private decimal _valorSinal;
    public decimal ValorSinal
    {
        get => _valorSinal;
        set { _valorSinal = value; OnPropertyChanged(); OnPropertyChanged(nameof(SinalTexto)); }
    }

    private decimal _valorRestante;
    public decimal ValorRestante
    {
        get => _valorRestante;
        set { _valorRestante = value; OnPropertyChanged(); OnPropertyChanged(nameof(RestanteTexto)); }
    }

    private bool _sinalPago;
    public bool SinalPago
    {
        get => _sinalPago;
        set { _sinalPago = value; OnPropertyChanged(); OnPropertyChanged(nameof(SinalTexto)); OnPropertyChanged(nameof(PodeReceberSinal)); }
    }

    private bool _restantePago;
    public bool RestantePago
    {
        get => _restantePago;
        set { _restantePago = value; OnPropertyChanged(); OnPropertyChanged(nameof(RestanteTexto)); OnPropertyChanged(nameof(PodeFechar)); }
    }

    private int? _agendamentoId;
    public int? AgendamentoId
    {
        get => _agendamentoId;
        set { _agendamentoId = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemAtendimento)); }
    }

    private int? _clienteId;
    public int? ClienteId { get; set; }

    private string _formaPagamento = "PIX";
    public string FormaPagamento
    {
        get => _formaPagamento;
        set { _formaPagamento = value; OnPropertyChanged(); }
    }

    private Servico? _servicoParaAdicionar;
    public Servico? ServicoParaAdicionar
    {
        get => _servicoParaAdicionar;
        set { _servicoParaAdicionar = value; OnPropertyChanged(); }
    }

    private Profissional? _profissionalWalkIn;
    public Profissional? ProfissionalWalkIn
    {
        get => _profissionalWalkIn;
        set { _profissionalWalkIn = value; OnPropertyChanged(); }
    }

    private Cliente? _clienteWalkIn;
    public Cliente? ClienteWalkIn
    {
        get => _clienteWalkIn;
        set { _clienteWalkIn = value; OnPropertyChanged(); }
    }

    public bool TemAtendimento => AgendamentoId.HasValue;
    public bool PodeReceberSinal => TemAtendimento && !SinalPago && ValorSinal > 0 && Status != "Concluido";
    public bool PodeFechar => TemAtendimento && !RestantePago && Status != "Concluido";
    public string SinalTexto => SinalPago ? $"✓ Sinal R$ {ValorSinal:N2} — Pago" : $"○ Sinal R$ {ValorSinal:N2} — Pendente";
    public string RestanteTexto => RestantePago ? $"✓ Restante R$ {ValorRestante:N2} — Pago" : $"○ Restante R$ {ValorRestante:N2} — Pendente";

    public MainViewModel(AtendimentoService atendimento)
    {
        _atendimento = atendimento;
    }

    public async Task CarregarInicialAsync()
    {
        var profs = await _atendimento.ListarProfissionaisAsync();
        Profissionais.Clear();
        foreach (var p in profs) Profissionais.Add(p);

        var servs = await _atendimento.ListarServicosAsync();
        ServicosDisponiveis.Clear();
        foreach (var s in servs) ServicosDisponiveis.Add(s);

        await AtualizarAgendaAsync();
    }

    public async Task AtualizarAgendaAsync()
    {
        var lista = string.IsNullOrWhiteSpace(Busca)
            ? await _atendimento.ListarAgendaHojeAsync()
            : await _atendimento.BuscarClienteAsync(Busca);

        AgendaHoje.Clear();
        foreach (var a in lista)
        {
            AgendaHoje.Add(new AgendaItem
            {
                AgendamentoId = a.AgendamentoId,
                ClienteNome = a.ClienteNome,
                Horario = a.WalkIn ? "Walk-in" : a.DataHora.ToString("HH:mm"),
                Status = a.Status.ToString(),
                ServicosResumo = string.Join(", ", a.Servicos.Select(s => s.Nome)),
                CorStatus = a.Status switch
                {
                    StatusAgendamento.Confirmado => "#16a34a",
                    StatusAgendamento.Concluido => "#2563eb",
                    _ => "#f59e0b"
                }
            });
        }
    }

    public async Task SelecionarAtendimentoAsync(int agendamentoId)
    {
        var at = await _atendimento.ObterAtendimentoAsync(agendamentoId);
        if (at == null) return;

        AgendamentoId = at.AgendamentoId;
        ClienteId = at.ClienteId;
        ClienteNome = at.ClienteNome;
        ClienteTelefone = at.ClienteTelefone ?? "";
        Horario = at.WalkIn ? $"Walk-in — {at.DataHora:dd/MM HH:mm}" : $"{at.DataHora:dd/MM/yyyy HH:mm} — {at.DataFim:HH:mm}";
        Status = at.Status.ToString();
        ValorTotal = at.ValorTotal;
        ValorSinal = at.ValorSinal;
        ValorRestante = at.ValorRestante;
        SinalPago = at.SinalPago;
        RestantePago = at.RestantePago;

        ServicosAtendimento.Clear();
        foreach (var s in at.Servicos)
        {
            ServicosAtendimento.Add(new ServicoAtendimentoItem
            {
                ServicoId = s.ServicoId,
                Nome = s.Nome,
                Categoria = s.Categoria,
                Valor = s.Valor,
                ProfissionalId = s.ProfissionalId,
                Realizado = s.Realizado
            });
        }

        OnPropertyChanged(nameof(PodeReceberSinal));
        OnPropertyChanged(nameof(PodeFechar));
    }

    public async Task SalvarServicosAsync()
    {
        if (!AgendamentoId.HasValue) return;

        await _atendimento.AtualizarServicosAsync(
            AgendamentoId.Value,
            ServicosAtendimento.Select(s => s.ToDto()).ToList());

        await SelecionarAtendimentoAsync(AgendamentoId.Value);
        await AtualizarAgendaAsync();
    }

    public void AdicionarServico()
    {
        if (ServicoParaAdicionar == null) return;
        if (ServicosAtendimento.Any(s => s.ServicoId == ServicoParaAdicionar.Id)) return;

        ServicosAtendimento.Add(new ServicoAtendimentoItem
        {
            ServicoId = ServicoParaAdicionar.Id,
            Nome = ServicoParaAdicionar.Nome,
            Categoria = ServicoParaAdicionar.Categoria.ToString(),
            Valor = ServicoParaAdicionar.ValorFinal,
            ProfissionalId = Profissionais.FirstOrDefault()?.Id,
            Realizado = false
        });

        ValorTotal = ServicosAtendimento.Sum(s => s.Valor);
        if (!SinalPago)
            ValorRestante = ValorTotal - (SinalPago ? ValorSinal : 0);
    }

    public void RemoverServico(ServicoAtendimentoItem item)
    {
        ServicosAtendimento.Remove(item);
        ValorTotal = ServicosAtendimento.Sum(s => s.Valor);
    }

    public async Task BuscarClientesAsync(string filtro)
    {
        var lista = await _atendimento.ListarClientesAsync(filtro);
        Clientes.Clear();
        foreach (var c in lista) Clientes.Add(c);
    }

    public async Task CriarWalkInAsync()
    {
        if (ClienteWalkIn == null)
            throw new InvalidOperationException("Selecione a cliente.");
        if (ProfissionalWalkIn == null)
            throw new InvalidOperationException("Selecione a profissional principal.");
        if (!ServicosAtendimento.Any())
            throw new InvalidOperationException("Adicione ao menos um serviço.");

        var ids = ServicosAtendimento.Select(s => s.ServicoId).ToArray();
        var at = await _atendimento.CriarWalkInAsync(ClienteWalkIn.Id, ProfissionalWalkIn.Id, ids);

        foreach (var s in ServicosAtendimento)
        {
            s.ProfissionalId ??= ProfissionalWalkIn.Id;
        }

        await _atendimento.AtualizarServicosAsync(at.AgendamentoId, ServicosAtendimento.Select(x => x.ToDto()).ToList());
        await SelecionarAtendimentoAsync(at.AgendamentoId);
        await AtualizarAgendaAsync();
    }

    public async Task ReceberSinalAsync()
    {
        if (!AgendamentoId.HasValue) return;
        await SalvarServicosAsync();
        await _atendimento.RegistrarSinalAsync(AgendamentoId.Value, FormaPagamento);
        await SelecionarAtendimentoAsync(AgendamentoId.Value);
        await AtualizarAgendaAsync();
    }

    public async Task FecharAtendimentoAsync(bool pagarSeparado)
    {
        if (!AgendamentoId.HasValue) return;

        await SalvarServicosAsync();

        if (pagarSeparado && SinalPago && ValorSinal > 0 && !RestantePago)
            await _atendimento.RegistrarRestanteAsync(AgendamentoId.Value, FormaPagamento);
        else
            await _atendimento.RegistrarPagamentoTotalAsync(AgendamentoId.Value, FormaPagamento);

        await SelecionarAtendimentoAsync(AgendamentoId.Value);
        await AtualizarAgendaAsync();
    }

    public void LimparAtendimento()
    {
        AgendamentoId = null;
        ClienteId = null;
        ClienteNome = "";
        ClienteTelefone = "";
        Horario = "";
        Status = "";
        ValorTotal = 0;
        ValorSinal = 0;
        ValorRestante = 0;
        SinalPago = false;
        RestantePago = false;
        ServicosAtendimento.Clear();
        ClienteWalkIn = null;
        OnPropertyChanged(nameof(TemAtendimento));
        OnPropertyChanged(nameof(PodeReceberSinal));
        OnPropertyChanged(nameof(PodeFechar));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
