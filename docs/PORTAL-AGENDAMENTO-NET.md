# Portal de agendamento no MegaHairPro.Web (.NET)

**Projeto:** `C:\git\MegaHairPro\MegaHairPro.Web`

A lógica do `bookingController.js` (Node) foi migrada para:

| Componente | Arquivo |
|------------|---------|
| Controller (rotas + páginas) | `MegaHairPro.Web/Controllers/BookingPortalController.cs` |
| Regras de negócio | `MegaHairPro.Web/Services/BookingPortalService.cs` |
| Config (% sinal, lembrete, 24h) | `MegaHairPro.Web/Services/BookingConfigService.cs` |
| PIX EMV | `MegaHairPro.Web/Services/PixBookingService.cs` |
| Status JSON ↔ portal | `MegaHairPro.Web/Services/BookingStatusMapper.cs` |

O `AgendamentoController` existente continua para **uso interno** (calendário da equipe, registrar sinal, concluir).

## Status no banco (enum .NET)

| Portal (JSON) | Significado | `StatusAgendamento` |
|---------------|-------------|---------------------|
| 0 | Aguardando PIX | `Agendado` + `SinalPago = false` |
| 1 | Confirmado | `Confirmado` |
| 3 | Cancelado | `Cancelado` |
| 4 | Finalizado | `Concluido` |

## Configurar

1. Copie o front PWA para `wwwroot/booking`:

```powershell
xcopy /E /I C:\git\megahair-booking\public C:\git\MegaHairPro\MegaHairPro.Web\wwwroot\booking
```

2. Ajuste `appsettings.json` → seção `Booking` (token, PIX, e-mail do salão).

3. Execute `migration-config.sql` no SQL Server (tabela `ConfiguracoesBooking`).

4. Rode o projeto e acesse:
   - Cliente: `https://localhost:49377/`
   - Admin interno: `https://localhost:49377/Account/Login`
   - Agenda staff: `https://localhost:49377/Agendamento`

## Rotas principais (iguais ao Node)

- `GET /`, `/agendar`, `/pagamento/{id}`, `/meus-agendamentos`
- `POST /auth/cadastro`, `/auth/login`
- `GET /api/servicos`, `/api/horarios-disponiveis`, `POST /api/agendamento`
- `GET /admin/aprovar/{id}?token=...`

## Próximos passos opcionais

- `POST /api/admin/agendamento-manual` e importar contatos (ainda no Node ou replicar no service)
- `POST /internal/enviar-lembretes` (cron)
- Integração NuPay quando tiver `client_id` / `client_secret`
