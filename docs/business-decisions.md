# RouteFlow — Registro de Decisões de Negócio e Regras de Domínio

Este documento formaliza todas as decisões de negócio e regras operacionais acordadas durante a fase de descoberta do domínio. Ele serve como fonte de verdade para a futura implementação em código.

---

## 1. Ciclo de Vida e Custódia Física da Entrega

A entrega é orientada pela **transferência de custódia física** do pacote. O conceito vago de "Em Rota" foi decomposto em etapas precisas:

```text
[Requested] 
     ↓
[DriverAssigned]
     ↓
[DispatchedToPickup]  --> (Custódia: Merchant)
     ↓
[ArrivedAtPickup]     --> (Custódia: Merchant / Conferência)
     ↓ (Pickup confirmada / Bipagem de código de barras)
[InTransit]           --> (Custódia: RouteFlow / Driver)
     ↓
[Desfecho: Completed, PendingReschedule ou InReturn]
```

### Regras de Custódia:
1. **Antes da Coleta:** O pacote está fisicamente na loja do remetente (`Merchant`). Se o motorista desistir, tiver pane mecânica ou a corrida for cancelada, o pacote permanece seguro e a entrega pode ser reatribuída sem resgate físico.
2. **Depois da Coleta:** A custódia transfere-se formalmente para a RouteFlow (`Driver`). Qualquer interrupção exige fluxo obrigatório de devolução ou transbordo físico monitorado.

---

## 2. Tentativas de Entrega, Falhas e Desfechos

Nem toda falha tem o mesmo tratamento. O sistema diferencia falhas transitórias de encerramentos definitivos:

### 2.1. Destinatário Ausente (Falha Transitória)
- Toda tentativa frustrada deve registrar: **data/hora exata**, **motivo (`FailureReason`)** e **número sequencial da tentativa (`AttemptNumber`)**.
- **Tentativas 1 e 2:** A entrega gera o evento `DeliveryAttemptFailed` e entra no estado `PendingReschedule` para o próximo dia útil.
- **Tentativa 3 (Limite Máximo):** Atingido o limite de 3 tentativas, o sistema encerra tentativas de entrega e transiciona para `InReturn`, disparando o evento `DeliveryReturnInitiated` para retornar o pacote ao remetente.

### 2.2. Recusa Expressa pelo Destinatário
- Se o destinatário atender e recusar expressamente receber o pacote (ex: avarias, alegação de compra cancelada na loja):
  - **Zero novas tentativas.** Não faz sentido reincidir em visitas se houve recusa formal.
  - A entrega transiciona imediatamente para `InReturn` (`DeliveryReturnInitiated`).

### 2.3. Endereço Inexistente ou Incompleto (Fila de Tratativa Operacional)
- Diferente de "destinatário ausente", falhas cadastrais de endereço **não sofrem nova tentativa automática** (evitando desperdício de combustível e tempo com novo insucesso previsível).
- O pacote entra no estado `InOperationalIssue`.
- O Operador Logístico é acionado para obter a correção com o lojista.
- **Resoluções possíveis da Tratativa:**
  1. **Endereço Corrigido:** O lojista fornece os dados válidos dentro do prazo; a entrega é atualizada e reenfileirada para a próxima rota (`InTransit`).
  2. **Devolução Autorizada pelo Lojista:** O lojista opta por cancelar o envio e receber o pacote de volta (`InReturn`).
  3. **Timeout de Proteção (48 horas):** Se após 48 horas o lojista não fornecer resolução, o sistema dispara `OperationalIssueExpired` e transiciona compulsoriamente para `InReturn`, impedindo que a base vire depósito de cargas paradas.

### 2.4. Encerramento da Operação
- Uma entrega só é considerada formalmente **encerrada** no sistema quando atinge um dos estados terminais:
  1. For entregue com sucesso ao destinatário (`Completed`), OU
  2. For devolvida e confirmada pelo remetente de origem (`ReturnedToSender`).

---

## 3. Política de Cancelamento

O cancelamento solicitado pelo cliente (lojista) depende estritamente do momento operacional e da custódia:

| Momento do Cancelamento | Ação do Sistema | Efeito Físico |
| :--- | :--- | :--- |
| **Antes da Coleta** (enquanto `Requested` ou `DispatchedToPickup`) | Cancela imediatamente (`DeliveryCanceled`). | Motorista é notificado e liberado para outras corridas. Pacote continua na loja. |
| **No Balcão da Loja** (`ArrivedAtPickup`) | Cancela a coleta (`PickupCanceledByMerchant`). | Motorista é liberado e recebe taxa de deslocamento paga pelo lojista. |
| **Depois da Coleta** (`InTransit`) | Não encerra direto! Transiciona para `InReturn` (`DeliveryReturnInitiated`). | Motorista é instruído a não entregar ao destinatário e retornar com o pacote à base/loja. |
| **Depois de Concluída** (`Completed`) | **Operação Proibida.** | O sistema lança exceção de domínio (não é possível cancelar entrega já concluída). |

---

## 4. Incompatibilidade de Pacote e Veículo na Coleta

Quando o motorista chega à loja e constata que o pacote real não condiz com o cadastrado (ex: motorista de moto e caixa de 40 kg):
1. A coleta por aquele motorista é cancelada com motivo `IncompatibleVehicle`.
2. O motorista é liberado e recebe taxa de deslocamento.
3. A entrega não morre: volta para o estado `Requested` exigindo o tipo de veículo correto (ex: utilitário/furgão).
4. O valor do frete é recalculado pelo módulo de **Pricing** e cobrado do lojista.

O requisito é registrado de forma estruturada como `VehicleType` (`Motorcycle`, `Car`, `Van` ou `LightTruck`). Enquanto houver um requisito registrado, uma nova atribuição sem tipo de veículo ou com tipo diferente é rejeitada pelo aggregate.

---

## 5. Política de Alteração de Endereço

O impacto da alteração de endereço depende do momento da operação:

### 5.1. Antes da Coleta
- O endereço pode ser alterado livremente enquanto o pacote estiver na loja do remetente.
- Se a alteração alterar significativamente a distância, o módulo de **Pricing** recalcula o frete para cobrança do lojista.
- O motorista atribuído é notificado sobre o novo destino. Caso recuse a nova rota, a entrega é reenfileirada para outro motorista sem penalidade.

### 5.2. Com o Pacote Já em Trânsito (`InTransit`)
- Alterações em trânsito não são automáticas:
  1. O módulo de **Pricing** calcula o acréscimo de frete pela quilometragem adicional.
  2. O lojista deve aprovar a cobrança da diferença.
  3. O motorista em rota recebe a oferta da nova rota com valor reajustado:
     - **Se o motorista aceitar:** Segue viagem para o novo endereço com a remuneração atualizada.
     - **Se o motorista recusar:** Em conformidade com a regra de proibição de transbordo em campo, o motorista não repassa a carga na rua. Ele encerra a rota na **Base/Hub da RouteFlow**, onde o pacote é acolhido e despachado no dia seguinte com outro motorista.
     - **Se o lojista recusar a taxa adicional:** A entrega segue o fluxo de devolução (`InReturn`).

---

## 6. Incidentes em Trânsito e Transbordo de Cargas (Pane / Acidente)

- **Transbordo em Campo Proibido:** É expressamente proibida a transferência direta de pacotes entre motoristas na rua ("passagem de bastão"). O risco de fraude, acidentes, perda de rastreabilidade e recusa da cobertura de seguro inviabilizam essa prática.
- **Protocolo de Incidente:**
  1. O motorista reporta o ocorrido no aplicativo (`TransitIncidentReported`).
  2. O estado da entrega transiciona para `HeldDueToIncident`.
  3. Lojista e destinatário são notificados sobre o imprevisto e nova previsão.
  4. O motorista permanece responsável pela custódia do pacote até que este seja fisicamente entregue e bipado em um ponto seguro (**Hub da RouteFlow** ou **Loja de Origem**).
  5. Somente após a bipagem na base (`PackageReceivedAtHub`) o motorista é desonerado da custódia, e a entrega é liberada para reagendamento em uma nova rota.

---

## 7. Concorrência e Atribuição de Motoristas

- **Problema a resolver:** Dois motoristas podem tentar aceitar a mesma entrega simultaneamente (*race condition*).
- **Decisão de implementação:** Quando a persistência PostgreSQL for introduzida, o aceite será protegido por controle de versão / concorrência otimista (*optimistic concurrency*). A coluna de versão será configurada como token de concorrência e o fluxo de aplicação tratará o conflito de atualização, informando amigavelmente à segunda tentativa que a corrida já foi assumida.
- **Status:** Decisão formalizada, ainda não implementada. O `Version` do aggregate, isoladamente, não oferece essa garantia sem configuração e tratamento na camada de persistência.

---

## 8. Stack e Decisões Técnicas Alinhadas

- **Linguagem / Plataforma:** C# / .NET 10 com **.NET Aspire** (orquestração local e telemetria OpenTelemetry).
- **Banco de Dados:** PostgreSQL (com schemas independentes por módulo: `deliveries`, `fleet`, `pricing`, `tracking`).
- **Identificadores Únicos (IDs):** Todos os identificadores do sistema utilizam **Guid v7 (UUIDv7)** nativo do .NET (`Guid.CreateVersion7()`), garantindo ordenação cronológica monótona e eliminando fragmentação de índices B-Tree no PostgreSQL.
- **Tipagem Temporal:** Todos os campos de data/hora utilizam `DateTimeOffset`, preservando fusos horários exatos e prevenindo ambiguidades em sistemas distribuídos.
- **Design de Tipos:** Classes não-herdadas e Value Objects são declarados como `sealed`, viabilizando desvirtualização pelo JIT e comunicando intenção de design fechado.
- **Práticas de Testes e Código:** Asserções nativas do xUnit (`Assert.*`, sem `FluentAssertions`), `NSubstitute` para mocks de dependências colaboradoras, fluxo de despacho direto e fortemente tipado (sem `MediatR`).
- **Arquitetura:** Monolito Modular (*Monolith First*), com comunicação entre módulos orientada a eventos e contratos públicos, preparando a futura extração para microsserviços.

---

## 9. Status da Descoberta do Domínio

Todos os dilemas operacionais levantados no [business-context.md](business-context.md) foram investigados e formalizados com sucesso:
- [x] **Dilema 1 (Transbordo / Pane):** Proibido em campo; pacote protegido e acolhido na Base.
- [x] **Dilema 2 (Conceito de 'Em Rota'):** Decomposto em 3 fases explícitas de custódia.
- [x] **Dilema 3 (Mudança de Endereço):** Livre antes da coleta; sob recálculo e consentimento do motorista em trânsito.
- [x] **Dilema 4 (Fila de Tratativa / Retorno):** Intervenção humana com timeout de proteção de 48 horas.
- [ ] **Concorrência e Race Conditions:** Decisão de concorrência otimista formalizada; implementação depende da persistência PostgreSQL.
- [x] **Incompatibilidade de Veículo:** Compensação de deslocamento e reenfileiramento com recálculo de frete.
