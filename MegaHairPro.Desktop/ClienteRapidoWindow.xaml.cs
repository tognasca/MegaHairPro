using MegaHairPro.Desktop.ViewModels;
using MegaHairPro.Web.Models;
using System.Windows;

namespace MegaHairPro.Desktop;

public partial class ClienteRapidoWindow : Window
{
    private readonly MainViewModel _vm;
    public Cliente? ClienteCriado { get; private set; }

    public ClienteRapidoWindow(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        Loaded += (_, _) => TxtNome.Focus();
    }

    private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ClienteCriado = await _vm.CriarClienteRapidoAsync(
                TxtNome.Text, TxtTelefone.Text, TxtWhatsApp.Text, TxtObs.Text);
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
