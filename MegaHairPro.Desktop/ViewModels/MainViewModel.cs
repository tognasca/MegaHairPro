using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

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
    public int ProfissionalId { get; set; }
    public DateTime DataHora { get; set; }
    public string ClienteNome { get; set; } = "";
    public string Inicial { get; set; } = "?";
    public string Horario { get; set; } = "";
    public string Periodo { get; set; } = "";
    public string Status { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string ServicosResumo { get; set; } = "";
    public string ProfissionalNome { get; set; } = "";
    public string CorStatus { get; set; } = "#C9A227";
    public string CorFundo { get; set; } = "#FFF8F0";
    public bool IsAtrasado { get; set; }
    public string ValorResumo { get; set; } = "";
}

public class FiltroProfissionalItem
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
}

public class MainViewModel : INotifyPropertyChanged
{
    private readonly AtendimentoService _atendimento;
    private static readonly CultureInfo PtBr = new("pt-BR");

    public ObservableCollection<AgendaItem> AgendaHoje { get; } = [];
    public ObservableCollection<ServicoAtendimentoItem> ServicosAtendimento { get; } = [];
    public ObservableCollection<Cliente> Clientes { get; } = [];
    public ObservableCollection<Servico> ServicosDisponiveis { get; } = [];
    public ObservableCollection<Profissional> Profissionais { get; } = [];
    public ObservableCollection<FiltroProfissionalItem> FiltrosProfissional { get; } = [];
    public ObservableCollection<Servico> CatalogoServicos { get; } = [];

    public string[] FormasPagamento { get; } = ["PIX", "Dinheiro", "Cartão Débito", "Cartão Crédito"];

    private string _busca = "";
    public string Busca
    {
        get => _busca;
        set { _busca = value; OnPropertyChanged(); }
    }

    private DateTime _dataAgenda = DateTime.Today;
    public DateTime DataAgenda
    {
        get => _dataAgenda;
        set
        {
            _dataAgenda = value.Date;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DataAgendaTexto));
            OnPropertyChanged(nameof(EhHoje));
        }
    }

    public string DataAgendaTexto => DataAgenda.ToString("dddd, dd 'de' MMMM", PtBr);
    public bool EhHoje => DataAgenda == DateTime.Today;

    private int _filtroProfissionalId;
    public int FiltroProfissionalId
    {
        get => _filtroProfissionalId;
        set { _filtroProfissionalId = value; OnPropertyChanged(); }
    }

    private string _relogio = DateTime.Now.ToString("HH:mm");
    public string Relogio
    {
        get => _relogio;
        set { _relogio = value; OnPropertyChanged(); }
    }

    private string _clienteNome = "";
    public string ClienteNome
    {
        get => _clienteNome;
        set { _clienteNome = value; OnPropertyChanged(); OnPropertyChanged(nameof(InicialCliente)); }
    }

    public string InicialCliente =>
        string.IsNullOrWhiteSpace(ClienteNome) ? "?" : ClienteNome.Trim()[0].ToString().ToUpperInvariant();

    private string _clienteTelefone = "";
    public string ClienteTelefone
    {
        get => _clienteTelefone;
        set { _clienteTelefone = value; OnPropertyChanged(); }
    }

    private string _clienteObservacoes = "";
    public string ClienteObservacoes
    {
        get => _clienteObservacoes;
        set { _clienteObservacoes = value; OnPropertyChanged(); }
    }

    private string _profissionalPrincipal = "";
    public string ProfissionalPrincipal
    {
        get => _profissionalPrincipal;
        set { _profissionalPrincipal = value; OnPropertyChanged(); }
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
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PodeReceberSinal));
            OnPropertyChanged(nameof(PodeFechar));
            OnPropertyChanged(nameof(PodeCheckIn));
            OnPropertyChanged(nameof(PodeCancelar));
            OnPropertyChanged(nameof(PodeEditarServicos));
            OnPropertyChanged(nameof(StatusCor));
            OnPropertyChanged(nameof(StatusLabelAmigavel));
        }
    }

    public string StatusCor => Status switch
    {
        "Confirmado" => "#15803D",
        "Concluido" => "#1D4ED8",
        _ => "#C9A227"
    };

    private string _alertaAnamnese = "";
    public string AlertaAnamnese
    {
        get => _alertaAnamnese;
        set { _alertaAnamnese = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemAlertaAnamnese)); }
    }

    public bool TemAlertaAnamnese => !string.IsNullOrWhiteSpace(AlertaAnamnese);

    private decimal _valorTotal;
    public decimal ValorTotal
    {
        get => _valorTotal;
        set
        {
            _valorTotal = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ValorJaNoCaixa));
            OnPropertyChanged(nameof(ValorAReceber));
        }
    }

    private decimal _valorSinal;
    public decimal ValorSinal
    {
        get => _valorSinal;
        set
        {
            _valorSinal = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SinalTexto));
            OnPropertyChanged(nameof(ValorJaNoCaixa));
            OnPropertyChanged(nameof(ValorAReceber));
        }
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
        set
        {
            _sinalPago = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SinalTexto));
            OnPropertyChanged(nameof(PodeReceberSinal));
            OnPropertyChanged(nameof(ValorJaNoCaixa));
            OnPropertyChanged(nameof(ValorAReceber));
        }
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

    public Profissional? ProfissionalWalkIn { get; set; }
    public Cliente? ClienteWalkIn { get; set; }

    private int _kpiTotal;
    public int KpiTotal { get => _kpiTotal; set { _kpiTotal = value; OnPropertyChanged(); } }
    private int _kpiAguardando;
    public int KpiAguardando { get => _kpiAguardando; set { _kpiAguardando = value; OnPropertyChanged(); } }
    private int _kpiConfirmados;
    public int KpiConfirmados { get => _kpiConfirmados; set { _kpiConfirmados = value; OnPropertyChanged(); } }
    private int _kpiConcluidos;
    public int KpiConcluidos { get => _kpiConcluidos; set { _kpiConcluidos = value; OnPropertyChanged(); } }
    private decimal _kpiFaturado;
    public decimal KpiFaturado { get => _kpiFaturado; set { _kpiFaturado = value; OnPropertyChanged(); } }

    public bool TemAtendimento => AgendamentoId.HasValue;
    public bool PodeReceberSinal => TemAtendimento && !SinalPago && ValorSinal > 0 && Status != "Concluido";
    public bool PodeFechar => TemAtendimento && !RestantePago && Status != "Cancelado";
    public bool PodeCheckIn => TemAtendimento && Status == "Agendado";
    public bool PodeCancelar => TemAtendimento && Status != "Concluido";
    public bool PodeEditarServicos => TemAtendimento && Status != "Concluido";
    public string SinalTexto => SinalPago ? $"Sinal R$ {ValorSinal:N2} · pago" : $"Sinal R$ {ValorSinal:N2} · pendente";
    public string RestanteTexto => RestantePago ? $"Restante R$ {ValorRestante:N2} · pago" : $"Restante R$ {ValorRestante:N2} · pendente";
    public decimal ValorJaNoCaixa => SinalPago ? ValorSinal : 0;
    public decimal ValorAReceber => Math.Max(0, ValorTotal - ValorJaNoCaixa);
    public string StatusLabelAmigavel => Status switch
    {
        "Confirmado" => "Na cadeira",
        "Concluido" => "Concluído",
        "Agendado" => "Aguardando",
        _ => Status
    };

    public MainViewModel(AtendimentoService atendimento)
    {
        _atendimento = atendimento;
        ServicosAtendimento.CollectionChanged += (_, _) => AtualizarCatalogo();
    }

    public void TickRelogio()
        => Relogio = DateTime.Now.ToString("HH:mm");

    private readonly SemaphoreSlim _carregamentoLock = new(1, 1);

    public async Task CarregarInicialAsync()
    {
        await _carregamentoLock.WaitAsync();

        try
        {
            var profs = await _atendimento.ListarProfissionaisAsync();

            Profissionais.Clear();
            FiltrosProfissional.Clear();

            FiltrosProfissional.Add(
                new FiltroProfissionalItem
                {
                    Id = 0,
                    Nome = "Todas as profissionais"
                });

            foreach (var p in profs)
            {
                Profissionais.Add(p);

                FiltrosProfissional.Add(
                    new FiltroProfissionalItem
                    {
                        Id = p.Id,
                        Nome = p.Nome
                    });
            }

            var servs = await _atendimento.ListarServicosAsync();

            ServicosDisponiveis.Clear();

            foreach (var s in servs)
                ServicosDisponiveis.Add(s);

            AtualizarCatalogo();
            await AtualizarAgendaAsync();
        }
        finally
        {
            _carregamentoLock.Release();
        }
    }

    public async Task AtualizarAgendaAsync()
    {
        var lista = string.IsNullOrWhiteSpace(Busca)
            ? await _atendimento.ListarAgendaPorDataAsync(DataAgenda)
            : await _atendimento.BuscarClienteAsync(Busca);

        if (FiltroProfissionalId > 0)
            lista = lista.Where(a => a.ProfissionalId == FiltroProfissionalId).ToList();

        var agora = DateTime.Now;
        var proximoId = lista
            .Where(a => a.Status == StatusAgendamento.Agendado && a.DataHora >= agora)
            .OrderBy(a => a.DataHora)
            .Select(a => (int?)a.AgendamentoId)
            .FirstOrDefault();

        AgendaHoje.Clear();
        foreach (var a in lista)
        {
            var atrasado = a.Status == StatusAgendamento.Agendado && a.DataHora < agora && a.DataHora.Date == DateTime.Today;
            var (cor, fundo, label) = a.Status switch
            {
                StatusAgendamento.Confirmado => ("#15803D", "#ECFDF3", "Na cadeira"),
                StatusAgendamento.Concluido => ("#1D4ED8", "#EFF6FF", "Concluído"),
                _ when atrasado => ("#B91C1C", "#FEF2F2", "Atrasada"),
                _ => ("#C9A227", "#FFFBEB", "Aguardando")
            };

            AgendaHoje.Add(new AgendaItem
            {
                AgendamentoId = a.AgendamentoId,
                ProfissionalId = a.ProfissionalId,
                DataHora = a.DataHora,
                ClienteNome = a.ClienteNome,
                Inicial = string.IsNullOrWhiteSpace(a.ClienteNome) ? "?" : a.ClienteNome.Trim()[0].ToString().ToUpperInvariant(),
                Horario = a.WalkIn ? "Walk-in" : a.DataHora.ToString("HH:mm"),
                Periodo = a.WalkIn ? "sem horário" : $"{a.DataHora:HH:mm} – {a.DataFim:HH:mm}",
                Status = a.Status.ToString(),
                StatusLabel = a.AgendamentoId == proximoId ? "Próxima" : label,
                ServicosResumo = string.Join(" · ", a.Servicos.Select(s => s.Nome)),
                ProfissionalNome = a.ProfissionalPrincipal,
                CorStatus = cor,
                CorFundo = fundo,
                IsAtrasado = atrasado,
                ValorResumo = $"R$ {a.ValorTotal:N0}"
            });
        }

        if (string.IsNullOrWhiteSpace(Busca))
        {
            var resumo = await _atendimento.ObterResumoDiaAsync(DataAgenda);
            KpiTotal = resumo.Total;
            KpiAguardando = resumo.Aguardando;
            KpiConfirmados = resumo.Confirmados;
            KpiConcluidos = resumo.Concluidos;
            KpiFaturado = resumo.Faturado;
        }
    }

    public async Task IrParaHojeAsync()
    {
        DataAgenda = DateTime.Today;
        Busca = "";
        await AtualizarAgendaAsync();
    }

    public async Task MudarDiaAsync(int delta)
    {
        DataAgenda = DataAgenda.AddDays(delta);
        Busca = "";
        await AtualizarAgendaAsync();
    }

    public async Task SelecionarAtendimentoAsync(int agendamentoId)
    {
        var at = await _atendimento.ObterAtendimentoAsync(agendamentoId);
        if (at == null) return;

        AgendamentoId = at.AgendamentoId;
        ClienteId = at.ClienteId;
        ClienteNome = at.ClienteNome;
        ClienteTelefone = at.ClienteTelefone ?? "";
        ClienteObservacoes = at.ClienteObservacoes ?? "";
        ProfissionalPrincipal = at.ProfissionalPrincipal;
        Horario = at.WalkIn
            ? $"Walk-in · {at.DataHora:dd/MM HH:mm}"
            : $"{at.DataHora:dddd, dd/MM · HH:mm} – {at.DataFim:HH:mm}";
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

        var ana = await _atendimento.ObterAnamneseAsync(at.ClienteId);
        if (ana.TemAlerta)
        {
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(ana.Alergias)) partes.Add($"Alergias: {ana.Alergias}");
            if (!string.IsNullOrWhiteSpace(ana.Restricoes)) partes.Add($"Restrições: {ana.Restricoes}");
            AlertaAnamnese = string.Join("  ·  ", partes);
        }
        else
        {
            AlertaAnamnese = ana.TemAnamnese ? "" : "Sem ficha de anamnese cadastrada.";
            if (!ana.TemAnamnese) AlertaAnamnese = "";
        }

        OnPropertyChanged(nameof(PodeReceberSinal));
        OnPropertyChanged(nameof(PodeFechar));
        OnPropertyChanged(nameof(PodeCheckIn));
        OnPropertyChanged(nameof(PodeCancelar));
        OnPropertyChanged(nameof(PodeEditarServicos));
        AtualizarCatalogo();
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

    public void AtualizarCatalogo()
    {
        var ids = ServicosAtendimento.Select(s => s.ServicoId).ToHashSet();
        CatalogoServicos.Clear();
        foreach (var s in ServicosDisponiveis.Where(s => !ids.Contains(s.Id)))
            CatalogoServicos.Add(s);
    }

    public async Task AdicionarServicoAsync(Servico? servico = null)
    {
        if (Status == "Concluido")
            throw new InvalidOperationException("Atendimento já concluído.");

        var origem = servico ?? ServicoParaAdicionar;
        if (origem == null) return;
        if (ServicosAtendimento.Any(s => s.ServicoId == origem.Id)) return;

        ServicosAtendimento.Add(new ServicoAtendimentoItem
        {
            ServicoId = origem.Id,
            Nome = origem.Nome,
            Categoria = origem.Categoria.ToString(),
            Valor = origem.ValorFinal,
            ProfissionalId = Profissionais.FirstOrDefault()?.Id,
            Realizado = false
        });

        RecalcularTotaisLocais();
        ServicoParaAdicionar = null;
        AtualizarCatalogo();

        if (AgendamentoId.HasValue)
            await SalvarServicosAsync();
    }

    public async Task AdicionarServicoPorIdAsync(int servicoId)
    {
        var servico = ServicosDisponiveis.FirstOrDefault(s => s.Id == servicoId);
        if (servico == null) return;
        await AdicionarServicoAsync(servico);
    }

    public async Task RemoverServicoAsync(ServicoAtendimentoItem item)
    {
        ServicosAtendimento.Remove(item);
        RecalcularTotaisLocais();
        AtualizarCatalogo();
        if (AgendamentoId.HasValue)
            await SalvarServicosAsync();
    }

    private void RecalcularTotaisLocais()
    {
        ValorTotal = ServicosAtendimento.Sum(s => s.Valor);
        ValorRestante = SinalPago ? Math.Max(0, ValorTotal - ValorSinal) : ValorTotal;
    }

    public void AdicionarServico(Servico? servico = null)
        => _ = AdicionarServicoAsync(servico);

    public void RemoverServico(ServicoAtendimentoItem item)
        => _ = RemoverServicoAsync(item);

    public void MarcarTodosRealizados()
    {
        foreach (var s in ServicosAtendimento)
            s.Realizado = true;
    }

    public async Task BuscarClientesAsync(string filtro)
    {
        var lista = await _atendimento.ListarClientesAsync(filtro);
        Clientes.Clear();
        foreach (var c in lista) Clientes.Add(c);
    }

    public async Task<Cliente> CriarClienteRapidoAsync(string nome, string? telefone, string? whatsapp, string? obs)
        => await _atendimento.CriarClienteRapidoAsync(nome, telefone, whatsapp, obs);

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
            s.ProfissionalId ??= ProfissionalWalkIn.Id;

        DataAgenda = DateTime.Today;
        await _atendimento.AtualizarServicosAsync(at.AgendamentoId, ServicosAtendimento.Select(x => x.ToDto()).ToList());
        await SelecionarAtendimentoAsync(at.AgendamentoId);
        await AtualizarAgendaAsync();
    }

    public async Task MarcarChegadaAsync()
    {
        if (!AgendamentoId.HasValue) return;
        await _atendimento.MarcarChegadaAsync(AgendamentoId.Value);
        await SelecionarAtendimentoAsync(AgendamentoId.Value);
        await AtualizarAgendaAsync();
    }

    public async Task CancelarAtendimentoAsync()
    {
        if (!AgendamentoId.HasValue) return;
        await _atendimento.CancelarAsync(AgendamentoId.Value);
        LimparAtendimento();
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

        MarcarTodosRealizados();
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
        ClienteObservacoes = "";
        ProfissionalPrincipal = "";
        Horario = "";
        Status = "";
        AlertaAnamnese = "";
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
        OnPropertyChanged(nameof(PodeCheckIn));
        OnPropertyChanged(nameof(PodeCancelar));
        OnPropertyChanged(nameof(PodeEditarServicos));
        AtualizarCatalogo();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
