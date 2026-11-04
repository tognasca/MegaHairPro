# Migração PostgreSQL — MegaHairPro

## Estado e limites de segurança

- Destino: banco lógico separado chamado MegaHairPro no container PostgreSQL existente megahair-postgres.
- O banco simulacaomegahairdb e o volume PostgreSQL existente não são alterados.
- SQL Server, LocalDB, backups e container megahairpro-db permanecem intactos como contingência.
- Migrações antigas em MegaHairPro.Web/Migrations são históricas do SQL Server. Não as execute no PostgreSQL.
- Migrações PostgreSQL ficam isoladas no projeto MegaHairPro.PostgreSqlMigrations.
- A aplicação não executa migrações automaticamente, a menos que Database__ApplyMigrations=true seja configurado explicitamente.

## Configuração

Defina a conexão por variável de ambiente, sem gravar credenciais no Git:

    $env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=55432;Database=MegaHairPro;Username=postgres;Password=SUA_SENHA"

No Desktop, use a mesma variável de ambiente no usuário que inicia o aplicativo.

## Criar o banco lógico (somente se ainda não existir)

Verifique:

    docker exec megahair-postgres psql -U postgres -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='MegaHairPro'"

Se não retornar 1, crie apenas o banco novo:

    docker exec megahair-postgres psql -U postgres -d postgres -c 'CREATE DATABASE "MegaHairPro";'

Não use DROP DATABASE, docker volume rm, docker compose down -v ou comandos de limpeza.

## Gerar a migração inicial PostgreSQL

A migração inicial precisa ser gerada pelo EF Core com o SDK .NET 10 e o modelo completo atual. O projeto dedicado evita misturar a migração SQL Server existente com PostgreSQL.

Na raiz do repositório:

    dotnet tool update --global dotnet-ef --version 10.0.5
    dotnet restore
    dotnet ef migrations add InitialPostgreSql --project MegaHairPro.PostgreSqlMigrations --startup-project MegaHairPro.Web --context AppDbContext --output-dir Migrations

Revise os arquivos gerados antes de aplicar. A migração deve criar o esquema completo do MegaHairPro, incluindo Identity, clientes, produtos, fornecedores, métodos, despesas, vendas, financeiro, salão e booking. Não deve criar tabelas de histórico técnico de geração de IA: isso continua no banco do Simulador.

Aplicar a migração somente depois da revisão:

    $env:Database__ApplyMigrations = "true"
    dotnet run --project MegaHairPro.Web

Depois da primeira aplicação, remova a variável:

    Remove-Item Env:Database__ApplyMigrations -ErrorAction SilentlyContinue

## Importar o catálogo real

Após a criação do esquema, execute scripts/postgresql/Importar-Catalogo.ps1 em PowerShell no Windows, na mesma máquina que possui o LocalDB e o Docker. O script exporta somente as linhas reais de Fornecedores e Produtos do LocalDB, importa no banco PostgreSQL MegaHairPro e confere os totais. Ele aborta se as tabelas de destino já tiverem registros; não apaga nem substitui dados.

Contagens esperadas da origem validada:
- Fornecedores: 1
- Produtos: 140
- Estoque total: 833,00 gramas
- Produtos ativos: 140
- Próximos IDs: Fornecedores 2 e Produtos 143

## Validação e corte

Antes de qualquer troca em produção:
1. Confirmar todas as tabelas e índices da migração PostgreSQL.
2. Confirmar os 1 fornecedor e 140 produtos, códigos sem duplicidade e estoque total de 833,00g.
3. Testar login/Identity, CRUD de clientes, produtos, compras, vendas, estoque, despesas, métodos, financeiro, agenda e booking.
4. Confirmar Desktop e Web usando o mesmo banco PostgreSQL.
5. Integrar o Simulador exclusivamente pela API autenticada do MegaHairPro; não compartilhar tabelas nem conectar o Simulador diretamente ao banco comercial.
6. Só depois de validação funcional e autorização explícita considerar desligar o SQL Server.

## Segurança

Os arquivos versionados não contêm mais a connection string antiga. Como uma credencial de banco já esteve no histórico Git, troque essa senha no SQL Server e em qualquer outro serviço onde tenha sido reutilizada. Configure também Booking/AdminToken, Booking/CronSecret e Email/Smtp por variáveis de ambiente ou secret manager.
