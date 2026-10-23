# Guia Técnico e de Apresentação — Pessoa B (Lucas)
**Projeto:** V.A.R.G. (*Abduction Méthodique*) — Unity 6.3 LTS (C#)

Este documento explica **todos os 43 scripts da Pessoa B** exatamente como estão hoje no projeto. Para cada script você tem:
1. **Onde usamos no jogo:** em qual cena ou objeto da Unity ele fica.
2. **Para que serve:** objetivo direto e simples.
3. **Como funciona:** explicação passo a passo das funções e variáveis reais do código.
4. **Por que escolhemos desse jeito:** argumento natural para responder ao professor na apresentação.

---

## Visão Geral da Sua Parte (Pessoa B)

Você é responsável pela **arquitetura central (Core, Missão, Inventário, Timers, Vidas)** e pelas cenas de **Menu (`00_Boot`), Briefing (`01_BossBriefing`) e Triagem Física no Porão (`04_ShipSorting`)**, além do sistema de **Humor do Chefe** e do **leitor de níveis em planilha CSV (`CustomLevel.csv`)**.

```
[00_Boot] ──► [01_BossBriefing] ──► [02_ShipFlight] ──► [03_CityHunt] ──► [04_ShipSorting] ──► [05_Results]
 (Você)            (Você)              (Samuel)           (Samuel)            (Você)            (Samuel)
```

---

## Módulo 1 — Core (`Assets/_Project/Scripts/Core/`)
*Os 5 scripts base que garantem que os sistemas principais existam em qualquer cena e conversem sem acoplamento.*

### 1. `Singleton.cs` (`Singleton<T>`)
* **Onde usamos no jogo:** Classe base herdada por `GameManager`, `MissionManager`, `InventoryManager`, `TimerManager`, `LivesManager`, `BossDialogueManager`, `SceneLoader` e `SortingManager`.
* **Para que serve:** Evita repetir o código de `Instance` e `DontDestroyOnLoad` em todos os gerenciadores do jogo.
* **Como funciona:**
  * Guarda a instância única em `private static T _instance` e expõe `Instance` e `HasInstance`.
  * Tem a propriedade `protected virtual bool IsPersistent => true;`. Se for `true`, chama `DontDestroyOnLoad(gameObject)` no `Awake()` para o gerenciador sobreviver à troca de cenas. (No `SortingManager`, mudamos `IsPersistent` para `false` porque ele só vive na cena do porão).
  * Se já existir outra instância duplicada ao carregar uma cena, destrói a cópia (`Destroy(gameObject)`).
* **Por que escolhemos desse jeito:** Em vez de copiar e colar 25 linhas de Singleton em 8 classes diferentes (violando o princípio DRY), criamos uma classe genérica `Singleton<T>` de 35 linhas.

---

### 2. `PersistentBootstrapper.cs`
* **Onde usamos no jogo:** Classe estática (não vai em nenhum objeto). Roda sozinha antes de qualquer cena abrir.
* **Para que serve:** Garante que o prefab `[Systems]` (que contém `GameManager`, `MissionManager`, `InventoryManager`, `TimerManager`, `LivesManager`, `BossDialogueManager`, `SceneLoader`) seja criado automaticamente mesmo se dermos **Play** direto na cena `03_CityHunt` ou `04_ShipSorting`.
* **Como funciona:**
  * Usa `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` no método `EnsureSystemsExist()`.
  * Se `GameManager.Instance` ainda for nulo, carrega `Resources.Load<GameObject>("Systems/[Systems]")`, instancia e marca com `DontDestroyOnLoad`.
* **Por que escolhemos desse jeito:** Durante o desenvolvimento em dupla, eu precisava testar a cena `04_ShipSorting` e o Samuel precisava testar a `03_CityHunt` sem ter que abrir a `00_Boot` toda vez. Esse script de 20 linhas resolve isso automaticamente.

---

### 3. `GameManager.cs`
* **Onde usamos no jogo:** Dentro do prefab persistente `[Systems]`.
* **Para que serve:** É o "maestro" do jogo. Controla o nível atual (`CurrentLevel`), guarda as configurações (`CampaignConfig`, `GameConfig`, `ScoringConfig`, `PenaltyConfig`), calcula o resultado final da partida e comanda a transição entre as cenas.
* **Como funciona:**
  * `GetCurrentLevelConfig()`: Se o jogador carregou uma planilha CSV (`_customSpreadsheetLevelConfig != null`), usa ela; senão, pega a configuração do nível atual no array `_campaignConfig.Levels`.
  * `StartLevel()`: Limpa o inventário (`InventoryManager.Clear`), reseta as 5 vidas (`LivesManager.ResetLives`), zera os timers (`TimerManager.ResetTimers`), gera a missão no `MissionManager` e carrega a cena `01_BossBriefing`.
  * `GoToHunt()`: Vai para `02_ShipFlight` (se `UseShipFlight` estiver ativo no `GameConfig`) ou direto para `03_CityHunt`.
  * `GoToSorting()`: Para o cronômetro da caça e carrega `04_ShipSorting`.
  * `Victory()` e `Defeat(DefeatReason reason)`: Param os cronômetros, calculam a pontuação final (`CalculateAndStoreResult`), disparam `GameEvents.TriggerVictory()` ou `TriggerDefeat()` e carregam `05_Results`.
  * `NextLevel()`, `RetryLevel()`, `GoToMainMenu()`: Avançam de nível, repetem o nível ou voltam para `00_Boot`.
* **Por que escolhemos desse jeito:** Centralizar o fluxo de fases e o cálculo da nota final no `GameManager` evita que cada cena tente adivinhar qual é a próxima cena ou quantas vidas restam.

---

### 4. `SceneLoader.cs`
* **Onde usamos no jogo:** Dentro do prefab `[Systems]`.
* **Para que serve:** Encapsula a chamada `SceneManager.LoadScene(sceneName)`.
* **Como funciona:** Herda de `Singleton<SceneLoader>` e expõe o método `Load(string sceneName)`, que verifica se o nome não é vazio e carrega a cena.
* **Por que escolhemos desse jeito:** Deixa um único ponto de troca de cenas caso precisemos adicionar telas de carregamento ou transições no futuro.

---

### 5. `GameEvents.cs`
* **Onde usamos no jogo:** Classe estática pura usada por todos os scripts do projeto.
* **Para que serve:** É o barramento de eventos (*Event Bus*) do jogo. Permite que a Caça, a Triagem, a UI e o Chefe conversem sem um depender do código do outro.
* **Como funciona:**
  * Declara eventos estáticos (`event Action<...>`) para todas as situações do jogo:
    * Capturas na cidade: `OnCorrectCapture`, `OnOutOfOrderCapture`, `OnNotInListCapture`, `OnMissionComplete`.
    * Triagem no porão: `OnCorrectCage`, `OnWrongCage`, `OnCorrectDiscard`, `OnWrongDiscard`.
    * Estado da partida: `OnLifeLost`, `OnTimeLow`, `OnBeamDenied`, `OnVictory`, `OnDefeat`.
  * Possui métodos `Trigger...()` para disparar cada evento com segurança (`?.Invoke(...)`).
  * `ResetAll()`: Limpa todos os eventos (`= null`) no `SubsystemRegistration` ao dar Play no Editor.
* **Por que escolhemos desse jeito:** Foi a peça-chave para dividir o trabalho entre a Pessoa A e a Pessoa B sem conflito de Git. Quando o jogador acerta ou erra um animal, quem detectou o acerto só chama `GameEvents.Trigger...`, e o `BossDialogueManager`, os timers e a UI reagem automaticamente.

---

## Módulo 2 — Missão, Inventário, Timers e Vidas (`Mission/` e `Inventory/`)

### 6. `MissionManager.cs` (`Assets/_Project/Scripts/Mission/`)
* **Onde usamos no jogo:** No prefab persistente `[Systems]`.
* **Para que serve:** Gera a lista ordenada de animais que o jogador precisa capturar, sorteia qual espécie é **proibida (`ForbiddenType`)** naquele nível e valida cada captura feita na cidade.
* **Como funciona:**
  * `GenerateMission(LevelConfig level)`: Sorteia 1 dos 4 tipos (`Chicken`, `Cow`, `Pig`, `Human`) para ser o `ForbiddenType`. Depois monta uma lista (`_requiredOrder`) do tamanho pedido pelo nível (`level.ListSize`) usando apenas os outros 3 tipos permitidos e embaralha a ordem.
  * `LoadCustomMission(...)`: Carrega uma sequência personalizada vinda do arquivo `CustomLevel.csv`.
  * `GetRequiredCountForType(TargetType type)`: Conta quantos animais daquela espécie foram pedidos na lista (usado pelas gaiolas na fase de Triagem para saber a cota máxima de cada gaiola!).
  * `ValidateCapture(TargetData captured)`: **Único lugar que julga a captura na cidade:**
    1. **Se capturou a espécie proibida (`captured.Type == ForbiddenType`):** adiciona no inventário como `NotInList`, aplica **dívida de tempo** para a fase de triagem (`ApplyPenalty(TimerPhase.Sorting, notInListDebt)`), tira **1 vida** (`LivesManager.LoseLife()`) e dispara `TriggerNotInListCapture`.
    2. **Se capturou o animal certo na ordem certa (`captured == expected`):** avança `CurrentIndex++`, adiciona no inventário como `Correct` e dispara `TriggerCorrectCapture` (e `TriggerMissionComplete` se terminou a lista).
    3. **Se capturou fora de ordem (`OutOfOrder`):** adiciona no inventário como `OutOfOrder`, desconta **10 segundos** imediatamente do cronômetro de Caça (`ApplyPenalty(TimerPhase.Capture, outOfOrderPenalty)`), tira **1 vida** e dispara `TriggerOutOfOrderCapture`.
* **Por que escolhemos desse jeito:** Concentrar toda a regra em `ValidateCapture` garante que o script `Target.cs` da Pessoa A só precise chamar uma linha (`MissionManager.Instance.ValidateCapture(_data)`).

---

### 7. `SpreadsheetLevelLoader.cs` (`Assets/_Project/Scripts/Mission/`)
* **Onde usamos no jogo:** Classe estática chamada pelo Menu Principal (`MainMenuController`) e pelo Briefing (`BriefingController`).
* **Para que serve:** Cria e lê o arquivo de planilha externa `CustomLevel.csv` na raiz do projeto, permitindo criar fases personalizadas no Excel/Calc sem recompilar o jogo.
* **Como funciona:**
  * `EnsureTemplateExists(out string filePath)`: Se `CustomLevel.csv` ainda não existir, cria um arquivo modelo pronto com as 7 configurações (`TargetSequence`, `ForbiddenTarget`, `CaptureTime`, `SortingTime`, `ExtraSpawns`, `ForbiddenSpawns`, `FleeSpeedMultiplier`).
  * `TryLoadCustomLevel(out SpreadsheetLevelData levelData, out string statusMessage)`: Lê todas as linhas do CSV, aceita tanto vírgula (`,`) quanto ponto-e-vírgula (`;`), converte os nomes dos animais em inglês ou francês (`Cow/Vache`, `Pig/Cochon`, `Chicken/Poule`, `Human/Humain`) e valida se o `ForbiddenTarget` não está misturado dentro da `TargetSequence`.
* **Por que escolhemos desse jeito:** Combina com o tema de "burocracia corporativa alienígena" do jogo e permite que o próprio professor edite o CSV na hora da apresentação para testar uma missão customizada.

---

### 8. `TimerManager.cs` (`Assets/_Project/Scripts/Mission/`)
* **Onde usamos no jogo:** No prefab persistente `[Systems]`.
* **Para que serve:** Controla os **dois cronômetros** do jogo (`TimerPhase.Capture` na cidade e `TimerPhase.Sorting` no porão) e guarda a **dívida de tempo (`SortingDebt`)**.
* **Como funciona:**
  * `StartTimer(TimerPhase phase, float duration)`: Quando inicia o cronômetro de `Sorting`, ele já desconta automaticamente a dívida acumulada na cidade (`_sortingRemaining = duration - SortingDebt`) e zera a dívida.
  * `Update()` -> `TickPhase(...)`: Diminui `Time.deltaTime`. Quando faltam 15 segundos (`TimeLowThreshold`), dispara `GameEvents.TriggerTimeLow`. Se chegar a `0`, dá derrota imediata (`GameManager.Defeat`).
  * `ApplyPenalty(TimerPhase phase, float seconds)`: Se o cronômetro daquela fase já estiver rodando, subtrai os segundos na hora. Se for uma penalidade de `Sorting` aplicada enquanto o jogador ainda está na cidade (ex.: capturou um Humano proibido), soma em `SortingDebt` para descontar quando chegar ao porão!
* **Por que escolhemos desse jeito:** O sistema de dívida (`SortingDebt`) faz com que erros graves na cidade tenham consequência direta no tempo que você terá para fazer a triagem dentro da nave.

---

### 9. `LivesManager.cs` (`Assets/_Project/Scripts/Mission/`)
* **Onde usamos no jogo:** No prefab persistente `[Systems]`.
* **Para que serve:** Controla as **5 vidas** do jogador em cada nível.
* **Como funciona:**
  * `ResetLives(int amount)`: Reinicia `Lives` (padrão: 5) no começo do nível e avisa a UI (`OnLivesChanged`).
  * `LoseLife()`: Subtrai `1` vida, dispara `GameEvents.TriggerLifeLost(Lives)` e, se chegar a `0`, chama `GameManager.Instance.Defeat(DefeatReason.NoLives)`.
* **Por que escolhemos desse jeito:** Isolar as vidas num gerenciador simples de 38 linhas evita espalhar contadores de vida entre a Caça e a Triagem.

---

### 10. `InventoryManager.cs` (`Assets/_Project/Scripts/Inventory/`)
* **Onde usamos no jogo:** No prefab persistente `[Systems]`.
* **Para que serve:** Guarda na memória tudo o que o jogador abduziu na cidade (`List<CapturedItem> _items`), sobrevivendo à troca de cena da cidade (`03_CityHunt`) para o porão (`04_ShipSorting`).
* **Como funciona:**
  * `Add(TargetData data, CaptureResult result)`: Cria um `CapturedItem(data, result, Time.time)`, adiciona na lista `_items` e dispara o evento `OnItemAdded`.
  * `Clear()`: Esvazia a lista quando começa um nível novo.
* **Por que escolhemos desse jeito:** Como os objetos 3D da cidade são destruídos ao mudar de cena, salvamos apenas a referência leve (`TargetData` + `CaptureResult`) no `InventoryManager` para recriar as miniaturas no porão.

---

## Módulo 3 — Dados e ScriptableObjects (`Assets/_Project/Scripts/Data/`)
*Os 13 arquivos que definem os tipos de dados, structs e configurações editáveis no Inspector.*

### 11 a 14. Enums (`TargetType.cs`, `CaptureResult.cs`, `DefeatReason.cs`, `TimerPhase.cs`)
* **`TargetType.cs`:** As 4 espécies do jogo (`Chicken`, `Cow`, `Pig`, `Human`).
* **`CaptureResult.cs`:** O resultado da abdução (`Correct`, `OutOfOrder`, `NotInList`).
* **`DefeatReason.cs`:** O motivo da derrota (`CaptureTimeUp`, `SortingTimeUp`, `NoLives`).
* **`TimerPhase.cs`:** Qual cronômetro está ativo (`Capture`, `Sorting`).

### 15 a 17. Structs e Dados em Memória (`CapturedItem.cs`, `LevelResult.cs`, `SpreadsheetLevelData.cs`)
* **`CapturedItem.cs`:** Struct imutável que guarda `TargetData Data`, `CaptureResult Result` e `float CaptureTimestamp` de cada animal abduzido.
* **`LevelResult.cs`:** Struct com o resumo da partida para a tela de resultados (`RemainingCaptureTime`, `RemainingSortingTime`, `RemainingLives`, `ErrorsCount`, `Score`).
* **`SpreadsheetLevelData.cs`:** Classe de dados preenchida pelo `SpreadsheetLevelLoader` ao ler o arquivo `CustomLevel.csv`.

### 18 a 23. ScriptableObjects (`TargetData.cs`, `LevelConfig.cs`, `CampaignConfig.cs`, `PenaltyConfig.cs`, `ScoringConfig.cs`, `GameConfig.cs`)
* **`TargetData.cs`:** Ficha de cada espécie (`Type`, `DisplayName`, `Icon`, `CityPrefab`, `MiniPrefab`, `WalkSpeed`, `FleeSpeed`, `ShrinkDuration`).
* **`LevelConfig.cs`:** Configuração de um nível (`ListSize`, `CaptureTime`, `SortingTime`, `ExtraSpawns`, `ForbiddenSpawns`, `FleeSpeedMultiplier`). Possui também o método `Setup(...)` para criar níveis em memória a partir do CSV.
* **`CampaignConfig.cs`:** Contém o array `LevelConfig[] Levels` com a progressão dos níveis da campanha.
* **`PenaltyConfig.cs`:** Define os valores das punições (`OutOfOrderCapturePenalty = 10s`, `NotInListSortingDebt = 10s`, `WrongCagePenalty = 5s`, `WrongDiscardPenalty = 5s`, `StartingLives = 5`).
* **`ScoringConfig.cs`:** Define quantos pontos vale cada segundo restante (`PointsPerSecond = 10`) e cada vida preservada (`PointsPerLife = 100`).
* **`GameConfig.cs`:** Define opções gerais como `UseShipFlight` (se passa pela fase do disco voador antes da cidade).
* **Por que escolhemos desse jeito:** Guardar números de balanceamento em `ScriptableObjects` na pasta `Resources/Data/` segue a regra de *Zero Hardcoding*: podemos ajustar tempos, velocidades e penalidades no Inspector sem alterar código.

---

## Módulo 4 — Triagem Física no Porão (`Assets/_Project/Scripts/Sorting/`)
*Os 12 scripts que controlam a cena `04_ShipSorting`, onde os animais abduzidos caem no compartimento central e você precisa colocá-los nas gaiolas certas ou no triturador.*

### 24. `SortingManager.cs`
* **Onde usamos no jogo:** Na cena `04_ShipSorting`.
* **Para que serve:** Conta quantos itens existem no porão (`TotalItems`), quantos ainda faltam ser triados (`RemainingItems`), inicia o cronômetro de Triagem e declara Vitória assim que o último item é resolvido.
* **Como funciona:**
  * `Awake()`: Chama `EnsureCaptureBoxSpawner()`, que manda o `CaptureBoxSpawner` instanciar no porão todos os itens trazidos da cidade.
  * `Start()`: Registra todos os `SortableItem` da cena e inicia o cronômetro `TimerPhase.Sorting` no `TimerManager`.
  * `RegisterItem(SortableItem)` e `NotifyItemResolved(SortableItem)`: Atualizam os conjuntos `_registeredItems` e `_resolvedItems` e avisam a UI (`OnItemCountsChanged`). Quando `RemainingItems <= 0`, inicia a corrotina `VictoryRoutine()`, que espera `0.6s`, para o relógio e chama `GameManager.Instance.Victory()`.
  * `GetUnsortedCountForType(TargetType type)`: Conta quantos animais daquela espécie ainda estão soltos no porão (usado pelo `DiscardBin` para saber se o jogador tem animais excedentes que podem ser jogados no lixo).
* **Por que escolhemos desse jeito:** O `SortingManager` não sabe se o item foi para a gaiola ou para o lixo — ele apenas recebe `NotifyItemResolved(item)` quando qualquer destino válido processa o item, mantendo o código desacoplado.

---

### 25. `CaptureBoxSpawner.cs`
* **Onde usamos no jogo:** Na cena `04_ShipSorting`, gerando os itens dentro do compartimento central (`_spawnAnchor`).
* **Para que serve:** Lê a lista de itens capturados no `InventoryManager` e instancia as miniaturas 3D (`MiniPrefab`) no centro do porão.
* **Como funciona:**
  * `SpawnExistingItems()`: Limpa objetos antigos e percorre `InventoryManager.Instance.Items`. Se a lista estiver vazia (porque você deu Play direto na cena `04_ShipSorting` para testar), chama `SeedFallbackItems()` para gerar uma missão de teste automaticamente.
  * `SpawnItem(CapturedItem item)`: Instancia o `MiniPrefab` em uma posição aleatória dentro da caixa central (`_spawnAreaSize`), aumenta a escala em `1.7x`, adiciona `SortableItem` e `Grabbable`, coloca na layer `"Grabbable"` e registra no `SortingManager`.
* **Por que escolhemos desse jeito:** Permite transformar os dados abstratos salvos no `InventoryManager` em objetos físicos reais com `Rigidbody`, além de permitir testar a cena `04_ShipSorting` isoladamente graças ao `SeedFallbackItems()`.

---

### 26. `SortableItem.cs`
* **Onde usamos no jogo:** Em cada animal/miniatura instanciado no porão.
* **Para que serve:** Guarda a identidade do item (`TargetData`, `CapturedItem`), controla se ele já foi triado (`IsSorted`) ou se está segurado na mão (`IsHeld`), e sabe como voltar para o centro do porão (`ReturnToCenter`) quando o jogador erra.
* **Como funciona:**
  * `Initialize(CapturedItem)`: Recebe os dados vindos do `CaptureBoxSpawner`.
  * `MarkSorted()`: Marca `_isSorted = true` assim que o item entra na gaiola certa ou no triturador certo, impedindo que seja processado duas vezes.
  * `ReturnToCenter()`: Zera a velocidade linear e angular do `Rigidbody` e teletransporta o objeto de volta para o centro do porão (`(0, 0.7, 1.2)` com leve variação aleatória).
* **Por que escolhemos desse jeito:** Centralizar o `ReturnToCenter()` dentro do `SortableItem` permitiu que tanto a Gaiola (`Cage`) quanto o Triturador (`DiscardBin`) devolvam animais jogados no lugar errado usando uma única linha de código (`item.ReturnToCenter()`), sem risco de o animal ser cuspido para fora do mapa.

---

### 27. `Grabbable.cs`
* **Onde usamos no jogo:** Em cada miniatura do porão (junto com `Rigidbody` e `SortableItem`).
* **Para que serve:** Prepara a física do objeto quando ele é segurado ou solto pelo jogador.
* **Como funciona:**
  * `OnGrab()`: Marca `IsHeld = true` no `SortableItem`, desliga a gravidade (`useGravity = false`), zera as velocidades e trava a rotação (`FreezeRotation`) para o animal não ficar girando descontroladamente enquanto você mira na gaiola.
  * `OnRelease(Vector3 releaseVelocity)`: Restaura a gravidade e as rotações originais do `Rigidbody` e aplica a velocidade de soltura/arremesso (`rb.linearVelocity = releaseVelocity`).
* **Por que escolhemos desse jeito:** Desligar a gravidade e travar a rotação apenas enquanto `IsHeld == true` faz o objeto ficar estável na frente da câmera sem precisar desligar as colisões físicas (`isKinematic = false`).

---

### 28. `GrabController.cs`
* **Onde usamos no jogo:** No jogador (`ETPlayer`) da cena `04_ShipSorting`.
* **Para que serve:** Permite pegar objetos à distância com o clique esquerdo, aproximar/afastar com o scroll do mouse e arremessar com o clique direito.
* **Como funciona:**
  * `Update()`: Lê os comandos através da interface `IPlayerInput` (`InputManager.Instance.Player`):
    * `FirePressed` (Clique Esquerdo): Se não está segurando nada, faz um `Physics.Raycast` do centro da câmera (`TryGrab`). Se já está segurando, solta suavemente (`ReleaseHeld(false)`).
    * `ThrowPressed` (Clique Direito): Arremessa o objeto na direção da câmera (`ReleaseHeld(true)` aplicando ` cameraForward * _throwForce`).
    * `ScrollDelta` (Rodinha do mouse): Ajusta `_currentHoldDistance` entre `_minHoldDistance` (`1.2m`) e `_maxHoldDistance` (`4.5m`).
  * `FixedUpdate()`: Enquanto segura um objeto, calcula a distância até o ponto-alvo na frente da câmera (`GetHoldPoint()`) e define `rb.linearVelocity = Vector3.ClampMagnitude(toTarget * _followSpeed, _maxFollowVelocity)`.
* **Por que escolhemos desse jeito:** Mover o objeto por `linearVelocity` no `FixedUpdate` em vez de teletransportar o `transform.position` impede que o jogador atravesse as grades das gaiolas ou paredes com o objeto segurado.

---

### 29. `TractorBeamVisual.cs`
* **Onde usamos no jogo:** No jogador da cena `04_ShipSorting`, controlando um `LineRenderer`.
* **Para que serve:** Desenha o feixe luminoso curvado (curva de Bézier quadrática com 12 pontos) ligando a mão do jogador ao animal segurado.
* **Como funciona:**
  * `LateUpdate()`: Verifica se `_grabController.IsHolding` é verdadeiro. Se for, ativa o `LineRenderer` e calcula 12 pontos formando um arco suave entre a origem (na frente da câmera), o ponto de controle (`GetHoldPoint`) e a posição atual do objeto (`held.transform.position`).
* **Por que escolhemos desse jeito:** Como o objeto físico tem uma leve inércia para alcançar o ponto da mira (`GetHoldPoint`), usar esses 3 pontos numa curva de Bézier faz o raio trator se curvar dinamicamente quando você move o mouse rápido, dando um visual muito legal com poucas linhas de matemática.

---

### 30. `Cage.cs`
* **Onde usamos no jogo:** Em cada uma das 4 gaiolas da cena `04_ShipSorting` (`Cage_Cow`, `Cage_Chicken`, `Cage_Pig`, `Cage_Human`).
* **Para que serve:** Valida os animais colocados dentro da gaiola, verificando se a espécie está certa, se não é a espécie proibida e se a cota daquela gaiola ainda não encheu.
* **Como funciona:**
  * `ProcessTriggerStay(Collider other)`: Ignora se o item ainda está sendo segurado na mão (`item.IsHeld`) ou se já foi processado (`item.IsSorted`). Quando o jogador solta o item dentro da gaiola, verifica 3 coisas:
    1. `item.Type == _acceptedType` (é da espécie desta gaiola?)
    2. `!isForbidden` (não é a espécie proibida da missão?)
    3. `_lockedItems.Count < quota` (ainda faltam animais dessa espécie na lista do chefe?)
  * **Se passou nas 3 condições (`AcceptItem`):** marca como triado (`MarkSorted`), desativa o `Grabbable`, posiciona dentro da gaiola (`SnapPoint`), toca o som de acerto, dispara `GameEvents.TriggerCorrectCage` e avisa `SortingManager.NotifyItemResolved(item)`.
  * **Se falhou em qualquer condição (`EjectItem`):** chama `item.ReturnToCenter()` para devolver o animal ao centro da sala, toca som de erro, aplica **-5 segundos** de penalidade no `TimerManager` e dispara `GameEvents.TriggerWrongCage`.
* **Por que escolhemos desse jeito:** Verificar `!item.IsHeld` garante que a gaiola só julgue o animal quando o jogador realmente **soltar** ele lá dentro, evitando ativações acidentais só de passar o objeto perto da grade.

---

### 31. `CageTriggerZone.cs`
* **Onde usamos no jogo:** No objeto filho com `Collider (IsTrigger)` dentro de cada gaiola.
* **Para que serve:** Encaminha o evento `OnTriggerEnter` / `OnTriggerStay` do colisor filho para o script `Cage` no objeto pai.
* **Como funciona:** Busca `GetComponentInParent<Cage>()` no `Awake()` e chama `_parentCage.ProcessTriggerStay(other)`.
* **Por que escolhemos desse jeito:** Permite separar os colisores físicos das grades da gaiola (no pai) da zona de detecção interna (no filho).

---

### 32. `DiscardBin.cs`
* **Onde usamos no jogo:** No Triturador / Lixeira (`DiscardBin`) da cena `04_ShipSorting`.
* **Para que serve:** Destrói os animais inválidos ou excedentes capturados por engano, mas pune o jogador se ele tentar triturar um animal que é necessário para completar a cota da missão.
* **Como funciona:**
  * `IsValidDiscard(SortableItem item)`: Calcula se aquele animal realmente deve ir para o lixo:
    1. Se for da espécie proibida (`ForbiddenType`), é lixo válido (`true`).
    2. Se a missão pediu `0` daquela espécie (`required == 0`), é lixo válido (`true`).
    3. Se a soma dos animais daquela espécie já guardados nas gaiolas + os que ainda estão soltos no porão for **maior** que a quantidade pedida pelo chefe (`(cagedCount + availableCount) > required`), significa que é um animal **excedente** (capturado fora de ordem a mais), então também pode ser descartado (`true`)!
  * **Se for descarte válido (`AcceptDiscard`):** marca como triado, joga para o fundo do triturador, destrói em `0.6s` (`Destroy(item.gameObject, 0.6f)`), dispara `GameEvents.TriggerCorrectDiscard` e avisa `SortingManager.NotifyItemResolved(item)`.
  * **Se tentar triturar um animal necessário (`EjectDiscard`):** salva o animal mandando de volta para o meio da sala (`item.ReturnToCenter()`), desconta **5 segundos** no `TimerManager`, tira **1 vida** no `LivesManager` e dispara `GameEvents.TriggerWrongDiscard`.
* **Por que escolhemos desse jeito:** A conta `(cagedCount + availableCount) > required` é muito inteligente porque não importa *qual* das duas vacas você jogou no lixo (a primeira ou a segunda): o que importa é se ainda restam vacas suficientes no porão para fechar a cota da gaiola. E se não restarem, o triturador devolve a vaca para o centro da sala para a fase nunca ficar impossível de vencer.

---

### 33. `DiscardTriggerZone.cs`
* **Onde usamos no jogo:** No objeto filho com `Collider (IsTrigger)` dentro do Triturador.
* **Para que serve:** Encaminha o `OnTriggerEnter` / `OnTriggerStay` do colisor filho para o `DiscardBin` no objeto pai.

---

### 34. `ETPlayerSortingController.cs`
* **Onde usamos no jogo:** No jogador (`ETPlayer`) da cena `04_ShipSorting`.
* **Para que serve:** Controla a movimentação em primeira pessoa (`WASD` via `CharacterController`) e rotação da câmera com o mouse dentro do porão da nave.
* **Como funciona:**
  * `Start()`: Trava o cursor no centro da tela (`CursorLockMode.Locked`).
  * `Update()`: Lê `Look` e `Move` da interface `IPlayerInput`. Gira o corpo no eixo Y, inclina a câmera no eixo X (limitado entre `-85°` e `+85°`), aplica gravidade (`-15f`) e move o `CharacterController`.

---

### 35. `KillZoneRespawn.cs`
* **Onde usamos no jogo:** Em um colisor invisível abaixo do chão da cena `04_ShipSorting`.
* **Para que serve:** Rede de segurança: se algum item físico ou o próprio jogador cair para fora da sala por algum empurrão físico, teletransporta de volta para dentro do porão (`_respawnPoint`).
* **Como funciona:** No `OnTriggerEnter(Collider other)`, zera a velocidade do `Rigidbody` (ou desliga/religa o `CharacterController`) e move o objeto para `_respawnPoint.position`.

---

## Módulo 5 — Humor e Chefe Alienígena (`Humor/` e `UI/`)

### 36. `BossDialogueManager.cs` (`Assets/_Project/Scripts/Humor/`)
* **Onde usamos no jogo:** No prefab persistente `[Systems]`.
* **Para que serve:** Escuta todos os eventos de acerto, erro, vitória e derrota da partida e faz o Chefe Alienígena falar uma frase sarcástica com efeito sonoro de voz extraterrestre.
* **Como funciona:**
  * `OnEnable()` / `OnDisable()`: Assina os 9 eventos principais do `GameEvents` (`OnCorrectCapture`, `OnOutOfOrderCapture`, `OnNotInListCapture`, `OnCorrectCage`, `OnWrongCage`, `OnCorrectDiscard`, `OnWrongDiscard`, `OnVictory`, `OnDefeat`).
  * `SpeakRandom(string[] array, TargetData captured)`: Sorteia uma frase do `BossLineSet` (`Random.Range`) e substitui `{captured}` pelo nome do animal em maiúsculas (`captured.DisplayName.ToUpper()`).
  * `Speak(string text)`: Respeita um intervalo mínimo de `1.8s` (`_minCooldown`) para não encavalar falas, toca o áudio da voz alienígena (`PlayVoice()`) e chama `BossPopupUI.Show(text)`.
* **Por que escolhemos desse jeito:** Como ele vive no `[Systems]` e escuta o `GameEvents`, o chefe reage tanto às ações da Cidade (Pessoa A) quanto às ações do Porão (Pessoa B) sem precisar ser colocado manualmente em cada cena.

---

### 37. `BossLineSet.cs` (`Assets/_Project/Scripts/Humor/`)
* **Onde usamos no jogo:** Arquivo `.asset` em `Resources/Data/BossLineSet`.
* **Para que serve:** `ScriptableObject` que armazena as listas de frases (`string[]`) do chefe para cada evento do jogo.

---

### 38. `BossPopupUI.cs` (`Assets/_Project/Scripts/UI/`)
* **Onde usamos no jogo:** Prefab carregado automaticamente de `Resources/UI/BossPopupUI` (com `DontDestroyOnLoad`).
* **Para que serve:** Mostra o balão de fala do chefe (`COMMANDER VARG`) na parte inferior da tela durante `3.2` segundos quando `BossPopupUI.Show(line)` é chamado.
* **Como funciona:**
  * `Show(string line)`: Se o balão ainda não existir na cena, carrega o prefab `Resources/UI/BossPopupUI` dentro de um `Canvas` persistente (`sortingOrder = 500`) e chama `DisplayLine(line)`.
  * `ShowRoutine(string line)`: Atualiza `_dialogueText.text = line`, deixa o `CanvasGroup` visível (`alpha = 1f`), aguarda `_displayDuration` (`3.2s`) e esconde novamente (`alpha = 0f`).

---

### 39. `DialogueBox.cs` (`Assets/_Project/Scripts/UI/`)
* **Onde usamos no jogo:** Na cena `01_BossBriefing`.
* **Para que serve:** Caixa de diálogo fixa do Briefing onde o Comandante Zorp passa as instruções iniciais da missão.
* **Como funciona:**
  * `Say(string speakerName, string text, Action onComplete)`: Preenche o nome do chefe, o texto da missão, toca o som de voz (`BossDialogueManager.Instance.PlayVoice()`) e ativa o indicador para continuar (`[ Press SPACE or Click to continue ]`).

---

## Módulo 6 — Menus e Interfaces das Suas Cenas (`Assets/_Project/Scripts/UI/`)

### 40. `MainMenuController.cs`
* **Onde usamos no jogo:** Na cena `00_Boot` (criado automaticamente toda vez que a cena `00_Boot` é carregada via `SceneManager.sceneLoaded`).
* **Para que serve:** Constrói e controla a tela de Menu Principal com 4 botões: **PLAY CAMPAIGN**, **PLAY CUSTOM (SPREADSHEET)**, **OPEN SPREADSHEET (EXCEL)** e **QUIT**.
* **Como funciona:**
  * `EnsureMainMenuInBootScene()`: Assina `SceneManager.sceneLoaded` para garantir que, sempre que o jogador entrar ou voltar para `00_Boot`, o `MainMenuController` seja criado.
  * `PlayCampaign()`: Limpa qualquer nível customizado e inicia a campanha normal no `GameManager`.
  * `PlayCustomSpreadsheet()`: Chama `SpreadsheetLevelLoader.TryLoadCustomLevel(...)`, cria o `LevelConfig` em memória com os dados do `CustomLevel.csv`, injeta no `GameManager` e no `MissionManager` e inicia o Briefing.
  * `OpenSpreadsheetFile()`: Abre o arquivo `CustomLevel.csv` direto no editor padrão do sistema operacional (Excel / LibreOffice Calc / Bloco de Notas).

---

### 41. `BriefingController.cs`
* **Onde usamos no jogo:** Na cena `01_BossBriefing`.
* **Para que serve:** Coordena a tela de Briefing: mostra o título do nível, faz o chefe falar a introdução, aciona a vitrine 3D (`TargetShowcaseUI`) e permite carregar o `CustomLevel.csv` clicando no botão `[SPREADSHEET LEVEL]`.
* **Como funciona:**
  * `Start()`: Atualiza o título da missão (`CAPTURE MISSION | LEVEL X`), exibe a fala do chefe na `DialogueBox`, cria o botão `[SPREADSHEET LEVEL]` no canto superior direito e inicia a sequência do Briefing.
  * `OnLoadSpreadsheetLevelClicked()`: Recarrega o arquivo `CustomLevel.csv` ao vivo e atualiza a vitrine 3D na hora.
  * `ProceedToHunt()`: Quando o tempo de memorização termina (ou o jogador avança), chama `GameManager.Instance.GoToHunt()`.

---

### 42. `TargetShowcaseUI.cs`
* **Onde usamos no jogo:** Na cena `01_BossBriefing`.
* **Para que serve:** Instancia os modelos 3D dos animais da missão enfileirados em pedestais iluminados para o jogador memorizar a sequência antes de ir para a cidade.
* **Como funciona:**
  * `ShowTargets(...)`: Lê a lista `MissionManager.Instance.RequiredOrder`, calcula o espaçamento horizontal para centralizar os modelos na câmera, instancia o `MiniPrefab` (ou `CityPrefab`) de cada animal em cima de um pedestal cilíndrico com número (`1, 2, 3...`) e inicia o cronômetro de memorização (`MEMORY TIME: 08s`).
  * `StripGameplayBehaviours(GameObject obj)`: Desativa `NavMeshAgent`, `Rigidbody` e `Collider` dos clones instanciados na vitrine para que eles funcionem apenas como estátuas giratórias no Briefing.
  * `Update()`: Gira os animais suavemente no eixo Y (`_spinSpeed = 35f`) e conta o tempo regressivo.

---

### 43. `SortingTimerUI.cs`
* **Onde usamos no jogo:** No HUD da cena `04_ShipSorting`.
* **Para que serve:** Exibe o cronômetro regressivo (`MM:SS`), a contagem de itens restantes no porão (`Targets: X/Y`) e o painel de vidas (`LIVES: X`).
* **Como funciona:**
  * `Start()`: Assina `TimerManager.OnTimeChanged`, `SortingManager.OnItemCountsChanged` e `LivesManager.OnLivesChanged` para atualizar os textos da tela sempre que o tempo, o número de animais ou as vidas mudarem. Quando faltam 10 segundos ou menos (`_lowTimeThreshold`), pinta o relógio de vermelho.
