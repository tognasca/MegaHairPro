using MegaHairPro.Desktop.ViewModels;
using MegaHairPro.Web.Models;
using System.Windows;

namespace MegaHairPro.Desktop;

public partial class WalkInWindow : Window
{
    private readonly MainViewModel _vm;

    public WalkInWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        CboProfissional.ItemsSource = _vm.Profissionais;
        LstServicos.ItemsSource = _vm.ServicosDisponiveis;
        if (_vm.Profissionais.Any())
            CboProfissional.SelectedIndex = 0;
    }

    private async void BtnBuscarCliente_Click(object sender, RoutedEventArgs e)
    {
        await _vm.BuscarClientesAsync(TxtFiltroCliente.Text);
        CboCliente.ItemsSource = _vm.Clientes;
        if (_vm.Clientes.Any())
            CboCliente.SelectedIndex = 0;
    }

    private void CboCliente_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        _vm.ClienteWalkIn = CboCliente.SelectedItem as Cliente;
    }

    private async void BtnIniciar_Click(object sender, RoutedEventArgs e)
    {
        _vm.ClienteWalkIn = CboCliente.SelectedItem as Cliente;
        _vm.ProfissionalWalkIn = CboProfissional.SelectedItem as Profissional;

        _vm.ServicosAtendimento.Clear();
        foreach (Servico s in LstServicos.SelectedItems)
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
