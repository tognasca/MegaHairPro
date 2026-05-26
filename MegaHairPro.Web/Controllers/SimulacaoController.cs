using MegaHairPro.Web.Data;
using MegaHairPro.Web.Models;
using MegaHairPro.Web.Services;
using MegaHairPro.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MegaHairPro.Web.Controllers
{
    public class SimulacaoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IViewRenderService _viewRender;
        private readonly IPdfService _pdfService;
        private readonly ISimulacaoService _simulacao;
        private readonly ICustoMedioService _custoMedio;

        public SimulacaoController(
            AppDbContext context,
            IViewRenderService viewRender,
            IPdfService pdfService,
            ISimulacaoService simulacao,
            ICustoMedioService custoMedio)
        {
            _context = context;
            _viewRender = viewRender;
            _pdfService = pdfService;
            _simulacao = simulacao;
            _custoMedio = custoMedio;
        }

        // ✅ GET
        [HttpGet]
        public IActionResult Index()
        {
            CarregarViewBags();

            var lista = _context.Simulacao
                .Where(x => x.Status == true)
                .OrderByDescending(x => x.DataCadastro)
                .ToList();

            foreach (var sim in lista)
            {
                var produto = _context.Produtos
                    .FirstOrDefault(p => p.Id == sim.ProdutoId);

                sim.ProdutoNome = produto != null
                    ? $"{produto.TipoProduto} {produto.Origem}"
                    : string.Empty;

                sim.Cor = produto?.Cor;
                sim.Tamanho = produto?.Tamanho;
            }

            return View(lista);
        }

        // ✅ POST — parse manual de todos os decimais para evitar erro de cultura
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SimulacaoVenda model)
        {
            // Parse manual — campos que chegam com vírgula do JS
            model.PrecoVenda = ParseDecimal(Request.Form["PrecoVenda"]);
            model.PercentualLucroCabelo = ParseDecimal(Request.Form["PercentualLucroCabelo"]);
            model.PercentualDesconto = ParseDecimal(Request.Form["PercentualDesconto"]);
            model.TotalDespesasSelecionadas = ParseDecimal(Request.Form["TotalDespesasSelecionadas"]);

            // ================= DESPESAS =================
            decimal totalDespesas = 0;
            int mediaAtendimentosMes = 50;

            var despesasSelecionadas = _context.Despesas
                .Where(d => model.DespesasSelecionadas.Contains(d.Id))
                .ToList();

            foreach (var desp in despesasSelecionadas)
                totalDespesas += desp.Ratear ? desp.Valor / mediaAtendimentosMes : desp.Valor;

            model.DespesaSalaoRateada = totalDespesas;
            model.TotalDespesasSelecionadas = totalDespesas;

            // ================= SIMULAÇÃO =================
            SimulacaoVenda resultado;
            try
            {
                resultado = _simulacao.Calcular(model);
            }
            catch (Exception ex)
            {
                CarregarViewBags();
                ModelState.AddModelError("", ex.Message);
                var lista = _context.Simulacao.Where(x => x.Status == true)
                    .OrderByDescending(x => x.DataCadastro).ToList();
                return View(lista);
            }

            resultado.Status = true;
            resultado.DataCadastro = DateTime.Now;

            // ================= DADOS AUXILIARES =================
            var cliente = _context.Clientes.FirstOrDefault(c => c.Id == model.ClienteId);
            var produto = _context.Produtos.FirstOrDefault(p => p.Id == model.ProdutoId);
            var metodo = _context.MetodosAplicacao.FirstOrDefault(m => m.Id == model.MetodoId);

            resultado.ClienteNome = cliente?.Nome;
            resultado.ProdutoNome = produto?.Descricao;
            resultado.MetodoNome = metodo?.Nome;

            if (metodo != null)
                resultado.ValorTotalConfeccao = metodo.ValorPorGrama * model.QuantidadeGramas;

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Imagem/LogoMegaHairPro.png");
            resultado.LogoCaminho = logoPath;

            _context.Simulacao.Add(resultado);
            _context.SaveChanges();

            // PRG — evita double submit ao recarregar
            return RedirectToAction("Index");
        }


        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [Route("api/whatsapp/simulacao")]
        public IActionResult CalcularSimulacao([FromBody] WhatsAppSimulacaoModel model)
        {
            try
            {
                var produto = _context.Produtos
                    .FirstOrDefault(p => p.Id == model.ProdutoId);

                var metodo = _context.MetodosAplicacao
                    .FirstOrDefault(m => m.Id == model.MetodoId);

                if (produto == null)
                {
                    return BadRequest(
                        "Produto não encontrado"
                    );
                }

                if (metodo == null)
                {
                    return BadRequest(
                        "Método não encontrado"
                    );
                }

                decimal totalDespesas = 0;
                int mediaAtendimentosMes = 50;
                model.DespesasSelecionadas = _context.Despesas.Where(d => d.Ratear == true).Select(x => x.Id).ToList();
                var despesasSelecionadas = _context.Despesas.Where(d => model.DespesasSelecionadas.Contains(d.Id)).ToList();

                foreach (var desp in despesasSelecionadas)
                {
                    totalDespesas += desp.Ratear
                        ? desp.Valor / mediaAtendimentosMes
                        : desp.Valor;
                }

                var custoLucroCabelo = _custoMedio.CalculaCustoCabelo(model.ProdutoId,
                                                                      model.QuantidadeGramas, 70M);

                decimal custo = Convert.ToDecimal(custoLucroCabelo[0]["custo"]);
                decimal lucro = Convert.ToDecimal(custoLucroCabelo[0]["lucro"]);
                decimal confeccao = (metodo?.ValorPorGrama ?? 0M) * model.QuantidadeGramas;
                decimal precoOriginal = custo + lucro + confeccao + totalDespesas;


                // =====================================
                // CÁLCULOS
                // =====================================

                decimal valorCabelo = custo + lucro;
                decimal valorMetodo = confeccao;
                decimal valorFinal = valorCabelo + valorMetodo + totalDespesas;

                // =====================================
                // RETORNO
                // =====================================

                return Ok(new
                {
                    sucesso = true,
                    mensagem = $@"✨ *Simulação Mega Hair* ✨

💎 Cabelo:
{produto?.Origem} {produto?.Cor}

🪄 Método:
{metodo?.Nome ?? "Não encontrado"}

📏 Quantidade:
{model.QuantidadeGramas}g

💰 Valor total:
R$ {valorFinal:N2}

Se estiver de acordo responda:
*CONFIRMAR* 💖"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // ✅ EDIT AJAX — completo com todos os campos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAjax(SimulacaoVenda model)
        {
            var simulacao = _context.Simulacao.FirstOrDefault(x => x.Id == model.Id);
            if (simulacao == null) return NotFound();

            int mediaAtendimentosMes = 50;

            // ── Campos básicos ────────────────────────────────────────────────
            simulacao.ProdutoId = model.ProdutoId;
            simulacao.QuantidadeGramas = model.QuantidadeGramas;
            simulacao.MetodoId = model.MetodoId;

            // ── Percentuais ───────────────────────────────────────────────────
            simulacao.PercentualLucroCabelo = ParseDecimal(Request.Form["PercentualLucroCabelo"]);
            simulacao.PercentualComissaoVendedora = ParseDecimal(Request.Form["PercentualComissaoVendedora"]);
            simulacao.PercentualComissaoCabeleireira = ParseDecimal(Request.Form["PercentualComissaoCabeleireira"]);
            simulacao.PercentualDesconto = ParseDecimal(Request.Form["PercentualDesconto"]);

            // ── Valores calculados vindos do front (já calculados com a mesma lógica) ──
            simulacao.PrecoVenda = ParseDecimal(Request.Form["PrecoVenda"]);
            simulacao.CustoTotal = ParseDecimal(Request.Form["CustoTotal"]);
            simulacao.LucroLiquido = ParseDecimal(Request.Form["LucroLiquido"]);
            simulacao.LucroTotal = simulacao.LucroLiquido;
            simulacao.ValorDesconto = ParseDecimal(Request.Form["ValorDesconto"]);
            simulacao.ValorTotalConfeccao = ParseDecimal(Request.Form["ValorConfeccao"]);
            simulacao.ValorComissaoVendedora = ParseDecimal(Request.Form["ValorComissaoVendedora"]);
            simulacao.ValorComissaoCabeleireira = ParseDecimal(Request.Form["ValorComissaoCabeleireira"]);
            simulacao.TotalDespesasSelecionadas = ParseDecimal(Request.Form["TotalDespesas"]);

            // ── Despesas selecionadas ─────────────────────────────────────────
            var idsDespesas = Request.Form["DespesasSelecionadas"]
                .Select(v => int.TryParse(v, out var n) ? n : 0)
                .Where(n => n > 0)
                .ToList();

            decimal totalDespesas = 0;
            var despesas = _context.Despesas.Where(d => idsDespesas.Contains(d.Id)).ToList();
            foreach (var d in despesas)
                totalDespesas += d.Ratear ? d.Valor / mediaAtendimentosMes : d.Valor;

            simulacao.DespesaSalaoRateada = totalDespesas;
            simulacao.TotalDespesasSelecionadas = totalDespesas;

            // ── Dados de exibição ─────────────────────────────────────────────
            var produto = _context.Produtos.FirstOrDefault(p => p.Id == simulacao.ProdutoId);
            var metodo = _context.MetodosAplicacao.FirstOrDefault(m => m.Id == simulacao.MetodoId);

            simulacao.ProdutoNome = produto != null
                ? $"{produto.TipoProduto} {produto.Origem}"
                : simulacao.ProdutoNome;
            simulacao.MetodoNome = metodo?.Nome ?? simulacao.MetodoNome;
            simulacao.Cor = produto?.Cor;
            simulacao.Tamanho = produto?.Tamanho;

            // ── Preço original (sem desconto) ─────────────────────────────────
            if (simulacao.PercentualDesconto > 0)
                simulacao.PrecoVendaOriginal = simulacao.PrecoVenda.HasValue && simulacao.ValorDesconto.HasValue
                    ? simulacao.PrecoVenda + simulacao.ValorDesconto
                    : simulacao.PrecoVenda;

            _context.SaveChanges();

            return PartialView("_SimulacaoRow", simulacao);
        }

        // ✅ DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var simulacao = _context.Simulacao.FirstOrDefault(x => x.Id == id);
            if (simulacao == null) return NotFound();
            _context.Simulacao.Remove(simulacao);
            _context.SaveChanges();
            return Ok();
        }

        // ✅ TOGGLE STATUS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatusAjax(int id)
        {
            var simulacao = _context.Simulacao.FirstOrDefault(x => x.Id == id);
            if (simulacao == null) return NotFound();
            simulacao.Status = !simulacao.Status;
            _context.SaveChanges();
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ImprimirPublico(int id, string token)
        {
            var tokenEsperado = "TATIANETOGNASCAMEGAHAIR2026";

            if (token != tokenEsperado)
                return Unauthorized();

            var sim = _context.Simulacao.FirstOrDefault(x => x.Id == id);

            if (sim == null)
                return NotFound();

            return View("Imprimir", sim);
        }

        // ✅ CONFIRMAR VENDA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarVenda(int id)
        {
            var sim = _context.Simulacao.FirstOrDefault(x => x.Id == id);
            if (sim == null) return NotFound();

            var venda = new Venda
            {
                ClienteId = sim.ClienteId,
                ClienteNome = sim.ClienteNome,
                ProdutoId = sim.ProdutoId,
                ProdutoNome = sim.ProdutoNome,
                QuantidadeGramas = sim.QuantidadeGramas,
                PrecoVenda = sim.PrecoVenda,
                CustoTotal = sim.CustoTotal,
                LucroTotal = sim.LucroTotal,
                ValorComissaoVendedora = sim.ValorComissaoVendedora ?? 0,
                ValorComissaoCabeleireira = sim.ValorComissaoCabeleireira ?? 0,
                DataVenda = DateTime.Now
            };

            _context.Vendas.Add(venda);

            _context.ContasFinanceiras.Add(new ContaFinanceira
            {
                Tipo = TipoContaFinanceira.Receber,
                Origem = OrigemContaFinanceira.Venda,
                Descricao = $"Venda - {venda.ProdutoNome} ({venda.QuantidadeGramas}g)",
                Valor = venda.PrecoVenda ?? 0,
                DataCompetencia = venda.DataVenda,
                DataVencimento = venda.DataVenda,
                Status = StatusContaFinanceira.Aberta,
                Venda = venda
            });

            // Baixa estoque FIFO
            decimal restante = sim.QuantidadeGramas;
            var compras = _context.Compras
                .Where(c => c.ProdutoId == sim.ProdutoId && c.QuantidadeGramas > 0)
                .OrderBy(c => c.DataCompra)
                .ToList();

            foreach (var c in compras)
            {
                if (restante <= 0) break;
                if (c.QuantidadeGramas >= restante)
                {
                    c.QuantidadeGramas -= restante;
                    restante = 0;
                }
                else
                {
                    restante -= c.QuantidadeGramas.Value;
                    c.QuantidadeGramas = 0;
                }
            }

            if (restante > 0)
                return BadRequest("Estoque insuficiente para concluir a venda.");

            var produto = _context.Produtos.FirstOrDefault(p => p.Id == sim.ProdutoId);
            if (produto != null)
            {
                produto.EstoqueGramas -= sim.QuantidadeGramas;
                if (produto.EstoqueGramas < 0) produto.EstoqueGramas = 0;
            }

            if (venda.ValorComissaoVendedora > 0)
                _context.Comissoes.Add(new Comissao
                {
                    VendaId = venda.Id,
                    Profissional = "Vendedora",
                    Valor = venda.ValorComissaoVendedora,
                    Pago = false
                });

            if (venda.ValorComissaoCabeleireira > 0)
                _context.Comissoes.Add(new Comissao
                {
                    VendaId = venda.Id,
                    Profissional = "Cabeleireira",
                    Valor = venda.ValorComissaoCabeleireira,
                    Pago = false
                });

            sim.Status = false;
            _context.SaveChanges();

            return Json(new
            {
                redirectUrl = Url.Action("Index", "Venda", new
                {
                    mes = DateTime.Now.Month,
                    ano = DateTime.Now.Year
                })
            });
        }

        // ✅ AJAX — Estoque
        [HttpGet]
        public IActionResult ObterEstoqueProduto(int produtoId)
        {
            var estoque = _context.Produtos
                .Where(p => p.Id == produtoId)
                .Select(p => p.EstoqueGramas)
                .FirstOrDefault();
            return Json(new { estoque });
        }

        // ✅ AJAX — Custo médio
        [HttpGet]
        public IActionResult ObterCustoCabelo(int produtoId, decimal quantidadeGramas)
        {
            if (quantidadeGramas <= 0) return Json(new { custo = 0 });

            var compras = _context.Compras
                .Where(c => c.ProdutoId == produtoId && c.QuantidadeGramas > 0)
                .ToList();

            if (!compras.Any()) return Json(new { custo = 0 });

            var custo = (compras.Sum(c => c.ValorTotal) / compras.Sum(c => c.QuantidadeGramas!.Value)) * quantidadeGramas;
            return Json(new { custo });
        }

        // ✅ AJAX — Custo + Lucro
        [HttpGet]
        public IActionResult ObterLucroCabelo(int produtoId, decimal quantidadeGramas, decimal percentualLucro)
        {
            if (produtoId <= 0 || quantidadeGramas <= 0)
                return Json(new { custo = 0m, lucro = 0m });

            var compras = _context.Compras
                .Where(c => c.ProdutoId == produtoId && c.QuantidadeGramas > 0)
                .ToList();

            if (!compras.Any()) return Json(new { custo = 0m, lucro = 0m });

            var custoMedio = compras.Sum(c => c.ValorTotal) / compras.Sum(c => c.QuantidadeGramas!.Value);
            var custo = custoMedio * quantidadeGramas;
            var lucro = custo * (percentualLucro / 100m);

            return Json(new { custo, lucro });
        }

        // ✅ GERAR PDF
        public async Task<IActionResult> GerarPdf(int id)
        {
            var sim = _context.Simulacao.FirstOrDefault(x => x.Id == id);
            if (sim == null) return NotFound();

            var html = await _viewRender.RenderToStringAsync("~/Views/Simulacao/Imprimir.cshtml", sim);
            var pdf = _pdfService.GerarPdf(html);
            return File(pdf, "application/pdf");
        }

        [HttpGet]
        public IActionResult Imprimir(int id)
        {
            var sim = _context.Simulacao.FirstOrDefault(x => x.Id == id);
            if (sim == null) return NotFound();

            var logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/Imagem/LogoMegaHairPro.png");

            if (System.IO.File.Exists(logoPath))
            {
                var bytes = System.IO.File.ReadAllBytes(logoPath);
                sim.LogoCaminho = "data:image/png;base64," + Convert.ToBase64String(bytes);
            }

            var produto = _context.Produtos.FirstOrDefault(p => p.Id == sim.ProdutoId);

            sim.ProdutoNome = produto != null
                ? $"{produto.TipoProduto} {produto.Origem}"
                : string.Empty;

            sim.Cor = produto?.Cor;
            sim.Tamanho = produto?.Tamanho;

            return View(sim);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void CarregarViewBags()
        {
            ViewBag.Clientes = _context.Clientes
                .Select(p => new { p.Id, p.CPF, p.Nome }).ToList();

            ViewBag.Produtos = _context.Produtos
                .Where(x => x.Status == true && x.EstoqueGramas > 0)
                .Select(p => new
                {
                    p.Id,
                    p.Codigo,
                    Nome = p.TipoProduto == TipoProduto.Cabelo
                        ? p.Codigo + " - " + p.Origem + " (" + p.Tamanho + ")"
                        : p.Codigo + " - " + p.Descricao,
                    p.EstoqueGramas,
                    p.UnidadeMedida,
                    p.Status,
                    p.Origem,
                    p.Tamanho
                })
                .AsEnumerable()
                .OrderBy(x => x.Origem)
                .ThenBy(x =>
                {
                    var num = new string(x.Tamanho?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());
                    return int.TryParse(num, out var n) ? n : 0;
                })
                .ToList();

            ViewBag.Metodos = _context.MetodosAplicacao
                .Where(x => x.Status).ToList();

            ViewBag.Despesas = _context.Despesas
                .Where(x => x.Status == true).ToList();
        }

        private static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            var normalized = value.Replace(".", "").Replace(",", ".");
            return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result : 0;
        }

        [HttpGet]
        public IActionResult MensagemWhatsApp(int id)
        {
            var sim = _context.Simulacao.FirstOrDefault(x => x.Id == id);
            if (sim == null) return NotFound();

            var produto = _context.Produtos.FirstOrDefault(p => p.Id == sim.ProdutoId);
            sim.ProdutoNome = produto != null
                ? $"{produto.TipoProduto} {produto.Origem}"
                : string.Empty;
            sim.Cor = produto?.Cor;
            sim.Tamanho = produto?.Tamanho;

            var cliente = _context.Clientes.FirstOrDefault(c => c.Id == sim.ClienteId);
            var whatsapp = cliente?.WhatsApp ?? cliente?.Telefone ?? "";
            var numero = new string(whatsapp.Where(char.IsDigit).ToArray());
            if (numero.Length <= 11) numero = "55" + numero;

            var linkOrcamento = $"{Request.Scheme}://{Request.Host}/Simulacao/ImprimirPublico/{id}?token=TATIANETOGNASCAMEGAHAIR2026";

            decimal precoFinal = sim.PrecoVenda ?? 0;
            decimal desconto = sim.ValorDesconto ?? 0;
            decimal confeccao = sim.ValorTotalConfeccao ?? 0;
            bool temDesconto = desconto > 0;
            bool temConfeccao = confeccao > 0;
            var validade = sim.DataValidadePix != default
                ? sim.DataValidadePix
                : DateTime.Now.AddDays(7);

            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"Olá, *{sim.ClienteNome}*! 👋");
            sb.AppendLine();
            sb.AppendLine("✨ *Seu orçamento — Mega Hair Pro*");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine($"💇 *Produto:* {sim.ProdutoNome}");

            if (!string.IsNullOrEmpty(sim.Cor))
                sb.AppendLine($"🎨 *Cor:* {sim.Cor}");
            if (!string.IsNullOrEmpty(sim.Tamanho))
                sb.AppendLine($"📐 *Tamanho:* {sim.Tamanho}");

            sb.AppendLine($"⚖️ *Quantidade:* {sim.QuantidadeGramas:N0}g");

            if (!string.IsNullOrEmpty(sim.MetodoNome))
                sb.AppendLine($"🔧 *Método:* {sim.MetodoNome}");

            if (temConfeccao)
                sb.AppendLine($"✂️ *Confecção:* R$ {confeccao:N2}");

            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━");

            if (temDesconto)
            {
                sb.AppendLine($"💰 *Valor:* ~R$ {(sim.PrecoVendaOriginal ?? 0):N2}~");
                sb.AppendLine($"🎁 *Desconto ({sim.PercentualDesconto:N0}%):* − R$ {desconto:N2}");
                sb.AppendLine($"✅ *Total:* *R$ {precoFinal:N2}*");
            }
            else
            {
                sb.AppendLine($"✅ *Total:* *R$ {precoFinal:N2}*");
            }

            sb.AppendLine();
            sb.AppendLine("💳 *Formas de pagamento:*");
            sb.AppendLine("  • Dinheiro, PIX ou Débito (à vista)");
            sb.AppendLine("  • Cartão de crédito parcelado _(sujeito a juros)_");
            sb.AppendLine();
            sb.AppendLine($"⏳ *Válido até:* {validade:dd/MM/yyyy}");
            sb.AppendLine();
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine();
            sb.AppendLine("📄 *Visualize seu orçamento completo:*");
            sb.AppendLine(linkOrcamento);
            sb.AppendLine();
            sb.AppendLine("_Mega Hair Pro — Tatiane Tognasca_");
            sb.AppendLine("_Rua das Manacás, 340, Sala 1_");
            sb.AppendLine("_📞 (11) 93052-8032_");

            var mensagem = Uri.EscapeDataString(sb.ToString());
            var urlWhatsAppWeb = string.IsNullOrEmpty(numero)
                ? $"https://wa.me/?text={mensagem}"
                : $"https://wa.me/{numero}?text={mensagem}";
            var urlWhatsAppNativo = string.IsNullOrEmpty(numero)
                ? $"whatsapp://send?text={mensagem}"
                : $"whatsapp://send?phone={numero}&text={mensagem}";

            return Json(new
            {
                url = urlWhatsAppWeb,
                urlNativo = urlWhatsAppNativo,
                temNumero = !string.IsNullOrEmpty(whatsapp)
            });
        }

        public IActionResult Compartilhar(int id)
        {
            var arquivo = $"/imagens/simulacoes/simulacao_{id}.png";
            return View(model: arquivo);
        }
    }
}