using MegaHairPro.Desktop.ViewModels;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace MegaHairPro.Desktop;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm = new MainViewModel(App.Services.GetRequiredService<AtendimentoService>());
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _vm.CarregarInicialAsync();
            ListaAgenda.ItemsSource = _vm.AgendaHoje;
            CboServicoAdd.ItemsSource = _vm.ServicosDisponiveis;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao conectar ao banco:\n{ex.Message}", "MegaHair Pro",
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
    {
        try
        {
            await _vm.AtualizarAgendaAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnNovoWalkIn_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new WalkInWindow(_vm) { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            AtualizarPainel();
        }
    }

    private void BtnAdicionarServico_Click(object sender, RoutedEventArgs e)
    {
        _vm.AdicionarServico();
    }

    private async void BtnSalvarServicos_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _vm.SalvarServicosAsync();
            MessageBox.Show("Serviços salvos!", "MegaHair Pro", MessageBoxButton.OK, MessageBoxImage.Information);
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
            MessageBox.Show($"Sinal de R$ {_vm.ValorSinal:N2} registrado!", "Sucesso",
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
            MessageBox.Show("Atendimento concluído!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
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
            MessageBox.Show("Atendimento concluído!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
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
}
