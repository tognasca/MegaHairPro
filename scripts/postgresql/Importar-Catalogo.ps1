[CmdletBinding()]
param(
    [string]$SqlServerConnectionString = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MegaHairPro;Integrated Security=True;TrustServerCertificate=True;",
    [string]$ContainerName = "megahair-postgres",
    [string]$DatabaseName = "MegaHairPro",
    [string]$PostgresUser = "postgres",
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "export")
)

$ErrorActionPreference = "Stop"

function Export-SqlTableCsv {
    param([System.Data.SqlClient.SqlConnection]$Connection, [string]$Query, [string]$Path)

    $command = $Connection.CreateCommand()
    $command.CommandText = $Query
    $adapter = [System.Data.SqlClient.SqlDataAdapter]::new($command)
    $table = [System.Data.DataTable]::new()
    [void]$adapter.Fill($table)

    $objects = foreach ($row in $table.Rows) {
        $record = [ordered]@{}
        foreach ($column in $table.Columns) {
            $value = $row[$column.ColumnName]
            if ($value -is [DBNull]) { $record[$column.ColumnName] = "__MHP_NULL_8F9C2A__" }
            elseif ($value -is [datetime]) { $record[$column.ColumnName] = $value.ToString("yyyy-MM-dd HH:mm:ss.fffffff", [Globalization.CultureInfo]::InvariantCulture) }
            elseif ($value -is [IFormattable]) { $record[$column.ColumnName] = $value.ToString($null, [Globalization.CultureInfo]::InvariantCulture) }
            else { $record[$column.ColumnName] = [string]$value }
        }
        [pscustomobject]$record
    }

    $csv = @($objects | ConvertTo-Csv -NoTypeInformation)
    [System.IO.File]::WriteAllText($Path, ($csv -join [Environment]::NewLine), [System.Text.UTF8Encoding]::new($false))
    return $table.Rows.Count
}

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "Docker CLI não encontrado."
}
if (-not (Test-Path $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
}

$connection = [System.Data.SqlClient.SqlConnection]::new($SqlServerConnectionString)
try {
    $connection.Open()

    $supplierCount = Export-SqlTableCsv -Connection $connection -Query @"
SELECT Id, Nome, Contato, CNPJ, InscricaoEstadual, RazaoSocial, NomeFantasia,
       Telefone, Email, Logradouro, Numero, Complemento, Bairro, Cidade, UF,
       CEP, Banco, Agencia, Conta, TipoConta, Observacoes, Status
FROM dbo.Fornecedores
ORDER BY Id;
"@ -Path (Join-Path $OutputDirectory "fornecedores.csv")

    $productCount = Export-SqlTableCsv -Connection $connection -Query @"
SELECT Id, Codigo, Descricao, TipoProduto, UnidadeMedida, Origem, Tamanho,
       Cor, FornecedorId, DataCadastro, Status, EstoqueGramas
FROM dbo.Produtos
ORDER BY Id;
"@ -Path (Join-Path $OutputDirectory "produtos.csv")
}
finally {
    $connection.Dispose()
}

if ($supplierCount -ne 1 -or $productCount -ne 140) {
    throw "Origem divergente. Esperado: 1 fornecedor e 140 produtos. Encontrado: $supplierCount fornecedor(es), $productCount produto(s). Nenhuma importação foi executada."
}

$sql = @'
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM public."Fornecedores")
       OR EXISTS (SELECT 1 FROM public."Produtos") THEN
        RAISE EXCEPTION 'Importação cancelada: Fornecedores ou Produtos já possuem registros no destino.';
    END IF;
END $$;

\copy public."Fornecedores" ("Id","Nome","Contato","CNPJ","InscricaoEstadual","RazaoSocial","NomeFantasia","Telefone","Email","Logradouro","Numero","Complemento","Bairro","Cidade","UF","CEP","Banco","Agencia","Conta","TipoConta","Observacoes","Status") FROM '/tmp/fornecedores.csv' WITH (FORMAT csv, HEADER true, NULL '__MHP_NULL_8F9C2A__', FORCE_NULL ("InscricaoEstadual","RazaoSocial","NomeFantasia","Telefone","Email","Logradouro","Numero","Complemento","Bairro","Cidade","UF","CEP","Banco","Agencia","Conta","TipoConta","Observacoes"));
\copy public."Produtos" ("Id","Codigo","Descricao","TipoProduto","UnidadeMedida","Origem","Tamanho","Cor","FornecedorId","DataCadastro","Status","EstoqueGramas") FROM '/tmp/produtos.csv' WITH (FORMAT csv, HEADER true, NULL '__MHP_NULL_8F9C2A__', FORCE_NULL ("Codigo","Descricao","Origem","Tamanho","Cor","FornecedorId"));

SELECT setval(pg_get_serial_sequence('public."Fornecedores"', 'Id'), GREATEST(COALESCE((SELECT MAX("Id") FROM public."Fornecedores"), 1), 1), true);
SELECT setval(pg_get_serial_sequence('public."Produtos"', 'Id'), GREATEST(COALESCE((SELECT MAX("Id") FROM public."Produtos"), 1), 1), true);

SELECT 'Fornecedores' AS tabela, COUNT(*) AS registros FROM public."Fornecedores"
UNION ALL
SELECT 'Produtos', COUNT(*) FROM public."Produtos";

SELECT COUNT(*) AS produtos_ativos,
       COALESCE(SUM("EstoqueGramas"), 0) AS estoque_total_gramas,
       COUNT(DISTINCT "Codigo") AS codigos_distintos
FROM public."Produtos";
'@

$sqlPath = Join-Path $OutputDirectory "importar-catalogo.sql"
[System.IO.File]::WriteAllText($sqlPath, $sql, [System.Text.UTF8Encoding]::new($false))

$containerCheck = & docker inspect -f '{{.State.Running}}' $ContainerName
if ($LASTEXITCODE -ne 0 -or $containerCheck -ne "true") {
    throw "Container PostgreSQL '$ContainerName' não está em execução."
}

& docker cp (Join-Path $OutputDirectory "fornecedores.csv") ($ContainerName + ":/tmp/fornecedores.csv")
if ($LASTEXITCODE -ne 0) { throw "Falha ao copiar fornecedores.csv para o container." }
& docker cp (Join-Path $OutputDirectory "produtos.csv") ($ContainerName + ":/tmp/produtos.csv")
if ($LASTEXITCODE -ne 0) { throw "Falha ao copiar produtos.csv para o container." }
& docker cp $sqlPath ($ContainerName + ":/tmp/importar-catalogo.sql")
if ($LASTEXITCODE -ne 0) { throw "Falha ao copiar o SQL de importação para o container." }

& docker exec $ContainerName psql -U $PostgresUser -d $DatabaseName -v ON_ERROR_STOP=1 -f /tmp/importar-catalogo.sql
if ($LASTEXITCODE -ne 0) {
    throw "Importação falhou. Os dados de origem e backups SQL Server foram preservados. Verifique o erro do PostgreSQL antes de tentar novamente."
}

Write-Host "Exportação/importação concluída. Esperado: 1 fornecedor e 140 produtos." -ForegroundColor Green
Write-Host "Confirme no resultado do PostgreSQL: 833.00g de estoque e 140 códigos distintos."
