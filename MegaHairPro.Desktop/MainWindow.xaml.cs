using MegaHairPro.Desktop.ViewModels;
using MegaHairPro.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MegaHairPro.Desktop;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;
    private readonly DispatcherTimer _relogio = new() { Interval = TimeSpan.FromSeconds(15) };
    private readonly DispatcherTimer _autoRefresh = new() { Interval = TimeSpan.FromSeconds(45) };

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm = new MainViewModel(App.Services.GetRequiredService<AtendimentoService>());
        Loaded += MainWindow_Loaded;
        Closed += (_, _) =>
        {
            _relogio.Stop();
            _autoRefresh.Stop();
        };
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _relogio.Tick += (_, _) => _vm.TickRelogio();
        _autoRefresh.Tick += async (_, _) =>
        {
            try { await _vm.AtualizarAgendaAsync(); }
            catch { /* silencioso — recepção não pode travar */ }
        };
        _relogio.Start();
        _autoRefresh.Start();
        _vm.TickRelogio();

        try
        {
            await _vm.CarregarInicialAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao conectar ao banco:\n{ex.Message}", "Tati Mega Hair",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AtualizarPainel()
    {
        var tem = _vm.TemAtendimento;
        PainelVazio.Visibility = tem ? Visibility.Collapsed : Visibility.Visible;
        PainelAtendimento.Visibility = tem ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void ListaAgenda_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListaAgenda.SelectedItem is not AgendaItem item) return;

        try
        {
            await _vm.SelecionarAtendimentoAsync(item.AgendamentoId);
            AtualizarPainel();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        => await ComSeguranca(_vm.AtualizarAgendaAsync);

    private async void TxtBusca_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await ComSeguranca(_vm.AtualizarAgendaAsync);
    }

    private async void FiltroProfissional_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        await ComSeguranca(_vm.AtualizarAgendaAsync);
    }

    private async void BtnDiaAnterior_Click(object sender, RoutedEventArgs e)
        => await ComSeguranca(() => _vm.MudarDiaAsync(-1));

    private async void BtnDiaProximo_Click(object sender, RoutedEventArgs e)
        => await ComSeguranca(() => _vm.MudarDiaAsync(1));

    private async void BtnHoje_Click(object sender, RoutedEventArgs e)
        => await ComSeguranca(_vm.IrParaHojeAsync);

    private async void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        => await ComSeguranca(_vm.AtualizarAgendaAsync);

    private void BtnNovoWalkIn_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new WalkInWindow(_vm) { Owner = this };
        if (dlg.ShowDialog() == true)
            AtualizarPainel();
    }

    private void BtnAdicionarServico_Click(object sender, RoutedEventArgs e)
        => _vm.AdicionarServico();

    private void BtnCatalogoServico_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is MegaHairPro.Web.Models.Servico servico)
            _vm.AdicionarServico(servico);
    }

    private void Pagamento_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string forma })
            _vm.FormaPagamento = forma;
    }

    private void BtnMarcarTodos_Click(object sender, RoutedEventArgs e)
        => _vm.MarcarTodosRealizados();

    private async void BtnSalvarServicos_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _vm.SalvarServicosAsync();
            MessageBox.Show("Serviços salvos.", "Tati Mega Hair", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnRemoverServico_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ServicoAtendimentoItem item)
            _vm.RemoverServico(item);
    }

    private async void BtnChegada_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _vm.MarcarChegadaAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show($"Cancelar o horário de {_vm.ClienteNome}?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            await _vm.CancelarAtendimentoAsync();
            ListaAgenda.SelectedItem = null;
            AtualizarPainel();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnWhatsApp_Click(object sender, RoutedEventArgs e)
    {
        var digits = Regex.Replace(_vm.ClienteTelefone ?? "", @"\D", "");
        if (digits.Length < 10)
        {
            MessageBox.Show("Telefone incompleto para WhatsApp.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!digits.StartsWith("55"))
            digits = "55" + digits;

        Process.Start(new ProcessStartInfo($"https://wa.me/{digits}") { UseShellExecute = true });
    }

    private async void BtnReceberSinal_Click(object sender, RoutedEventArgs e)
    {
        if (!_vm.PodeReceberSinal)
        {
            MessageBox.Show("Sinal já pago ou não aplicável.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            await _vm.ReceberSinalAsync();
            MessageBox.Show($"Sinal de R$ {_vm.ValorSinal:N2} registrado.", "Sucesso",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnFecharRestante_Click(object sender, RoutedEventArgs e)
    {
        if (!_vm.PodeFechar) return;

        var msg = _vm.SinalPago
            ? $"Receber restante de R$ {_vm.ValorRestante:N2} via {_vm.FormaPagamento}?"
            : $"Fechar atendimento e receber R$ {_vm.ValorRestante:N2}?";

        if (MessageBox.Show(msg, "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            await _vm.FecharAtendimentoAsync(pagarSeparado: true);
            MessageBox.Show("Atendimento concluído.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void BtnFecharTotal_Click(object sender, RoutedEventArgs e)
    {
        if (!_vm.PodeFechar) return;

        if (MessageBox.Show($"Receber valor total de R$ {_vm.ValorTotal:N2} via {_vm.FormaPagamento}?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            await _vm.FecharAtendimentoAsync(pagarSeparado: false);
            MessageBox.Show("Atendimento concluído.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnLimpar_Click(object sender, RoutedEventArgs e)
    {
        _vm.LimparAtendimento();
        ListaAgenda.SelectedItem = null;
        AtualizarPainel();
    }

    private static async Task ComSeguranca(Func<Task> acao)
    {
        try
        {
            await acao();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnNovoCliente_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ClienteRapidoWindow(_vm) { Owner = this };
        if (dlg.ShowDialog() == true && dlg.ClienteCriado != null)
        {
            _vm.Clientes.Insert(0, dlg.ClienteCriado);
            //CboCliente.ItemsSource = null;
            //CboCliente.ItemsSource = _vm.Clientes;
            //CboCliente.SelectedItem = dlg.ClienteCriado;
            _vm.ClienteWalkIn = dlg.ClienteCriado;
        }
    }
}
