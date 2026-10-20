# Documentação Técnica e Guia de Estudo Individual — Pessoa B (Lucas)
# Projeto V.A.R.G — "Abdução Metódica" (Unity 6.3 LTS)

---

> **Autor e Desenvolvedor:** Lucas (Pessoa B)  
> **Responsabilidade no Projeto:** Arquiteto de Núcleo (Core), Gerenciamento de Missão e Inventário, Sistema Físico de Triagem na Nave (Sorting), Sistema de Humor/Falas do Chefe Alienígena, Nível Customizado via Planilha CSV e Interface Gráfica (UI da Pessoa B).  
> **Finalidade deste Documento:** Manual completo e guia preparatório para a apresentação oral e sabatina individual com o professor avaliador. Contém a explicação de cada classe, de cada método/função, onde o script é usado no Unity e as perguntas prováveis que o professor pode fazer.  
> **Escopo:** Exclusivamente os **44 scripts permanentes de gameplay, sistemas e dados** desenvolvidos por Lucas. Todos os scripts de geração procedural/builders de teste foram eliminados do projeto.

---

## Sumário
1. [Tabela Mestre dos 44 Scripts da Pessoa B](#1-tabela-mestre-dos-44-scripts-da-pessoa-b)
2. [Módulo 1: Core & Arquitetura (5 Scripts)](#2-módulo-1-core--arquitetura-5-scripts)
3. [Módulo 2: Missão, Inventário & Planilha CSV (5 Scripts)](#3-módulo-2-missão-inventário--planilha-csv-5-scripts)
4. [Módulo 3: Dados, Configurações & ScriptableObjects (13 Scripts)](#4-módulo-3-dados-configurações--scriptableobjects-13-scripts)
5. [Módulo 4: Sistema Físico de Triagem na Nave (12 Scripts)](#5-módulo-4-sistema-físico-de-triagem-na-nave-12-scripts)
6. [Módulo 5: Humor & Chefe Alienígena (3 Scripts)](#6-módulo-5-humor--chefe-alienígena-3-scripts)
7. [Módulo 6: Interface Gráfica / UI (6 Scripts)](#7-módulo-6-interface-gráfica--ui-6-scripts)
8. [Perguntas Transversais de Arquitetura para a Sabatina](#8-perguntas-transversais-de-arquitetura-para-a-sabatina)

---

## 1. Tabela Mestre dos 44 Scripts da Pessoa B

| Nº | Script (.cs) | Caminho no Projeto | Responsabilidade Principal |
|:---|:---|:---|:---|
| **01** | `Singleton<T>` | `Assets/_Project/Scripts/Core/Singleton.cs` | Classe base genérica MonoBehaviour com suporte a DontDestroyOnLoad |
| **02** | `PersistentBootstrapper` | `Assets/_Project/Scripts/Core/PersistentBootstrapper.cs` | Bootstrap automático do prefab `[Systems]` antes de qualquer cena carregar |
| **03** | `GameManager` | `Assets/_Project/Scripts/Core/GameManager.cs` | Máquina de estados macro, fluxo de fases, vitória/derrota e pontuação final |
| **04** | `SceneLoader` | `Assets/_Project/Scripts/Core/SceneLoader.cs` | Carregamento de cenas desacoplado e proteção contra carregamentos múltiplos |
| **05** | `GameEvents` | `Assets/_Project/Scripts/Core/GameEvents.cs` | Barramento de eventos estáticos C# para desacoplamento total entre sistemas |
| **06** | `InventoryManager` | `Assets/_Project/Scripts/Inventory/InventoryManager.cs` | Inventário persistente dos animais capturados com eventos reativos |
| **07** | `TimerManager` | `Assets/_Project/Scripts/Mission/TimerManager.cs` | Cronômetros de Caça e Triagem, aplicação de penalidades e acúmulo de dívida |
| **08** | `LivesManager` | `Assets/_Project/Scripts/Mission/LivesManager.cs` | Integridade da nave (5 vidas), perda de vidas e gatilho de derrota |
| **09** | `MissionManager` | `Assets/_Project/Scripts/Mission/MissionManager.cs` | Geração procedural da missão, alvos proibidos e validação única de captura |
| **10** | `SpreadsheetLevelLoader` | `Assets/_Project/Scripts/Mission/SpreadsheetLevelLoader.cs` | Leitura e validação de níveis customizados via planilha CSV/Excel (`CustomLevel.csv`) |
| **11** | `CampaignConfig` | `Assets/_Project/Scripts/Data/CampaignConfig.cs` | ScriptableObject com a lista ordenada de níveis da campanha principal |
| **12** | `CapturedItem` | `Assets/_Project/Scripts/Data/CapturedItem.cs` | Struct serializável contendo o TargetData e o resultado da captura |
| **13** | `CaptureResult` | `Assets/_Project/Scripts/Data/CaptureResult.cs` | Enum com os resultados de captura: `Correct`, `OutOfOrder`, `NotInList` |
| **14** | `DefeatReason` | `Assets/_Project/Scripts/Data/DefeatReason.cs` | Enum com os motivos de derrota: `NoLives`, `CaptureTimeUp`, `SortingTimeUp` |
| **15** | `GameConfig` | `Assets/_Project/Scripts/Data/GameConfig.cs` | ScriptableObject com opção para alternar o voo da nave |
| **16** | `LevelConfig` | `Assets/_Project/Scripts/Data/LevelConfig.cs` | ScriptableObject com parâmetros de cada fase (tempo, tamanho de lista, spawns) |
| **17** | `LevelResult` | `Assets/_Project/Scripts/Data/LevelResult.cs` | Struct com tempos restantes, vidas, erros e score final |
| **18** | `PenaltyConfig` | `Assets/_Project/Scripts/Data/PenaltyConfig.cs` | ScriptableObject com valores de penalidades em segundos e vidas |
| **19** | `ScoringConfig` | `Assets/_Project/Scripts/Data/ScoringConfig.cs` | ScriptableObject com multiplicadores de pontuação por tempo e vidas |
| **20** | `SpreadsheetLevelData` | `Assets/_Project/Scripts/Data/SpreadsheetLevelData.cs` | Classe de dados (POCO) que encapsula as configurações parseadas da planilha |
| **21** | `TargetData` | `Assets/_Project/Scripts/Data/TargetData.cs` | ScriptableObject com metadados do alvo (espécie, ícone, prefabs normal e mini) |
| **22** | `TargetType` | `Assets/_Project/Scripts/Data/TargetType.cs` | Enum das espécies de alvos: `Chicken`, `Cow`, `Pig`, `Human` |
| **23** | `TimerPhase` | `Assets/_Project/Scripts/Data/TimerPhase.cs` | Enum das fases temporizadas: `Capture` e `Sorting` |
| **24** | `SortingManager` | `Assets/_Project/Scripts/Sorting/SortingManager.cs` | Gerenciador da cena de triagem, registro de itens e condição de vitória |
| **25** | `CaptureBoxSpawner` | `Assets/_Project/Scripts/Sorting/CaptureBoxSpawner.cs` | Instanciação física dos mini-animais no chão da nave baseada no inventário |
| **26** | `SortableItem` | `Assets/_Project/Scripts/Sorting/SortableItem.cs` | Componente de cada mini-animal que rastreia estado físico, tipo e resolução |
| **27** | `GrabController` | `Assets/_Project/Scripts/Sorting/GrabController.cs` | Raio trator em primeira pessoa para manipular fisicamente os itens na nave |
| **28** | `Grabbable` | `Assets/_Project/Scripts/Sorting/Grabbable.cs` | Componente no mini-animal que reage à física do raio trator e arremesso |
| **29** | `TractorBeamVisual` | `Assets/_Project/Scripts/Sorting/TractorBeamVisual.cs` | Renderização visual da linha do raio trator usando `LineRenderer` |
| **30** | `Cage` | `Assets/_Project/Scripts/Sorting/Cage.cs` | Lógica da gaiola: aceita espécie correta ou ejeta com cuspe e penalidade |
| **31** | `CageTriggerZone` | `Assets/_Project/Scripts/Sorting/CageTriggerZone.cs` | Gatilho físico que detecta entrada do item e repassa à gaiola |
| **32** | `DiscardBin` | `Assets/_Project/Scripts/Sorting/DiscardBin.cs` | Lixeira/triturador: destrói itens sobressalentes ou pune descarte indevido |
| **33** | `DiscardTriggerZone` | `Assets/_Project/Scripts/Sorting/DiscardTriggerZone.cs` | Gatilho físico da lixeira que detecta entrada do item e repassa à lixeira |
| **34** | `KillZoneRespawn` | `Assets/_Project/Scripts/Sorting/KillZoneRespawn.cs` | Gatilho no fundo da nave que reposiciona itens ou jogador se caírem do mapa |
| **35** | `ETPlayerSortingController` | `Assets/_Project/Scripts/Sorting/ETPlayerSortingController.cs` | Movimentação em primeira pessoa e controle de câmera do jogador na triagem |
| **36** | `BossDialogueManager` | `Assets/_Project/Scripts/Humor/BossDialogueManager.cs` | Gerenciador de falas do chefe, cooldown, áudio e reação aos eventos de jogo |
| **37** | `BossLineSet` | `Assets/_Project/Scripts/Humor/BossLineSet.cs` | ScriptableObject com categorias de textos das falas do chefe |
| **38** | `BossExpression` | `Assets/_Project/Scripts/Humor/BossExpression.cs` | Enum dos estados emocionais do chefe: `Neutral`, `Smug`, `Angry`, `Furious`, `Annoyed` |
| **39** | `MainMenuController` | `Assets/_Project/Scripts/UI/MainMenuController.cs` | Menu principal da cena `00_Boot`: iniciar campanha, nível planilha, abrir CSV e sair |
| **40** | `BriefingController` | `Assets/_Project/Scripts/UI/BriefingController.cs` | Controlador da cena `01_BossBriefing`: apresentação da missão, diálogo e transição |
| **41** | `TargetShowcaseUI` | `Assets/_Project/Scripts/UI/TargetShowcaseUI.cs` | Exibição 3D dos modelos dos alvos em fila e barra de contagem de memorização |
| **42** | `DialogueBox` | `Assets/_Project/Scripts/UI/DialogueBox.cs` | Caixa de diálogo com áudio e avanço por clique/tecla de interação |
| **43** | `BossPopupUI` | `Assets/_Project/Scripts/UI/BossPopupUI.cs` | Balão de diálogo dinâmico na tela durante a caça e a triagem |
| **44** | `SortingTimerUI` | `Assets/_Project/Scripts/UI/SortingTimerUI.cs` | HUD da triagem mostrando tempo restante da fase e contagem de alvos pendentes |

---

## 2. Módulo 1: Core & Arquitetura (5 Scripts)

### 2.1 `Singleton<T>`
* **Onde é Usado:** Classe base abstrata herdada por todos os gerentes persistentes (`GameManager`, `SceneLoader`, `InventoryManager`, `TimerManager`, `LivesManager`, `MissionManager`, `BossDialogueManager`) e pelo gerente de cena `SortingManager`. Fica ativa no GameObject `[Systems]` (persistente) ou em instâncias locais.
* **Explicação da Classe:** Implementa o padrão de projeto Singleton genérico para Unity (`MonoBehaviour`), garantindo uma única instância global acessível via `Instance`, prevenção de duplicatas com `Destroy(gameObject)` e suporte a persistência entre cenas via `DontDestroyOnLoad`.
* **Funções:**
  * `Awake()`: Garante a unicidade da instância, destrói duplicatas e aplica `DontDestroyOnLoad` se `IsPersistent` for verdadeiro.
  * `OnDestroy()`: Limpa a referência estática `Instance` se o objeto destruído for a instância atual.
* **Possível pergunta do professor:**
  * *P: "Por que você usou um Singleton genérico (`where T : MonoBehaviour`) em vez de instanciar cada um manualmente?"*
  * *R:* "Para respeitar o princípio DRY (*Don't Repeat Yourself*). Centralizei a lógica de prevenção de duplicatas e persistência em uma única classe base abstrata herdada via CRTP (*Curiously Recurring Template Pattern*), evitando copiar e colar o mesmo código em 8 gerentes diferentes."

---

### 2.2 `PersistentBootstrapper`
* **Onde é Usado:** Executado automaticamente pela Unity antes do carregamento da primeira cena através da anotação `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`.
* **Explicação da Classe:** Garante que o prefab `Resources/Systems/[Systems]` seja instanciado em memória antes de qualquer cena rodar, permitindo que qualquer cena (ex: `04_ShipSorting` ou `03_CityHunt`) seja testada diretamente no Play Mode do Editor sem quebrar dependências de arquitetura.
* **Funções:**
  * `EnsureSystemsExist()`: Carrega o prefab `[Systems]` da pasta `Resources`, instancia no jogo se `GameManager.Instance` for nulo e marca como `DontDestroyOnLoad`.
* **Possível pergunta do professor:**
  * *P: "Se eu der Play direto na cena 04_ShipSorting no Unity Editor, o jogo quebra por falta de gerentes?"*
  * *R:* "Não, graças ao `PersistentBootstrapper` com `BeforeSceneLoad`. Ele detecta que o `GameManager` ainda não existe e instancia o prefab `[Systems]` antes de a cena inicializar seus objetos, garantindo independência total de teste entre cenas."

---

### 2.3 `GameManager`
* **Onde é Usado:** Fica acoplado no prefab `[Systems]` (persistente entre todas as cenas). Controla o fluxo global do jogo.
* **Explicação da Classe:** Atua como a Máquina de Estados macro do jogo, gerenciando o nível atual, o carregamento das cenas de Briefing, Caça, Triagem e Resultados, o cálculo do score final e os estados terminais de Vitória e Derrota.
* **Funções:**
  * `SetCustomSpreadsheetLevel(LevelConfig customConfig)`: Define uma configuração customizada de nível carregada da planilha CSV.
  * `ClearCustomSpreadsheetLevel()`: Limpa a configuração customizada para retornar à campanha padrão.
  * `Awake()`: Carrega ScriptableObjects de fallback (`CampaignConfig`, `GameConfig`, `ScoringConfig`, `PenaltyConfig`) da pasta `Resources` se não injetados pelo Inspector.
  * `GetCurrentLevelConfig()`: Retorna o `LevelConfig` atual da campanha ou o nível customizado da planilha se estiver ativo.
  * `StartLevel()`: Reinicia inventário, vidas, cronômetros, gera a missão no `MissionManager` e carrega a cena `01_BossBriefing`.
  * `GoToHunt()`: Transita da cena de Briefing para a cena de voo da nave (`02_ShipFlight`) ou caça na cidade (`03_CityHunt`) respeitando `GameConfig.UseShipFlight`.
  * `GoToSorting()`: Para o cronômetro de caça e carrega a cena de triagem `04_ShipSorting`.
  * `Victory()`: Para os cronômetros, calcula o resultado da partida, dispara o evento de vitória e carrega a cena `05_Results`.
  * `Defeat(DefeatReason reason)`: Para os cronômetros, armazena o motivo da derrota, dispara o evento de derrota e carrega a cena `05_Results`.
  * `NextLevel()`: Avança o nível atual e reinicia o loop, ou reinicia a campanha se atingir o nível máximo.
  * `RestartCampaign()`: Reseta o nível atual para 1 e inicia a fase.
  * `RetryLevel()`: Reinicia o nível atual mantendo o mesmo índice de fase.
  * `GoToMainMenu()`: Para cronômetros, reseta customizações e carrega a cena de menu `00_Boot`.
  * `CalculateAndStoreResult()`: Calcula a pontuação final com base no tempo restante e vidas preservadas, salvando a struct `LevelResult`.
* **Possível pergunta do professor:**
  * *P: "Onde fica a máquina de estados do jogo e como ela transita entre as fases?"*
  * *R:* "A máquina de estados macro fica centralizada no `GameManager`. Ela orquestra a sequência `Boot -> Briefing -> Hunt -> Sorting -> Results` chamando métodos explícitos (`StartLevel`, `GoToHunt`, `GoToSorting`, `Victory`, `Defeat`) e delegando o carregamento ao `SceneLoader`."

---

### 2.4 `SceneLoader`
* **Onde é Usado:** Fica acoplado no prefab `[Systems]` (persistente entre todas as cenas). É chamado por `GameManager` e botões de interface.
* **Explicação da Classe:** Responsável por encapsular as chamadas de troca de cena da Unity (`SceneManager.LoadScene`), protegendo o jogo contra chamadas simultâneas duplicadas durante transições.
* **Funções:**
  * `Load(string sceneName)`: Executa o carregamento síncrono da cena informada e bloqueia novas requisições enquanto a operação estiver em curso.
* **Possível pergunta do professor:**
  * *P: "Por que não chamar `SceneManager.LoadScene` direto nos botões de UI?"*
  * *R:* "Para manter o desacoplamento. Se precisarmos adicionar transições suaves (fade), telas de carregamento ou travas contra duplo-clique, alteramos apenas o `SceneLoader` sem precisar refatorar nenhuma tela de UI ou botão."

---

### 2.5 `GameEvents`
* **Onde é Usado:** Classe estática C#. É assinada e disparada por scripts de missão, física, triagem, humor e interface por todo o projeto.
* **Explicação da Classe:** Implementa o padrão de projeto *Observer* através de um barramento de eventos estáticos fortemente tipados (`System.Action`), permitindo comunicação totalmente desacoplada entre sistemas emissores e ouvintes sem dependências diretas de referência.
* **Funções:**
  * `ResetEvents()`: Executado na Unity 6.3 em `SubsystemRegistration` para anular todos os delegates estáticos ao entrar e sair do Play Mode, prevenindo vazamentos de memória.
  * `TriggerCorrectCapture(TargetData data)`: Notifica ouvintes sobre uma captura correta realizada na ordem esperada.
  * `TriggerOutOfOrderCapture(TargetData captured, TargetData expected)`: Notifica ouvintes sobre uma captura fora de ordem.
  * `TriggerNotInListCapture(TargetData data)`: Notifica ouvintes sobre a captura de um alvo proibido ou não listado.
  * `TriggerMissionComplete()`: Notifica que todos os alvos da cota de caça foram capturados.
  * `TriggerBeamDenied()`: Notifica que o raio da nave tentou abduzir um alvo proibido ou sem encolhimento.
  * `TriggerCorrectCage(TargetData data)`: Notifica que um animal foi colocado na gaiola correspondente à sua espécie.
  * `TriggerWrongCage(TargetData data)`: Notifica que um animal foi colocado na gaiola errada.
  * `TriggerCorrectDiscard(TargetData data)`: Notifica que um animal sobressalente/proibido foi triturado corretamente no descarte.
  * `TriggerWrongDiscard(TargetData data)`: Notifica que um animal correto necessário foi triturado indevidamente no descarte.
  * `TriggerLifeLost(int livesLeft)`: Notifica a perda de uma vida e informa a quantidade restante.
  * `TriggerTimeLow(TimerPhase phase)`: Notifica que o tempo da fase atual está em estado crítico (abaixo de 15 segundos).
  * `TriggerVictory()`: Notifica a vitória na fase.
  * `TriggerDefeat(DefeatReason reason)`: Notifica a derrota e informa a causa.
  * `TriggerIdle()`: Notifica que o jogador está ocioso há muito tempo sem realizar ações.
* **Possível pergunta do professor:**
  * *P: "Eventos estáticos em C# na Unity não causam vazamento de memória (*Memory Leak*) entre sessões de jogo no Editor?"*
  * *R:* "Sim, por padrão delegates estáticos retêm referências a objetos destruídos. Por isso, implementei o método `ResetEvents()` com a anotação obrigatória da Unity 6 `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`, que limpa todos os delegates antes de cada Play, e garanto que ouvintes assinem no `OnEnable` e desassinem no `OnDisable`."

---

## 3. Módulo 2: Missão, Inventário & Planilha CSV (5 Scripts)

### 3.1 `InventoryManager`
* **Onde é Usado:** Fica acoplado no prefab `[Systems]` (persistente entre cenas). É alimentado pelo `MissionManager` na Caça e lido pelo `CaptureBoxSpawner` na Triagem.
* **Explicação da Classe:** Gerencia a lista de itens abduzidos pelo jogador, preservando os dados intactos na transição da cena de Caça para a cena de Triagem na nave (atendendo diretamente ao item 8 do pipeline do professor).
* **Funções:**
  * `Add(CapturedItem item)`: Adiciona uma struct de item capturado à lista e dispara `OnItemAdded`.
  * `Add(TargetData data, CaptureResult result)`: Sobrecarga que cria a struct `CapturedItem` e a adiciona à lista.
  * `Remove(CapturedItem item)`: Remove um item da lista e dispara `OnItemRemoved`.
  * `RemoveAt(int index)`: Remove o item no índice especificado e dispara `OnItemRemoved`.
  * `Contains(TargetData data)`: Verifica se determinado ScriptableObject de alvo está contido no inventário.
  * `CountByType(TargetType type)`: Retorna a quantidade de itens presentes no inventário pertencentes à espécie consultada.
  * `Clear()`: Esvazia todos os itens do inventário e dispara `OnInventoryCleared`.
* **Possível pergunta do professor:**
  * *P: "Como você garantiu que a lista de animais capturados na caça não se perdesse ao carregar a cena de triagem?"*
  * *R:* "O `InventoryManager` herda de `Singleton<InventoryManager>`, cujo GameObject vive no prefab `[Systems]` sob `DontDestroyOnLoad`. Quando a cena `03_CityHunt` descarrega e a `04_ShipSorting` carrega, o `InventoryManager` permanece vivo na memória com a lista de `CapturedItem` intacta."

---

### 3.2 `TimerManager`
* **Onde é Usado:** Fica acoplado no prefab `[Systems]`. É consultado pelas UIs (`TimerUI`, `SortingTimerUI`) e manipulado por `GameManager`, `MissionManager`, `Cage` e `DiscardBin`.
* **Explicação da Classe:** Controla os cronômetros regressivos independentes das fases de Caça e Triagem, com cálculo de dívida de tempo acumulada, alertas de tempo baixo e aplicação de penalidades (atendendo ao item 13 do pipeline do professor).
* **Funções:**
  * `Update()`: Atualiza a passagem do tempo a cada frame para a fase ativa através do método `TickPhase`.
  * `TickPhase(...)`: Subtrai o delta de tempo, dispara eventos de alteração, aciona o alerta de tempo crítico e aciona a derrota se o cronômetro zerar.
  * `StartTimer(TimerPhase phase, float duration)`: Inicia o cronômetro da fase especificada, descontando a dívida acumulada (`SortingDebt`) no caso da Triagem.
  * `StopTimer(TimerPhase phase)`: Pausa o cronômetro da fase especificada.
  * `StopAllTimers()`: Pausa todos os cronômetros ativos simultaneamente.
  * `ResetTimers()`: Zera todos os tempos, cancela estados críticos e reseta a dívida de triagem.
  * `ApplyPenalty(TimerPhase phase, float seconds)`: Aplica penalidade subtraindo segundos da fase ativa ou acumulando em `SortingDebt` se a triagem ainda não começou.
  * `GetRemaining(TimerPhase phase)`: Retorna os segundos restantes da fase consultada.
  * `IsRunning(TimerPhase phase)`: Informa se o cronômetro da fase consultada está em execução.
  * `ResetDebt()`: Zera a dívida acumulada de tempo da triagem.
* **Possível pergunta do professor:**
  * *P: "O que é a 'dívida de tempo' (SortingDebt) e como ela funciona?"*
  * *R:* "Quando o jogador comete erros na fase de Caça abduzindo alvos proibidos, a penalidade não é apenas imediata: uma dívida de tempo é registrada no `TimerManager`. Quando a fase de Triagem inicia, o tempo total é subtraído dessa dívida (`duration - SortingDebt`), tornando a organização na nave mais desafiadora."

---

### 3.3 `LivesManager`
* **Onde é Usado:** Fica acoplado no prefab `[Systems]`. É manipulado por `MissionManager`, `DiscardBin` e lido por `LivesUI`.
* **Explicação da Classe:** Controla a integridade estrutural da nave (representada por 5 vidas/escudos). Cada erro grave reduz vidas e, ao atingir zero, dispara a derrota imediata por perda de vidas.
* **Funções:**
  * `ResetLives(int amount)`: Redefine a quantidade de vidas para o valor especificado e notifica os ouvintes via `OnLivesChanged`.
  * `LoseLife()`: Subtrai uma vida, dispara eventos de perda e aciona `GameManager.Instance.Defeat(DefeatReason.NoLives)` caso atinja zero.
* **Possível pergunta do professor:**
  * *P: "Quais ações no jogo fazem o jogador perder vidas?"*
  * *R:* "Abduzir um alvo proibido na caça (`MissionManager.ValidateCapture`), capturar alvos fora da ordem obrigatória, e triturar na lixeira da nave um animal que ainda era necessário para cumprir a cota das gaiolas (`DiscardBin.EjectDiscard`)."

---

### 3.4 `MissionManager`
* **Onde é Usado:** Fica acoplado no prefab `[Systems]`. É configurado no Briefing e chamado pela arma de abdução (`ShrinkGun`/`ShipBeamZone`) durante a Caça.
* **Explicação da Classe:** Centraliza com exclusividade as regras de negócio de validação de captura (SRP), a geração procedural da lista de alvos da missão, o sorteio da espécie proibida e o avanço da ordem requerida.
* **Funções:**
  * `Awake()`: Inicializa a instância e carrega a configuração de penalidades `PenaltyConfig` da pasta Resources se necessário.
  * `LoadDependenciesIfMissing()`: Garante que o ScriptableObject de penalidades esteja alocado na memória.
  * `GenerateMission(LevelConfig level)`: Sorteia aleatoriamente a espécie proibida, monta a lista ordenada de alvos com repetições permitidas e preenche a cota da fase.
  * `LoadCustomMission(IReadOnlyList<TargetType> targetSequence, TargetType forbiddenType)`: Carrega uma missão customizada importada diretamente da planilha CSV.
  * `GetTargetDataForType(TargetType type)`: Busca nos alvos disponíveis o `TargetData` correspondente à espécie consultada.
  * `IsForbidden(TargetType targetType)`: Retorna se a espécie informada é o alvo proibido da missão atual.
  * `GetRequiredCountForType(TargetType type)`: Calcula quantos animais da espécie informada são necessários para concluir a missão.
  * `ValidateCapture(TargetData captured)`: Valida uma captura contra a ordem esperada: se for proibido penaliza e pune vida; se for o alvo correto esperado avança o índice e armazena; se for fora de ordem pune tempo e vida.
  * `SetMissionDebug(List<TargetData> order, TargetType forbidden)`: Método auxiliar para injetar ordens controladas durante testes.
* **Possível pergunta do professor:**
  * *P: "Onde fica a regra que decide se o animal capturado estava certo ou errado? Ela está no player ou na arma?"*
  * *R:* "Fica estritamente no `MissionManager.ValidateCapture()`. O Player e a arma apenas detectam o alvo e passam a referência ao `MissionManager`. Isso respeita o princípio da Responsabilidade Única (SRP) e desacopla a física de tiro das regras de pontuação e missão."

---

### 3.5 `SpreadsheetLevelLoader`
* **Onde é Usado:** Classe utilitária estática chamada pelo `MainMenuController` e `BriefingController` para ler o arquivo `CustomLevel.csv`.
* **Explicação da Classe:** Realiza a leitura, criação de template e parse seguro de arquivos CSV/Excel externos, convertendo texto em dados de missão validados (`SpreadsheetLevelData`) e tratando vírgulas, ponto-e-vírgula e culturas de formato numérico.
* **Funções:**
  * `GetDefaultFilePath()`: Determina o caminho preferencial do arquivo `CustomLevel.csv` no diretório raiz do projeto ou na pasta persistente.
  * `EnsureTemplateExists(out string filePath)`: Cria automaticamente um modelo de planilha pré-preenchido e comentado caso o arquivo ainda não exista no disco.
  * `TryLoadCustomLevel(out SpreadsheetLevelData levelData, out string statusMessage)`: Lê o arquivo CSV linha por linha, faz o parse de chaves e valores, valida a sequência e verifica se o alvo proibido não está na lista.
  * `NormalizeKey(string raw)`: Remove acentos, espaços e padroniza a chave do parâmetro para minúsculas.
  * `SplitRow(string line, char delimiter)`: Divide as colunas do CSV respeitando aspas que contenham delimitadores internos.
  * `ParseTargetSequence(string value, List<TargetType> sequence)`: Converte a string de alvos separados por vírgula em uma lista fortemente tipada de `TargetType`.
  * `TryParseTargetType(string value, out TargetType type)`: Converte um nome textual em inglês ou português para o enum `TargetType`.
* **Possível pergunta do professor:**
  * *P: "Como o jogo trata erros se o usuário editar a planilha e digitar valores inválidos ou colocar o alvo proibido dentro da sequência?"*
  * *R:* "O método `TryLoadCustomLevel` realiza uma validação rigorosa. Se a sequência estiver vazia, ou se o `ForbiddenTarget` fizer parte da `TargetSequence`, ele rejeita o carregamento, retorna falso e entrega uma mensagem explicativa exibida diretamente na interface e no balão do chefe."

---

## 4. Módulo 3: Dados, Configurações & ScriptableObjects (13 Scripts)

### 4.1 `CampaignConfig`
* **Onde é Usado:** Criado como asset em `Assets/_Project/Data/CampaignConfig.asset`, injetado no `GameManager`.
* **Explicação da Classe:** ScriptableObject que contém o array ordenado de todos os `LevelConfig` que compõem a campanha principal do jogo.
* **Funções / Propriedades:**
  * `Levels`: Propriedade pública somente leitura que expõe o array de configurações de nível da campanha.
* **Possível pergunta do professor:**
  * *P: "Como você adicionaria mais 5 fases ao jogo sem mexer em nenhuma linha de código?"*
  * *R:* "Basta criar 5 novos assets de `LevelConfig` no Project window, abrir o asset `CampaignConfig` e adicioná-los ao array `Levels`. O `GameManager` lerá os novos níveis automaticamente."

---

### 4.2 `CapturedItem`
* **Onde é Usado:** Struct utilizada pelo `InventoryManager`, `CaptureBoxSpawner` e `SortableItem`.
* **Explicação da Classe:** Estrutura de dados serializável (DTO) leve e imutável que empacota o `TargetData` capturado e o resultado da sua captura (`CaptureResult`), sem sobrecarga de `MonoBehaviour`.
* **Funções:**
  * `CapturedItem(TargetData data, CaptureResult result)`: Construtor que inicializa os campos somente leitura do item.
* **Possível pergunta do professor:**
  * *P: "Por que você usou `struct` e não `class` para o `CapturedItem`?"*
  * *R:* "Por ser um objeto de transferência de dados (DTO) pequeno e imutável. Como tipo por valor (*value type*), seu armazenamento em listas e arrays é contíguo na memória, reduzindo a fragmentação do Garbage Collector."

---

### 4.3 `CaptureResult`
* **Onde é Usado:** Enum referenciado em `CapturedItem`, `MissionManager`, `GameEvents` e `InventoryManager`.
* **Explicação da Classe:** Define os três possíveis estados de uma captura de animal: `Correct` (capturado na ordem certa), `OutOfOrder` (pertence à missão, mas capturado na ordem errada) e `NotInList` (animal proibido ou não solicitado).

---

### 4.4 `DefeatReason`
* **Onde é Usado:** Enum referenciado em `GameManager`, `GameEvents` e `BossDialogueManager`.
* **Explicação da Classe:** Especifica a causa do encerramento da partida em derrota: `NoLives` (esgotamento das vidas), `CaptureTimeUp` (tempo de caça esgotado) ou `SortingTimeUp` (tempo de triagem esgotado).

---

### 4.5 `GameConfig`
* **Onde é Usado:** Asset em `Assets/_Project/Data/GameConfig.asset`, injetado no `GameManager`.
* **Explicação da Classe:** ScriptableObject com opções globais de execução, incluindo o toggle booleano `UseShipFlight` que define se a fase de Caça deve ser precedida pelo voo com a nave.
* **Funções / Propriedades:**
  * `UseShipFlight`: Propriedade booleana que habilita ou pula a cena `02_ShipFlight`.

---

### 4.6 `LevelConfig`
* **Onde é Usado:** Assets de nível da campanha e instâncias dinâmicas geradas no carregamento da planilha CSV.
* **Explicação da Classe:** ScriptableObject que define as regras e o balanceamento de um nível específico: tamanho da lista de alvos, tempo de caça, tempo de triagem, alvos extras na cidade e multiplicador de velocidade de fuga.
* **Funções:**
  * `Setup(...)`: Método utilitário que permite preencher todos os campos do `LevelConfig` em tempo de execução ao instanciar dados vindos do CSV.

---

### 4.7 `LevelResult`
* **Onde é Usado:** Struct gerada pelo `GameManager` ao fim da partida e lida pela tela de resultados `ResultsScreen`.
* **Explicação da Classe:** Estrutura pura que consolida todas as estatísticas da partida: tempo restante de caça, tempo restante de triagem, vidas finais, erros cometidos, score total calculado e estado de vitória.
* **Funções:**
  * `LevelResult(...)`: Construtor que inicializa todas as métricas consolidadas da partida.

---

### 4.8 `PenaltyConfig`
* **Onde é Usado:** Asset em `Assets/_Project/Data/PenaltyConfig.asset`, injetado em `MissionManager`, `Cage` e `DiscardBin`.
* **Explicação da Classe:** ScriptableObject que centraliza todos os valores de balanceamento de penalidades (segundos perdidos por captura errada, dívida de triagem, penalidade de gaiola errada, penalidade de descarte incorreto e vidas iniciais).

---

### 4.9 `ScoringConfig`
* **Onde é Usado:** Asset em `Assets/_Project/Data/ScoringConfig.asset`, injetado no `GameManager`.
* **Explicação da Classe:** ScriptableObject que define a fórmula de cálculo da pontuação: pontos por segundo de tempo poupado e pontos por vida preservada.

---

### 4.10 `SpreadsheetLevelData`
* **Onde é Usado:** Classe pura (POCO) instanciada pelo `SpreadsheetLevelLoader`.
* **Explicação da Classe:** Modelo de dados em memória que encapsula os valores extraídos da planilha `CustomLevel.csv` antes de serem convertidos em ScriptableObjects pelo `GameManager`.

---

### 4.11 `TargetData`
* **Onde é Usado:** Assets individuais para cada espécie (`Target_Chicken.asset`, `Target_Cow.asset`, `Target_Pig.asset`, `Target_Human.asset`).
* **Explicação da Classe:** ScriptableObject que descreve uma espécie de alvo: enum `TargetType`, nome exibido, ícone de interface, prefab do animal em tamanho real na cidade e `MiniPrefab` correspondente usado na triagem da nave.

---

### 4.12 `TargetType`
* **Onde é Usado:** Enum universal referenciado em quase todos os sistemas do jogo.
* **Explicação da Classe:** Enumeração fortemente tipada das 4 espécies existentes no jogo: `Chicken`, `Cow`, `Pig` e `Human`.

---

### 4.13 `TimerPhase`
* **Onde é Usado:** Enum referenciado em `TimerManager`, `GameEvents` e interfaces de tempo.
* **Explicação da Classe:** Identificador das duas fases temporizadas do jogo: `Capture` (Caça na cidade) e `Sorting` (Triagem no interior da nave).

---

## 5. Módulo 4: Sistema Físico de Triagem na Nave (12 Scripts)

### 5.1 `SortingManager`
* **Onde é Usado:** Fica no GameObject `SortingManager` na cena `04_ShipSorting`.
* **Explicação da Classe:** Gerencia o ciclo de vida da cena de triagem (atendendo ao item 10 do pipeline do professor). Registra todos os mini-animais soltos na nave, rastreia quantos já foram guardados em gaiolas ou triturados, e dispara a vitória da fase quando a contagem de pendências chega a zero.
* **Funções:**
  * `Start()`: Localiza o `GrabController`, escaneia os itens na cena e aciona o cronômetro de triagem no `TimerManager`.
  * `ScanSceneItems()`: Busca todos os componentes `SortableItem` presentes na cena e os registra.
  * `ClearItems()`: Limpa as listas de registro e resolução de itens.
  * `UnregisterItem(SortableItem item)`: Remove um item do registro caso seja destruído ou desativado.
  * `RegisterItem(SortableItem item)`: Adiciona um item à lista de itens a serem triados e atualiza contadores de UI.
  * `NotifyItemResolved(SortableItem item)`: Marca um item como resolvido (engaiolado ou triturado) e inicia a corrotina de vitória se não houver mais pendências.
  * `GetUnsortedCountForType(TargetType type)`: Retorna a quantidade de itens ainda não resolvidos de uma espécie específica.
  * `VictoryRoutine()`: Aguarda o jogador soltar qualquer item segurado e chama `GameManager.Instance.Victory()`.
  * `OnDestroy()`: Para o cronômetro de triagem ao descarregar a cena.
* **Possível pergunta do professor:**
  * *P: "O que acontece se o jogador colocar todos os animais nas gaiolas certas enquanto ainda estiver segurando um animal no raio trator?"*
  * *R:* "A corrotina `VictoryRoutine()` verifica `while (_grabController.IsHolding) yield return null;`. Ela aguarda o jogador arremessar ou soltar o item antes de congelar a física e transicionar para a tela de vitória, evitando que a cena mude abruptamente com um objeto preso no ar."

---

### 5.2 `CaptureBoxSpawner`
* **Onde é Usado:** Fica acoplado no GameObject/Prefab `CaptureBox` dentro da cena `04_ShipSorting`.
* **Explicação da Classe:** Executa a mecânica inversa de depósito dos objetos trazidos da caça (item 10 do pipeline do professor). Lê a lista do `InventoryManager` e instancia os `MiniPrefab` com física ativa diretamente no chão da nave, garantindo altura mínima para evitar colisões com o piso.
* **Funções:**
  * `Awake()`: Localiza a âncora de spawn `SpawnAnchor` se não tiver sido vinculada pelo Inspector.
  * `Start()`: Dispara a geração de itens se o `InventoryManager` estiver presente na sessão.
  * `SpawnExistingItems()`: Limpa objetos anteriores e instancia um mini-animal para cada registro do inventário.
  * `SpawnItem(CapturedItem item)`: Instancia o mini-prefab na posição calculada, adiciona `SortableItem` e `Grabbable`, ajusta a layer para `Grabbable` e registra no `SortingManager`.
  * `ClearSpawned()`: Destrói todas as instâncias de mini-animais geradas anteriormente.
* **Possível pergunta do professor:**
  * *P: "Como os animais aparecem na nave na fase de triagem?"*
  * *R:* "O `CaptureBoxSpawner` lê a lista de `CapturedItem` mantida pelo `InventoryManager`. Para cada item, ele instancia o `MiniPrefab` correspondente com uma leve variação aleatória de posição e rotação sobre o ponto de spawn (`SpawnAnchor`), garantindo que todos os animais abduzidos na cidade materializem fisicamente na nave."

---

### 5.3 `SortableItem`
* **Onde é Usado:** Componente anexado dinamicamente ou pré-configurado na raiz de cada miniatura de animal instanciada na nave (`Mini_Cow`, `Mini_Pig`, `Mini_Chicken`, `Mini_Human`).
* **Explicação da Classe:** Rastreia os metadados e o estado de triagem do mini-animal: se está sendo segurado pelo jogador (`IsHeld`), se já foi resolvido em uma gaiola ou lixeira (`IsSorted`) e o tempo transcorrido desde que foi solto.
* **Funções:**
  * `MarkSorted()`: Marca o item como resolvido em definitivo, impedindo que seja manipulado ou validado novamente.
  * `Initialize(CapturedItem capturedItem)`: Inicializa os dados do item a partir da captura original da caça.
  * `OnDestroy()`: Notifica o `SortingManager` para desregistrar o item se for destruído antes da resolução.
* **Possível pergunta do professor:**
  * *P: "Por que uma gaiola não pode engaiolar um animal enquanto o jogador ainda estiver segurando ele no raio?"*
  * *R:* "Porque o `SortableItem` expõe a propriedade `IsHeld`. As gaiolas e a lixeira checam `if (item.IsHeld) return;` no trigger. O animal só é processado fisicamente quando o jogador realmente solta o botão do mouse."

---

### 5.4 `GrabController`
* **Onde é Usado:** Fica anexado no prefab do jogador `ETPlayer_Sorting` na cena `04_ShipSorting`.
* **Explicação da Classe:** Implementa a mecânica de manipulação física em primeira pessoa com raio trator / grabbing (item 11 do pipeline do professor). Lança um raycast a partir da câmera, captura o `Grabbable`, mantém o objeto flutuando à frente do jogador via forças físicas no `FixedUpdate` e permite arremessá-lo com impulso.
* **Funções:**
  * `Awake()`: Obtém referências à câmera do jogador, à máscara de camada `Grabbable` e ao script visual do raio.
  * `Configure(Camera playerCamera, TractorBeamVisual beamVisual)`: Método de injeção de dependências para configurar câmera e visual do raio.
  * `Update()`: Lê o input do jogador, gerencia o agarrar/soltar/arremessar e atualiza a renderização da linha do raio.
  * `FixedUpdate()`: Aplica força física contínua no `Rigidbody` do objeto segurado para que ele acompanhe suavemente a mira da câmera.
  * `UpdateGrabbing(IPlayerInput input)`: Dispara o raycast na câmera para pegar objetos ou calcula o vetor de impulso ao soltar/arremessar.
  * `DropHeld()`: Solta imediatamente qualquer objeto que esteja sendo manipulado e desativa o raio visual.
  * `UpdateBeamVisual()`: Atualiza as coordenadas de início e fim da linha do raio trator.
  * `GetInput()`: Retorna o input do jogador via `InputManager` ou utiliza o fallback de teclado/mouse.
  * `OnDisable()`: Garante que objetos sejam soltos se o script for desativado.
* **Possível pergunta do professor:**
  * *P: "Você usou `transform.position` ou física para mover o objeto agarrado?"*
  * *R:* "Utilizei estritamente física (`FixedUpdate` e `Rigidbody.linearVelocity`). Modificar o `transform.position` diretamente faria o animal atravessar paredes da nave e outros colliders (*tunnelling*). Calculando a velocidade proporcional à distância (`delta * followForce`), o objeto respeita colliders e colisões do cenário."

---

### 5.5 `Grabbable`
* **Onde é Usado:** Anexado a cada miniatura de animal manipulável na cena de triagem.
* **Explicação da Classe:** Componente que confere capacidades de agarre ao GameObject. Manipula as propriedades físicas do `Rigidbody` (desliga a gravidade ao ser puxado, anula velocidades residuais e aplica impulsos ao ser lançado).
* **Funções:**
  * `Awake()`: Armazena as referências locais ao `Rigidbody` e ao `SortableItem`.
  * `OnGrabbed()`: Desliga a gravidade, zera velocidades e avisa o `SortableItem` que o objeto está preso no raio.
  * `OnReleased(Vector3 impulse)`: Restaura a gravidade, notifica o `SortableItem` e aplica a força de impulso caso o jogador tenha arremessado o animal.
  * `FollowTarget(Vector3 targetPosition, float followForce)`: Ajusta a `linearVelocity` para deslocar o objeto em direção ao alvo e amortece a rotação angular.
  * `SetHighlight(bool enable)`: Ponto de extensão para feedback visual de realce.
* **Possível pergunta do professor:**
  * *P: "Por que você usou `Rigidbody.linearVelocity` em vez de `velocity`?"*
  * *R:* "Porque `velocity` foi descontinuada (deprecada) na Unity 6. A API padrão atual exige o uso explícito de `linearVelocity` e `angularVelocity` para maior clareza e desempenho no motor de física PhysX."

---

### 5.6 `TractorBeamVisual`
* **Onde é Usado:** Fica acoplado no GameObject do jogador `ETPlayer_Sorting` ou na ponta da sua arma/visualizador.
* **Explicação da Classe:** Responsável pelo feedback visual do raio trator, utilizando um componente `LineRenderer` com 2 vértices (ponta da arma/câmera até o ponto de contato no animal).
* **Funções:**
  * `Awake()`: Configura os 2 pontos iniciais do `LineRenderer` e o desativa por padrão.
  * `Configure(...)`: Permite vincular a referência do `LineRenderer` e a âncora de saída do raio (`MuzzlePoint`).
  * `UpdateBeam(Vector3 currentItemPos)`: Ativa o `LineRenderer` e posiciona o vértice 0 na ponta da arma e o vértice 1 no centro do animal manipulado.
  * `HideBeam()`: Desativa a renderização da linha do raio.
  * `OnDisable()`: Oculta o raio caso o componente seja desativado.
* **Possível pergunta do professor:**
  * *P: "Como é desenhado o raio do raio trator?"*
  * *R:* "Com um componente nativo `LineRenderer` configurado com 2 vértices (`positionCount = 2`). Quando o `GrabController` está segurando um item, ele chama `UpdateBeam()` passando a posição do objeto a cada frame, ligando a ponta da câmera ao animal."

---

### 5.7 `Cage`
* **Onde é Usado:** Anexado aos prefabs das 4 gaiolas (`Cage_Chicken`, `Cage_Cow`, `Cage_Pig`, `Cage_Human`) posicionadas na nave na cena `04_ShipSorting`.
* **Explicação da Classe:** Implementa a validação com trigger box e colliders para testar se o objeto é o correto (item 12 do pipeline do professor). Se for a espécie correta e houver cota disponível, a gaiola aceita o animal, trava seu Rigidbody e toca o áudio de sucesso. Se for a espécie errada ou um alvo proibido, ejeta o animal com um impulso cômico para fora e aplica penalidade de tempo.
* **Funções:**
  * `Awake()`: Carrega a configuração de penalidades e a fonte de áudio.
  * `Configure(...)`: Permite injetar a espécie aceita pela gaiola e os pontos de encaixe (`SnapPoint`) e ejeção (`EjectPoint`).
  * `ProcessTriggerStay(Collider other)`: Ignora objetos que ainda estejam sendo segurados pelo raio e decide entre aceitar (`AcceptItem`) ou cuspir (`EjectItem`).
  * `OnTriggerStay(Collider other)`: Redireciona a detecção física para o método `ProcessTriggerStay`.
  * `AcceptItem(SortableItem item)`: Trava a física do animal em kinematic, move para o `SnapPoint`, toca o áudio de acerto, emite partículas e notifica o `SortingManager`.
  * `EjectItem(SortableItem item)`: Aplica um impulso físico para trás e para cima expulsando o animal, toca o som de erro e aplica penalidade de tempo no `TimerManager`.
* **Possível pergunta do professor:**
  * *P: "O que acontece se o jogador tentar colocar mais vacas na gaiola de vacas do que a missão pediu?"*
  * *R:* "A gaiola verifica a cota da missão através de `_lockedItems.Count < quota`. Se a cota para aquela espécie já tiver sido atingida, a gaiola trata o animal sobressalente como incorreto e o ejeta com cuspe e penalidade, forçando o jogador a jogá-lo no triturador de descarte (`DiscardBin`)."

---

### 5.8 `CageTriggerZone`
* **Onde é Usado:** Fica acoplado no GameObject filho de cada gaiola que contém o `Collider` configurado como `IsTrigger`.
* **Explicação da Classe:** Atua como proxy de colisão. Detecta o evento `OnTriggerStay` e repassa o colisor para a classe mãe `Cage.ProcessTriggerStay(other)`.
* **Funções:**
  * `Awake()`: Localiza automaticamente o componente `Cage` no objeto pai.
  * `Configure(Cage cage)`: Injeta a referência da gaiola pai.
  * `OnTriggerStay(Collider other)`: Repassa o colisor detectado para o método de processamento da gaiola.
* **Possível pergunta do professor:**
  * *P: "Por que separar `Cage` e `CageTriggerZone` em classes diferentes?"*
  * *R:* "Para manter a flexibilidade física no prefab. O `Collider` de gatilho muitas vezes precisa estar em um objeto filho com escala e rotação específicas na boca da gaiola, enquanto o script principal fica na raiz. O trigger zone serve como uma ponte limpa sem sujar a hierarquia com `GetComponentInParent` a cada frame."

---

### 5.9 `DiscardBin`
* **Onde é Usado:** Anexado ao prefab da lixeira / triturador de descarte `DiscardBin` na cena `04_ShipSorting`.
* **Explicação da Classe:** Implementa a lógica do triturador de descarte na nave. Se o animal descartado for um alvo proibido ou uma sobra desnecessária para a cota, ele tritura o objeto com segurança. Se o jogador tentar triturar um animal que ainda era necessário para cumprir a cota da missão, a lixeira ejeta o animal de volta e pune o jogador com perda de vida e tempo.
* **Funções:**
  * `Awake()`: Inicializa a configuração de penalidades e a fonte de áudio.
  * `Configure(Transform destroyPoint)`: Define o ponto de destruição do objeto.
  * `ProcessTriggerStay(Collider other)`: Ignora animais segurados e valida se o descarte é legítimo (`IsValidDiscard`).
  * `OnTriggerStay(Collider other)`: Redireciona o gatilho físico para `ProcessTriggerStay`.
  * `AcceptDiscard(SortableItem item)`: Destrói o GameObject do animal com pequeno atraso, toca o áudio de triturador, emite partículas e notifica o `SortingManager`.
  * `EjectDiscard(SortableItem item)`: Ejeta o animal com força física, toca o som de erro, retira uma vida no `LivesManager` e aplica penalidade no `TimerManager`.
  * `ConfigureFeedback(...)`: Permite vincular clipes de áudio e partículas de descarte via código.
  * `IsValidDiscard(SortableItem item)`: Calcula matematicamente se o animal é proibido ou se a quantidade disponível somada à já engaiolada ultrapassa a cota necessária da missão.
* **Possível pergunta do professor:**
  * *P: "Como a lixeira sabe que um animal pode ou não ser destruído?"*
  * *R:* "Através do método `IsValidDiscard()`. Se for da espécie proibida, o descarte é sempre válido. Caso contrário, ela soma quantos animais daquela espécie já foram engaiolados com quantos ainda estão soltos na nave. Se essa soma for maior que a cota exigida pela missão, o animal é uma sobra e pode ser triturado; caso contrário, é punido com perda de vida."

---

### 5.10 `DiscardTriggerZone`
* **Onde é Usado:** Fica acoplado no GameObject filho com collider trigger na boca da lixeira `DiscardBin`.
* **Explicação da Classe:** Atua como proxy de colisão para a lixeira, detectando `OnTriggerStay` e repassando o colisor para `DiscardBin.ProcessTriggerStay(other)`.
* **Funções:**
  * `Awake()`: Localiza o script `DiscardBin` no objeto pai.
  * `Configure(DiscardBin bin)`: Injeta a referência da lixeira pai.
  * `OnTriggerStay(Collider other)`: Transmite o colisor detectado para a lixeira.

---

### 5.11 `KillZoneRespawn`
* **Onde é Usado:** Fica em um grande collider trigger posicionado abaixo do piso da nave na cena `04_ShipSorting`.
* **Explicação da Classe:** Sistema de segurança de física (*fail-safe*). Se algum animal ou o próprio jogador atravessar o chão por falha de colisão e cair no vácuo espacial, o script intercepta o objeto e o reposiciona de volta no centro da nave com velocidade zerada.
* **Funções:**
  * `OnTriggerEnter(Collider other)`: Detecta a entrada de qualquer colisor na zona de queda, zera a `linearVelocity` do Rigidbody ou desativa temporariamente o `CharacterController` e reposiciona a entidade no `RespawnPoint`.
* **Possível pergunta do professor:**
  * *P: "O que acontece se a física do PhysX bugar e uma miniatura de porco atravessar o chão da nave?"*
  * *R:* "Ela cai na trigger box do `KillZoneRespawn` embaixo do piso. O script detecta a queda, anula a velocidade física (`linearVelocity = Vector3.zero`) e teletransporta o animal instantaneamente de volta ao ponto seguro no chão da nave (`RespawnPoint`), garantindo que a fase nunca se torne invencível."

---

### 5.12 `ETPlayerSortingController`
* **Onde é Usado:** Anexado ao prefab `ETPlayer_Sorting` na cena `04_ShipSorting`.
* **Explicação da Classe:** Controla a movimentação e a câmera em primeira pessoa do alienígena especificamente no ambiente interno e fechado da nave, utilizando `CharacterController` para deslocamento sem rolamento e clamping vertical de rotação da câmera.
* **Funções:**
  * `Awake()`: Obtém o `CharacterController` e inicializa o fallback de input de teclado e mouse.
  * `OnEnable()`: Trava o cursor do mouse no centro da tela para navegação em primeira pessoa.
  * `OnDisable()`: Libera o cursor do mouse.
  * `Update()`: Atualiza a rotação da visão (`HandleLook`), a movimentação com gravidade (`HandleMovement`) e a tecla de alternância do cursor (`HandleCursorToggle`).
  * `Configure(...)`: Permite injetar a referência da câmera e a velocidade de caminhada.
  * `HandleLook()`: Aplica a rotação horizontal (*yaw*) no corpo do jogador e a rotação vertical (*pitch*) com limites angulares (*clamp*) na câmera.
  * `HandleMovement()`: Calcula o vetor de movimento baseado no plano local do jogador e aplica a gravidade no `CharacterController`.
  * `HandleCursorToggle()`: Permite destravar o cursor do mouse ao pressionar Pause/Esc.
  * `ReadMoveInput()`: Lê o eixo bidimensional de movimento a partir do `IPlayerInput`.
  * `ReadLookInput()`: Lê o delta do mouse a partir do `IPlayerInput`.
  * `GetInput()`: Retorna o provedor de entrada ativo via `InputManager` ou o fallback local.
  * `LockCursor(bool locked)`: Trava ou destrava o cursor do sistema operacional.
* **Possível pergunta do professor:**
  * *P: "Por que você usou `CharacterController` para o jogador na triagem e não um `Rigidbody`?"*
  * *R:* "Porque o `CharacterController` oferece controle de movimento em primeira pessoa perfeito e determinístico em ambientes com piso plano e paredes fechadas, sem sofrer com torque, tombamento ou fricção indesejada contra as quinas dos consoles e gaiolas da nave."

---

## 6. Módulo 5: Humor & Chefe Alienígena (3 Scripts)

### 6.1 `BossDialogueManager`
* **Onde é Usado:** Fica no prefab `[Systems]` (persistente). Reage a eventos em todas as cenas do jogo e gerencia a voz e o humor do comandante Zorp (item 15 do pipeline do professor).
* **Explicação da Classe:** Gerenciador central de humor do jogo. Assina os eventos do `GameEvents`, seleciona falas cômicas aleatórias baseadas no contexto (acertos, erros, alvos proibidos, tempo acabando, perda de vidas), gerencia cooldown de fala para não sobrecarregar o áudio e emite eventos para os balões de diálogo.
* **Funções:**
  * `ResetStaticEvents()`: Limpa os delegates de eventos estáticos no ciclo `SubsystemRegistration`.
  * `Awake()`: Garante a presença do `AudioSource` e desativa o playOnAwake.
  * `OnEnable()`: Assina todos os 13 eventos do barramento `GameEvents`.
  * `OnDisable()`: Desassina rigorosamente todos os 13 eventos do `GameEvents`.
  * `HandleCorrectCapture(TargetData d)`: Dispara fala de elogio sarcástico quando o jogador acerta uma captura.
  * `HandleOutOfOrderCapture(TargetData c, TargetData e)`: Dispara fala de bronca quando o jogador erra a ordem requerida.
  * `HandleNotInListCapture(TargetData d)`: Dispara fala de fúria quando o jogador abduz um alvo proibido.
  * `HandleBeamDenied()`: Dispara fala irônica quando a nave tenta puxar algo inválido.
  * `HandleCorrectCage(TargetData d)`: Elogia quando o jogador acerta a gaiola na triagem.
  * `HandleWrongCage(TargetData d)`: Xinga quando o jogador erra a gaiola.
  * `HandleCorrectDiscard(TargetData d)`: Comenta a trituração de uma sobra na lixeira.
  * `HandleWrongDiscard(TargetData d)`: Desespera-se quando o jogador tritura um animal necessário.
  * `HandleTimeLow(TimerPhase p)`: Apressa o jogador quando o tempo está abaixo de 15 segundos.
  * `HandleVictory()`: Dispara comemoração pomposa na vitória com alta prioridade.
  * `HandleDefeat(DefeatReason r)`: Dispara humilhação final na derrota com alta prioridade.
  * `HandleIdle()`: Reclama quando o jogador passa tempo sem interagir.
  * `Speak(string rawLine, BossExpression expression, bool highPriority)`: Valida cooldown, atualiza a última fala e dispara o áudio e os eventos de exibição de UI.
  * `PlayVoice()`: Executa o clipe de áudio de voz do chefe no `AudioSource`.
  * `PickLine(string categoryKey, string[] lines)`: Sorteia aleatoriamente uma string de um array de falas.
  * `SetLineSet(BossLineSet lineSet)`: Permite injetar um novo conjunto de falas via ScriptableObject.
  * `SpeakRandom(...)`: Substitui os tokens `{captured}` e `{expected}` pelos nomes dos animais e chama `Speak()`.
  * `HandleLifeLost(int livesLeft)`: Escala o desespero e a raiva do chefe conforme as vidas diminuem (Tier 1 a Tier 3).
* **Possível pergunta do professor:**
  * *P: "Como o sistema de humor reage dinamicamente aos erros do jogador sem poluir a tela de falas o tempo todo?"*
  * *R:* "Através de um temporizador de cooldown (`_minCooldown = 3.5f`). A menos que seja um evento crítico de alta prioridade (como vitória, derrota ou perda de vida), falas normais só são emitidas se tiver transcorrido o intervalo de cooldown, impedindo que o chefe fale em cima de si mesmo."

---

### 6.2 `BossLineSet`
* **Onde é Usado:** Criado como asset em `Assets/_Project/Data/Humor/BossLineSet.asset` e injetado no `BossDialogueManager`.
* **Explicação da Classe:** ScriptableObject com categorias organizadas de arrays de strings em português com as piadas e diálogos do chefe alienígena comandante Zorp, utilizando os coringas `{captured}` e `{expected}` para interpolação dinâmica.
* **Campos / Categorias:**
  * Falas de captura correta, fora de ordem, não listado, raio negado, gaiola correta, gaiola errada, descarte correto, descarte errado, tempo baixo, vitória, derrota, ociosidade e três níveis progressivos de perda de vidas (`Tier 1`, `Tier 2`, `Tier 3`).

---

### 6.3 `BossExpression`
* **Onde é Usado:** Enum compartilhado entre `BossDialogueManager`, `BossPopupUI` e `DialogueBox`.
* **Explicação da Classe:** Enumeração que representa o estado de humor visual do chefe alienígena: `Neutral` (neutro), `Smug` (presunçoso/satisfeito), `Angry` (irritado), `Furious` (enfurecido) e `Annoyed` (entediado/impaciente).

---

## 7. Módulo 6: Interface Gráfica / UI (6 Scripts)

### 7.1 `MainMenuController`
* **Onde é Usado:** Fica na cena `00_Boot`. Controla os botões e títulos do menu inicial do jogo.
* **Explicação da Classe:** Gerencia a tela inicial (item 14 do pipeline do professor). Fornece botões para iniciar a Campanha, carregar um nível customizado da Planilha CSV, abrir o arquivo CSV diretamente no editor externo do sistema operacional (Excel/Notepad) e sair do jogo.
* **Funções:**
  * `EnsureMainMenuInBootScene()`: Método estático `AfterSceneLoad` que garante a presença do GameObject `MainMenuController` na cena `00_Boot`.
  * `Start()`: Configura cursor do mouse, interface e garante que o template do CSV exista em disco.
  * `OnDestroy()`: Remove os listeners dos 4 botões para evitar vazamentos de eventos de UI.
  * `Configure(...)`: Permite injetar programaticamente os botões e textos da UI.
  * `SetupListeners()`: Registra os métodos de clique nos botões da interface.
  * `PlayCampaign()`: Limpa customizações da planilha e inicia a campanha através do `GameManager.Instance.StartLevel()`.
  * `PlayCustomSpreadsheet()`: Invoca o `SpreadsheetLevelLoader` para ler o CSV, monta o `LevelConfig` customizado e inicia a missão personalizada.
  * `StartBriefing()`: Transiciona para a cena `01_BossBriefing`.
  * `OpenSpreadsheetFile()`: Abre o arquivo `CustomLevel.csv` no programa padrão do sistema operacional usando `Application.OpenURL("file://" + path)`.
  * `QuitGame()`: Encerra a aplicação ou interrompe o Play Mode no Editor.
  * `SetStatus(string message, Color color)`: Atualiza o texto de feedback de status na parte inferior da tela com a cor informada.
  * `EnsureMenuUI()`: Fallback robusto que monta o Canvas e botões na tela caso o componente não encontre botões serializados no Inspector.
  * `CreateMenuButton(...)`: Construtor estático de botões estilizados para a interface do menu.
  * `CreatePanel(...)`: Construtor estático de painéis retangulares com `Image` e `RectTransform`.
  * `CreateText(...)`: Construtor estático de campos de texto `TextMeshProUGUI`.
* **Possível pergunta do professor:**
  * *P: "Como o jogador cria e testa a sua própria fase via planilha?"*
  * *R:* "No menu principal, ele clica no botão 'OPEN SPREADSHEET'. O jogo abre o arquivo `CustomLevel.csv` diretamente no Excel ou Bloco de Notas. Ele altera a sequência de animais ou os tempos, salva o arquivo, volta para o jogo e clica em 'PLAY CUSTOM'. O jogo lê as alterações na hora e inicializa a missão personalizada."

---

### 7.2 `BriefingController`
* **Onde é Usado:** Fica acoplado no GameObject `BriefingController` na cena `01_BossBriefing`.
* **Explicação da Classe:** Orquestra a apresentação da missão antes do envio da nave para a Terra (item 14 do pipeline do professor). Exibe a introdução do chefe via `DialogueBox`, reposiciona o avatar do chefe, aciona o painel 3D de memorização dos alvos (`TargetShowcaseUI`) e transiciona para a Caça quando o tempo de memorização expira ou o jogador confirma.
* **Funções:**
  * `Start()`: Inicializa o botão de planilha e inicia o fluxo de apresentação da fase.
  * `Configure(...)`: Injeção de dependências para vincular a caixa de diálogo, vitrine 3D, títulos e avatar do chefe.
  * `InitializeBriefing()`: Gera a missão no `MissionManager` se ainda não tiver alvos, atualiza o título da fase e faz o comandante Zorp falar a introdução.
  * `GetBossOpeningLine(int level)`: Retorna o texto introdutório da missão baseado no nível ou estado de planilha.
  * `OnBossFinishedSpeaking()`: Inicia a transição do chefe para o canto da tela.
  * `TransitionBossToCornerRoutine()`: Move e reduz a escala do avatar do chefe para abrir espaço aos modelos 3D dos alvos e fala o aviso de alvo proibido.
  * `EnsureSpreadsheetButton()`: Configura ou instancia o botão de atalho para recarregar planilhas durante a fase de briefing.
  * `OnLoadSpreadsheetLevelClicked()`: Recarrega a planilha customizada durante o briefing e atualiza a vitrine de alvos em tempo real.
  * `OnStage2Finished()`: Inicia a contagem regressiva da vitrine de memorização dos alvos.
  * `ProceedToHunt()`: Oculta a vitrine e solicita ao `GameManager` a transição para a cena de caça (`03_CityHunt` ou `02_ShipFlight`).
* **Possível pergunta do professor:**
  * *P: "Por que o jogador tem um tempo limitado para ver os animais na tela de Briefing?"*
  * *R:* "Porque o conceito do jogo é a 'Abdução Metódica': o jogador precisa memorizar a sequência exata de animais e o alvo proibido antes de descer na cidade. Terminada a contagem, os animais somem da tela e o jogador deve caçá-los de memória."

---

### 7.3 `TargetShowcaseUI`
* **Onde é Usado:** Fica na cena `01_BossBriefing` dentro do Canvas de Briefing.
* **Explicação da Classe:** Responsável por exibir a fila física/3D dos animais que o jogador precisa memorizar. Instancia as miniaturas em linha com espaçamento dinâmico, desativa suas colisões, orienta os modelos para a câmera e exibe a barra e o texto da contagem regressiva de memorização.
* **Funções:**
  * `Awake()`: Limpa objetos anteriores e vincula o clique do botão de confirmação.
  * `Configure(...)`: Permite injetar programaticamente os componentes de barra de progresso, botão de confirmação e âncora.
  * `Update()`: Permite ao jogador pular o tempo de memorização pressionando as teclas de ação (Espaço ou clique).
  * `StartCountdown(float duration, Action onCompleted)`: Exibe o painel de tempo e inicia a corrotina de contagem regressiva.
  * `DisplayShowcase(IReadOnlyList<TargetData> order)`: Ativa o painel e instancia a fila de miniaturas dos alvos da missão.
  * `HideShowcase()`: Para corrotinas e desativa o painel.
  * `ConfirmAndProceed()`: Finaliza a memorização imediatamente e dispara o callback de transição para a caça.
  * `StopCountdownAndClearUI()`: Interrompe a corrotina de tempo e destrói os modelos 3D instanciados.
  * `ClearShowcase()`: Remove da cena todas as instâncias de miniaturas criadas para exibição.
  * `SpawnShowcaseTargets(IReadOnlyList<TargetData> order)`: Instancia os prefabs dos alvos, alinha em fila com rotação voltada para a câmera, desativa colisores e limpa scripts de física para manter os modelos estáticos como vitrine.
  * `CalculateLayout(int count)`: Calcula matematicamente o espaçamento horizontal e a escala com base na quantidade de alvos da missão.
  * `MemoryCountdownRoutine(float duration)`: Corrotina que atualiza o preenchimento da barra (`fillAmount`) e o texto de contagem a cada frame até zerar.
* **Possível pergunta do professor:**
  * *P: "Como você garantiu que os animais 3D instanciados na vitrine de memorização não caíssem com gravidade ou ficassem interagíveis?"*
  * *R:* "No método `SpawnShowcaseTargets()`, eu desativo todos os colliders (`col.enabled = false`), defino o Rigidbody como cinemático e sem gravidade (`isKinematic = true; useGravity = false`) e destruo quaisquer scripts de física (`SortableItem`, `Grabbable`) que venham no prefab. Eles atuam puramente como manequins visuais de visualização."

---

### 7.4 `DialogueBox`
* **Onde é Usado:** Fica na cena `01_BossBriefing` acoplado ao painel de diálogo do chefe.
* **Explicação da Classe:** Controla a exibição das falas do chefe no Briefing, reproduz o som da voz alienígena e avança para a próxima etapa quando o jogador clica ou pressiona a tecla de interação.
* **Funções:**
  * `Awake()`: Vincula o botão de avançar e localiza fontes de áudio e campos de texto `TextMeshProUGUI`.
  * `Start()`: Obtém o provedor de input ativo.
  * `Update()`: Detecta o pressionamento de teclas de interação ou clique para avançar a fala.
  * `Say(string speakerName, string text, Action onComplete)`: Preenche o nome do interlocutor, exibe o texto, executa o som de voz e ativa o indicador de continuação.
  * `HandleAdvanceRequested()`: Invoca o callback de conclusão e permite a continuidade da cena.
* **Possível pergunta do professor:**
  * *P: "Como o jogador avança os diálogos do chefe no briefing?"*
  * *R:* "Pressionando a tecla de interação (E ou Espaço), o botão esquerdo do mouse ou clicando diretamente no botão de avançar da caixa de diálogo. O `DialogueBox` intercepta esses inputs no `Update` e chama `HandleAdvanceRequested()`."

---

### 7.5 `BossPopupUI`
* **Onde é Usado:** Prefab `BossPopupUI` instanciado nas cenas de Caça (`03_CityHunt`) e Triagem (`04_ShipSorting`).
* **Explicação da Classe:** Exibe um balão de fala dinâmico na tela durante o gameplay sempre que o comandante Zorp reage a um evento (item 15 do pipeline do professor). Controla a opacidade via `CanvasGroup`, toca o áudio de voz alienígena e faz o balão desaparecer suavemente após alguns segundos.
* **Funções:**
  * `OnEnable()`: Assina o evento estático `BossDialogueManager.OnBossSpokeStatic`.
  * `OnDisable()`: Desassina o evento estático `BossDialogueManager.OnBossSpokeStatic`.
  * `Awake()`: Inicializa o `CanvasGroup`, a fonte de áudio e busca referências de texto.
  * `Show(string line, BossExpression expression)`: Dispara o efeito sonoro de voz e inicia a corrotina de exibição do balão na tela.
  * `ShowRoutine(string line)`: Torna o balão visível (`alpha = 1f`), aguarda a duração configurada (`_displayDuration`) e torna o balão invisível (`alpha = 0f`).
* **Possível pergunta do professor:**
  * *P: "Como o balão do chefe sabe o momento exato de aparecer na tela durante a caça na cidade?"*
  * *R:* "Através do evento estático `BossDialogueManager.OnBossSpokeStatic`. Quando o jogador erra ou acerta uma captura, o `MissionManager` dispara um evento no `GameEvents`, o `BossDialogueManager` sorteia a fala e dispara `OnBossSpokeStatic`. O `BossPopupUI` escuta esse evento e se torna visível na tela sem precisar de acoplamento direto com o player."

---

### 7.6 `SortingTimerUI`
* **Onde é Usado:** Anexado ao painel `SortingHUD` na cena `04_ShipSorting`.
* **Explicação da Classe:** Exibe na interface superior da nave o tempo restante da fase de Triagem (formatado em `MM:SS`) e a contagem de animais que ainda precisam ser organizados (`Targets: X/Y`).
* **Funções:**
  * `Configure(...)`: Permite vincular as referências de texto do cronômetro e da contagem de alvos.
  * `Update()`: Consulta o `TimerManager` para atualizar o texto do cronômetro regressivo e o `SortingManager` para atualizar a contagem de animais pendentes na nave.
* **Possível pergunta do professor:**
  * *P: "Onde o jogador visualiza quantos animais faltam guardar nas gaiolas na fase de triagem?"*
  * *R:* "No topo da tela através do `SortingTimerUI`. Ele consulta `SortingManager.Instance.RemainingItems` e `TotalItems` a cada frame, exibindo a contagem exata no formato `Targets: Restantes/Total`."

---

## 8. Perguntas Transversais de Arquitetura para a Sabatina

### 8.1 "Como o seu código se comunica com o código da Pessoa A (Samuel) sem gerar conflitos no Git?"
> **Resposta:**  
> "Através de interfaces (`IPlayerInput`), ScriptableObjects de configuração e do barramento de eventos **`GameEvents`**.  
> Por exemplo, quando o Samuel implementou a arma de encolhimento e o raio da nave (`ShrinkGun`/`ShipBeamZone`), o código dele não precisou instanciar regras nem acessar o inventário: ele apenas chama `MissionManager.Instance.ValidateCapture(targetData)`.  
> Da mesma forma, quando o jogador se move na triagem, o `ETPlayerSortingController` lê o input através de `IPlayerInput`, que é a interface implementada pelo `InputManager` do Samuel. Nunca editamos o script do outro diretamente."

### 8.2 "Por que vocês não usaram tags ou strings mágicas para checar se o animal é o correto na gaiola?"
> **Resposta:**  
> "Porque comparar strings (`other.tag == "Cow"`) é frágil, suscetível a erros de digitação e lento.  
> No nosso projeto, cada animal possui o componente `SortableItem`, que contém uma referência direta ao ScriptableObject fortemente tipado `TargetData` e ao enum `TargetType`. A gaiola simplesmente compara enums (`item.Type == _acceptedType`), o que é extremamente performático (comparação de inteiros de 32 bits) e seguro contra erros."

### 8.3 "Como vocês garantiram que o projeto ficasse leve (< 10 MB) para atender ao requisito da disciplina?"
> **Resposta:**  
> "1. Eliminamos pacotes externos pesados (sem Cinemachine, sem ProBuilder, sem assets pesados da Asset Store);  
> 2. Modelagem low-poly eficiente compartilhando materiais e paletas de cores simples;  
> 3. Eliminamos todos os scripts de geração procedural temporários que geravam malhas pesadas em tempo de execução;  
> 4. Utilizamos apenas compressão nativa LZ4HC e configurações otimizadas de TextMesh Pro."

---

<div align="center">
  <b>Manual Técnico e Guia de Sabatina Oral — Lucas (Pessoa B)</b><br>
  <i>Projeto V.A.R.G — "Abdução Metódica" (Unity 6.3 LTS)</i>
</div>
