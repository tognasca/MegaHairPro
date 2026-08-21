using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaHairPro.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateAdemarT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhatsApp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CPF = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendaId = table.Column<int>(type: "int", nullable: false),
                    Profissional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Pago = table.Column<bool>(type: "bit", nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comissoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracoesBooking",
                columns: table => new
                {
                    Chave = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesBooking", x => x.Chave);
                });

            migrationBuilder.CreateTable(
                name: "Despesas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ratear = table.Column<bool>(type: "bit", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorCategoria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Despesas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Contato = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CNPJ = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InscricaoEstadual = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RazaoSocial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NomeFantasia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logradouro = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Complemento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Bairro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cidade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UF = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    CEP = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Banco = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Agencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Conta = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoConta = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetodosAplicacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValorPorGrama = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorTotalConfeccao = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodosAplicacao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profissionais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Especialidade = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profissionais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorPromocional = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DuracaoMinutos = table.Column<int>(type: "int", nullable: false),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    ImagemUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Simulacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ClienteNome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProdutoNome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    QuantidadeGramas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstoqueAtualGramas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstoquePosVendaGramas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustoCabelo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CustoServico = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PercentualLucroCabelo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorLucroCabelo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaoDeObra = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tinta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CorteEscova = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DespesaSalaoRateada = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PercentualComissaoVendedora = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValorComissaoVendedora = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PercentualComissaoCabeleireira = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValorComissaoCabeleireira = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PrecoVenda = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CustoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LucroCabelo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LucroVenda = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LucroLiquido = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LucroTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValorDesconto = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PrecoVendaOriginal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MargemPercentual = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    QrCodePixBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoCaminho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValorTotalConfeccao = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MetodoId = table.Column<int>(type: "int", nullable: false),
                    MetodoNome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataValidadePix = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simulacao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Login = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Senha = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenhaHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Perfil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ClienteNome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    ProdutoNome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuantidadeGramas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecoVenda = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CustoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LucroTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValorComissaoVendedora = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorComissaoCabeleireira = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataVenda = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Anamneses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Alergias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HistoricoClinico = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcedimentosAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Restricoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssinaturaDigital = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ativa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anamneses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anamneses_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosClientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosClientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosClientes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoProduto = table.Column<int>(type: "int", nullable: false),
                    UnidadeMedida = table.Column<int>(type: "int", nullable: false),
                    Origem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tamanho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FornecedorId = table.Column<int>(type: "int", nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    EstoqueGramas = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produtos_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Agendamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ProfissionalId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Cancelado = table.Column<bool>(type: "bit", nullable: false),
                    PercentualSinal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorSinal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SinalPago = table.Column<bool>(type: "bit", nullable: false),
                    DataPagamentoSinal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValorRestante = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RestantePago = table.Column<bool>(type: "bit", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agendamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "Profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Agendamentos_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    FornecedorId = table.Column<int>(type: "int", nullable: false),
                    QuantidadeGramas = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataCompra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compras_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Compras_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Estoques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    QuantidadeGramas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantidadeUnidade = table.Column<int>(type: "int", nullable: false),
                    CustoMedio = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estoques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Estoques_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgendamentoServicos",
                columns: table => new
                {
                    AgendamentoId = table.Column<int>(type: "int", nullable: false),
                    ServicoId = table.Column<int>(type: "int", nullable: false),
                    ProfissionalId = table.Column<int>(type: "int", nullable: true),
                    Realizado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendamentoServicos", x => new { x.AgendamentoId, x.ServicoId });
                    table.ForeignKey(
                        name: "FK_AgendamentoServicos_Agendamentos_AgendamentoId",
                        column: x => x.AgendamentoId,
                        principalTable: "Agendamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendamentoServicos_Profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "Profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendamentoServicos_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContasFinanceiras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Origem = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DataCompetencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FormaPagamento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    VendaId = table.Column<int>(type: "int", nullable: true),
                    DespesaId = table.Column<int>(type: "int", nullable: true),
                    AgendamentoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasFinanceiras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContasFinanceiras_Agendamentos_AgendamentoId",
                        column: x => x.AgendamentoId,
                        principalTable: "Agendamentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContasFinanceiras_Despesas_DespesaId",
                        column: x => x.DespesaId,
                        principalTable: "Despesas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContasFinanceiras_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_ClienteId",
                table: "Agendamentos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_ProfissionalId",
                table: "Agendamentos",
                column: "ProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_ServicoId",
                table: "Agendamentos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendamentoServicos_ProfissionalId",
                table: "AgendamentoServicos",
                column: "ProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendamentoServicos_ServicoId",
                table: "AgendamentoServicos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anamneses_ClienteId",
                table: "Anamneses",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_FornecedorId",
                table: "Compras",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_ProdutoId",
                table: "Compras",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasFinanceiras_AgendamentoId",
                table: "ContasFinanceiras",
                column: "AgendamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasFinanceiras_DespesaId",
                table: "ContasFinanceiras",
                column: "DespesaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasFinanceiras_VendaId",
                table: "ContasFinanceiras",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoques_ProdutoId",
                table: "Estoques",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_FornecedorId",
                table: "Produtos",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosClientes_ClienteId",
                table: "UsuariosClientes",
                column: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgendamentoServicos");

            migrationBuilder.DropTable(
                name: "Anamneses");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Comissoes");

            migrationBuilder.DropTable(
                name: "Compras");

            migrationBuilder.DropTable(
                name: "ConfiguracoesBooking");

            migrationBuilder.DropTable(
                name: "ContasFinanceiras");

            migrationBuilder.DropTable(
                name: "Estoques");

            migrationBuilder.DropTable(
                name: "MetodosAplicacao");

            migrationBuilder.DropTable(
                name: "Simulacao");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "UsuariosClientes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Agendamentos");

            migrationBuilder.DropTable(
                name: "Despesas");

            migrationBuilder.DropTable(
                name: "Vendas");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Profissionais");

            migrationBuilder.DropTable(
                name: "Servicos");

            migrationBuilder.DropTable(
                name: "Fornecedores");
        }
    }
}
