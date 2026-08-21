using MegaHairPro.Desktop.ViewModels;
using MegaHairPro.Web.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace MegaHairPro.Desktop;

public partial class WalkInWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly HashSet<int> _servicosMarcados = [];
    private readonly DispatcherTimer _buscaTimer = new() { Interval = TimeSpan.FromMilliseconds(280) };
    private int _buscaSeq;

    public WalkInWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        CboProfissional.ItemsSource = _vm.Profissionais;
        LstServicos.ItemsSource = _vm.ServicosDisponiveis;
        LstClientes.ItemsSource = _vm.Clientes;
        if (_vm.Profissionais.Any())
            CboProfissional.SelectedIndex = 0;

        _buscaTimer.Tick += async (_, _) =>
        {
            _buscaTimer.Stop();
            await BuscarClientesAsync();
        };

        Loaded += WalkInWindow_Loaded;
    }

    private async void WalkInWindow_Loaded(object sender, RoutedEventArgs e)
    {
        AjustarAoMonitor();
        await BuscarClientesAsync();
    }

    private void AjustarAoMonitor()
    {
        var origem = Owner ?? this;
        var area = SystemParameters.WorkArea;
        if (origem.ActualWidth > 0 && origem.ActualHeight > 0)
        {
            Width = Math.Max(MinWidth, origem.ActualWidth * 0.82);
            Height = Math.Max(MinHeight, origem.ActualHeight * 0.88);
        }
        else
        {
            Width = Math.Max(MinWidth, area.Width * 0.82);
            Height = Math.Max(MinHeight, area.Height * 0.88);
        }

        Width = Math.Min(Width, area.Width);
        Height = Math.Min(Height, area.Height);
        Left = area.Left + (area.Width - Width) / 2;
        Top = area.Top + (area.Height - Height) / 2;
    }

    private void TxtFiltroCliente_TextChanged(object sender, TextChangedEventArgs e)
    {
        _buscaTimer.Stop();
        _buscaTimer.Start();
    }

    private async void TxtFiltroCliente_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        _buscaTimer.Stop();
        await BuscarClientesAsync();
    }

    private async Task BuscarClientesAsync()
    {
        var seq = ++_buscaSeq;
        TxtStatusClientes.Text = "Buscando…";
        try
        {
            await _vm.BuscarClientesAsync(TxtFiltroCliente.Text);
            if (seq != _buscaSeq) return;

            LstClientes.ItemsSource = null;
            LstClientes.ItemsSource = _vm.Clientes;

            if (_vm.Clientes.Count == 0)
            {
                TxtStatusClientes.Text = "Nenhuma cliente encontrada. Toque em + Nova para cadastrar.";
                return;
            }

            TxtStatusClientes.Text = _vm.Clientes.Count == 1
                ? "1 cliente — toque para selecionar"
                : $"{_vm.Clientes.Count} clientes — toque para selecionar";

            if (_vm.ClienteWalkIn != null)
            {
                var match = _vm.Clientes.FirstOrDefault(c => c.Id == _vm.ClienteWalkIn.Id);
                if (match != null)
                    LstClientes.SelectedItem = match;
            }
            else if (LstClientes.SelectedItem == null && _vm.Clientes.Count > 0)
            {
                LstClientes.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            TxtStatusClientes.Text = "Não foi possível carregar as clientes.";
            MessageBox.Show(ex.Message, "Erro ao buscar clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void LstClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _vm.ClienteWalkIn = LstClientes.SelectedItem as Cliente;
    }

    private async void BtnNovaCliente_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ClienteRapidoWindow(_vm) { Owner = this };
        if (dlg.ShowDialog() == true && dlg.ClienteCriado != null)
        {
            _vm.Clientes.Insert(0, dlg.ClienteCriado);
            LstClientes.ItemsSource = null;
            LstClientes.ItemsSource = _vm.Clientes;
            LstClientes.SelectedItem = dlg.ClienteCriado;
            _vm.ClienteWalkIn = dlg.ClienteCriado;
            TxtStatusClientes.Text = "Cliente cadastrada — pronta para atender.";
        }
    }

    private void ServicoTile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton btn)
            return;

        Servico? servico = btn.Tag switch
        {
            Servico s => s,
            int id => _vm.ServicosDisponiveis.FirstOrDefault(x => x.Id == id),
            _ => null
        };
        if (servico == null) return;

        if (btn.IsChecked == true)
            _servicosMarcados.Add(servico.Id);
        else
            _servicosMarcados.Remove(servico.Id);

        AtualizarResumoServicos();
    }

    private void AtualizarResumoServicos()
    {
        var escolhidos = _vm.ServicosDisponiveis.Where(s => _servicosMarcados.Contains(s.Id)).ToList();
        if (escolhidos.Count == 0)
        {
            TxtResumoServicos.Text = "";
            return;
        }

        var total = escolhidos.Sum(s => s.ValorFinal);
        TxtResumoServicos.Text = escolhidos.Count == 1
            ? $"1 serviço · R$ {total:N0}"
            : $"{escolhidos.Count} serviços · R$ {total:N0}";
    }

    private async void BtnIniciar_Click(object sender, RoutedEventArgs e)
    {
        _vm.ClienteWalkIn = LstClientes.SelectedItem as Cliente;
        _vm.ProfissionalWalkIn = CboProfissional.SelectedItem as Profissional;

        _vm.ServicosAtendimento.Clear();
        foreach (var s in _vm.ServicosDisponiveis.Where(x => _servicosMarcados.Contains(x.Id)))
        {
            _vm.ServicosAtendimento.Add(new ServicoAtendimentoItem
            {
                ServicoId = s.Id,
                Nome = s.Nome,
                Categoria = s.Categoria.ToString(),
                Valor = s.ValorFinal,
                ProfissionalId = _vm.ProfissionalWalkIn?.Id,
                Realizado = false
            });
        }

        try
        {
            await _vm.CriarWalkInAsync();
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
