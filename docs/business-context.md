# RouteFlow — Contexto de Negócio

## Visão geral

A **RouteFlow** é uma empresa que presta serviços de logística de última milha para pequenos e médios lojistas.

Os lojistas já realizaram a venda dos produtos por seus próprios canais. A responsabilidade da RouteFlow começa depois da venda: ela recebe uma solicitação de entrega, coleta o pacote no remetente e tenta entregá-lo ao destinatário.

A empresa cresceu nos últimos anos e, com esse crescimento, a operação começou a enfrentar problemas relacionados ao acompanhamento e à execução das entregas.

---

## Funcionamento do negócio

Um cliente empresarial solicita uma nova entrega informando:

- origem;
- destino;
- características do pacote;
- janela esperada de entrega.

Depois que a solicitação é recebida, a operação precisa decidir como essa entrega será atendida, atribuir um motorista disponível e acompanhar sua execução até que o trabalho seja considerado encerrado.

O motorista pode aceitar ou recusar uma atribuição.

Depois de aceitar, ele precisa se deslocar até o ponto de coleta. A entrega não deve ser considerada em trânsito antes que a coleta realmente tenha acontecido.

Durante a execução podem ocorrer diferentes situações, como:

- destinatário ausente;
- endereço não localizado;
- pacote recusado;
- problema com o veículo;
- impossibilidade de acesso ao local.

Dependendo do motivo, a empresa pode realizar uma nova tentativa, reagendar a entrega ou determinar o retorno do pacote ao remetente.

Existe também uma equipe operacional responsável por acompanhar entregas problemáticas. Algumas situações podem ser resolvidas automaticamente pelo sistema, enquanto outras exigem intervenção humana.

---

## Ciclo de uma entrega

De maneira simplificada, uma entrega pode passar por algo conceitualmente parecido com:

```text
Solicitada
    ↓
Planejada
    ↓
Motorista atribuído
    ↓
Coletada
    ↓
Em rota
    ↓
?
```

Nem toda entrega termina em sucesso.

A partir de determinado ponto, diferentes caminhos podem surgir. Os estados necessários e os possíveis desfechos ainda precisam ser descobertos a partir das regras e processos do negócio.

---

## Algumas regras conhecidas

A empresa conhece algumas regras sobre sua operação:

- Uma entrega concluída não pode simplesmente voltar para um estado anterior.
- Uma entrega não pode começar sem que alguém tenha sido responsável pela coleta.
- Um motorista não pode assumir novas entregas quando estiver indisponível.
- Quando uma tentativa de entrega falha, é necessário registrar o motivo e o momento em que aconteceu.
- Alguns tipos de falha permitem uma nova tentativa; outros encerram imediatamente a operação daquela entrega.
- A empresa limita a quantidade de tentativas de entrega. Depois desse limite, o pacote precisa seguir outro fluxo.
- O cliente que contratou a entrega pode solicitar cancelamento, mas o momento da operação influencia se esse cancelamento ainda é possível.
- O endereço pode ser corrigido durante a operação, mas nem sempre.

Essas regras não estão completamente especificadas e existem ambiguidades que precisam ser investigadas.

Algumas perguntas que poderiam ser feitas às pessoas do negócio:

- O que exatamente significa uma entrega estar **em rota**?
- É possível trocar o motorista depois da coleta?
- Se o destinatário recusar o pacote, isso conta como uma tentativa?
- Quem decide que o pacote deve voltar ao remetente?
- Em quais situações uma entrega pode ser cancelada?
- Até qual momento o endereço pode ser alterado?
- O que acontece quando um motorista aceita uma entrega e depois fica indisponível?
- O que significa, para o negócio, considerar uma entrega encerrada?

---

## Atores envolvidos

Inicialmente, existem quatro grupos importantes envolvidos no processo.

### Cliente empresarial

É a empresa que contrata a RouteFlow para realizar uma entrega.

### Operador logístico

Acompanha a operação e intervém quando surgem problemas que não podem ser resolvidos automaticamente.

### Motorista

Executa a coleta e a entrega dos pacotes.

### Destinatário

É quem deve receber o pacote. Inicialmente, ele pode não interagir diretamente com o sistema.

> A existência desses atores não implica necessariamente que cada um deles represente um bounded context.

---

## Problemas atuais da empresa

Com o crescimento da operação, alguns problemas começaram a aparecer.

Os atendentes frequentemente não conseguem determinar com precisão em que situação uma entrega se encontra.

Às vezes um motorista recebe uma entrega que já havia sido atribuída a outro motorista.

Existem relatos de tentativas de entrega que não foram corretamente registradas.

Quando alguma coisa dá errado, é difícil reconstruir a sequência de acontecimentos que levou a entrega até aquela situação.

Além disso, começaram a surgir clientes maiores exigindo funcionalidades como:

- acompanhamento quase em tempo real;
- integração com seus próprios sistemas;
- maior confiabilidade das informações sobre as entregas.

Existem, portanto, dois tipos diferentes de complexidade envolvidos:

```text
Complexidade do negócio

+

Complexidade técnica
```

Durante a modelagem inicial, o objetivo é priorizar a compreensão da **complexidade do negócio** antes de tomar decisões técnicas.

---

## Precificação

A RouteFlow cobra seus clientes pelas entregas realizadas.

O cálculo do preço não é trivial e pode depender de fatores como:

- distância;
- tamanho do pacote;
- peso;
- urgência;
- região;
- características específicas do contrato do cliente.

Ainda não está definido se a precificação pertence ao mesmo domínio responsável pela execução da entrega ou se possui necessidades e regras próprias.

Essa é uma questão que deve ser investigada durante a análise do domínio.

---

## Ponto de partida para descoberta do domínio

Imagine que será realizada a primeira sessão de Event Storming da RouteFlow.

Não pense inicialmente em:

- classes;
- entidades;
- aggregates;
- banco de dados;
- APIs;
- filas;
- microserviços;
- frameworks.

Comece pela seguinte pergunta:

> **O que acontece desde o momento em que um cliente pede uma entrega até o momento em que nós consideramos nosso trabalho encerrado?**

A partir dela, tente identificar os acontecimentos relevantes do negócio utilizando linguagem no passado.

O objetivo inicial é **descobrir o domínio**, não desenhar a solução técnica.
