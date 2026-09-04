# RouteFlow

> **Work in progress (WIP):** este repositório está em desenvolvimento. A documentação define o produto e a arquitetura desejados; nem todos os módulos, fluxos e garantias descritos já estão implementados.

RouteFlow é uma plataforma de logística de última milha para acompanhar uma entrega desde a solicitação e coleta até a conclusão, devolução ou tratamento de ocorrências operacionais.

## Visão geral

O produto é concebido como um monólito modular em .NET 10, com fronteiras que permitem futura extração de módulos. O domínio central de **Entregas** governa o ciclo de vida e a custódia do pacote; **Frotas**, **Precificação** e **Rastreamento & Notificações** o complementam por contratos públicos e eventos de integração.

## Escopo planejado

- **Entregas:** máquina de estados rigorosa, custódia, tentativas, ocorrências, cancelamentos e devoluções.
- **Frotas:** disponibilidade de motoristas e atribuição concorrente de entregas.
- **Precificação:** cálculo e recálculo de frete conforme pacote, rota e regras contratuais.
- **Rastreamento & Notificações:** linha do tempo pública, previsão de entrega e alertas.

As regras operacionais, transições válidas e decisões pendentes de implementação estão documentadas em [`docs/`](docs/).

## Arquitetura

- `src/Bootstrapper/RouteFlow.Api`: API HTTP e composição dos módulos.
- `src/Modules`: módulos organizados em `Domain`, `Application`, `Contracts` e `Infrastructure`.
- `src/Orchestration/RouteFlow.AppHost`: orquestra a API e os bancos PostgreSQL com .NET Aspire.
- `src/Shared` e `src/SharedKernel`: recursos transversais e primitivas de domínio.
- `tests`: testes unitários, de integração e de arquitetura.

Cada módulo terá dados isolados e se comunicará apenas por contratos públicos ou eventos de integração — sem compartilhamento de tabelas entre contextos.

## Executar localmente

Pré-requisitos: .NET 10 SDK e Docker em execução.

```powershell
dotnet restore RouteFlow.slnx
dotnet run --project src/Orchestration/RouteFlow.AppHost
```

O AppHost inicia os recursos atualmente configurados e disponibiliza o dashboard do Aspire. Em ambiente de desenvolvimento, a documentação interativa da API é exposta pelo Scalar.

Para executar todos os testes:

```powershell
dotnet test RouteFlow.slnx --no-restore --disable-build-servers -m:1 --verbosity minimal
```

## Documentação

- [Contexto de negócio](docs/business-context.md)
- [Decisões de negócio](docs/business-decisions.md)
- [Modelo de domínio](docs/domain-model-design.md)
- [Blueprint de arquitetura](docs/modular-monolith-blueprint.md)
