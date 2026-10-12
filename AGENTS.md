# AGENTS.md — Regras para o Antigravity (Projeto V.A.R.G / "Abdução Metódica")

Estas regras valem para **qualquer agente** trabalhando neste repositório (os dois membros da dupla usam o Antigravity).

## Fontes da verdade
1. [GDD_Tecnico.md](./GDD_Tecnico.md) — **§R** (regras de jogo) e **§C** (contratos de código). Leia antes de implementar.
2. [Tasks_Divisao_Dupla.md](./Tasks_Divisao_Dupla.md) — tarefas com ID, dono, dependências e critério "Pronto quando".
3. [pipeline_professor.md](./pipeline_professor.md) — pipeline exigido pelo professor.

Se um pedido do usuário contradisser o GDD, **avise antes de implementar** e sugira atualizar o GDD primeiro.

## Projeto
- **Unity 6.3 LTS**, C#. Plataforma principal: PC, teclado + mouse. Celular só na Etapa 2 (Fase Extra).
- Limite de build: **< 10 MB** → não adicionar pacotes, plugins ou assets pesados sem pedir.
- Proibido adicionar: Cinemachine, ProBuilder, Visual Scripting, assets da Asset Store, qualquer pacote novo (mudança em `Packages/manifest.json` só com a dupla de acordo).

## API da Unity 6.3 (obrigatório)
- `Rigidbody.linearVelocity` / `linearDamping` / `angularDamping` — **nunca** `velocity`, `drag`, `angularDrag`.
- `Object.FindFirstObjectByType<T>()` / `FindObjectsByType<T>(FindObjectsSortMode.None)` — nunca `FindObjectOfType`.
- **Input System** (`UnityEngine.InputSystem`) — nunca `UnityEngine.Input` (`Input.GetKey`, `Input.GetAxis`, `Input.gyro`…). Gameplay lê input só via `IPlayerInput` / `IShipInput`.
- NavMesh: `Unity.AI.Navigation.NavMeshSurface` (pacote AI Navigation).
- UI: uGUI + `TMPro.TextMeshProUGUI` — nunca `UnityEngine.UI.Text`.
- Eventos estáticos: limpar em `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`; assinar em `OnEnable`, desassinar em `OnDisable`.

## Convenções de código e boas práticas
- **Código 100% em inglês:** todos os identificadores, classes, métodos, propriedades, enums e **variáveis** devem estar estritamente em inglês. Apenas textos exibidos na interface (UI) e diálogos de humor ficam em português.
- **Comentários mínimos e código autoexplicativo:** siga os princípios de *Clean Code*. O código deve falar por si mesmo através de nomes descritivos. Evite comentários óbvios, redundantes ou excessivos. Comente apenas o "porquê" de lógicas não triviais ou decisões de arquitetura complexas.
- **Boas práticas de programação (SOLID, DRY, KISS):**
  - **Responsabilidade Única (SRP):** cada classe/método faz apenas uma coisa bem feita. Métodos curtos, focados e coesos.
  - **Encapsulamento:** campos privados `_camelCase` com `[SerializeField]`, sem campos públicos mutáveis (exponha apenas propriedades com getters).
  - **Desacoplamento e Inversão de Dependência:** use interfaces (`IPlayerInput`, `IShipInput`, `IInteractable`) e eventos (`GameEvents`). Nunca use `GameObject.Find` ou `SendMessage`.
  - **Zero Hardcoding (KISS/DRY):** balanceamento (tempos, penalidades, velocidades, vidas) **sempre** em ScriptableObjects (`LevelConfig`, `PenaltyConfig`, `ScoringConfig`). Constantes nomeadas para valores fixos.
- Scripts em `Assets/_Project/Scripts/<Pasta>/`, namespace `VARG.<Pasta>` (ver GDD §0.4).
- Um `MonoBehaviour` por arquivo, nome do arquivo = nome da classe.
- **Performance e memória (Garbage Collector):** `Update` sem alocação (sem `new`, lambdas ou LINQ por frame). Não usar `Destroy` em alvos da Caça (usar desativação/pool).
- Regras de captura ficam **só** em `MissionManager.ValidateCapture()`; regras de triagem **só** em `Cage`/`DiscardBin`.

## Contratos e donos
- **Não alterar** nada da GDD §C (enums, SOs, interfaces, assinaturas públicas, eventos, layers) sem o usuário confirmar que combinou com o colega. Se precisar, proponha a mudança no GDD primeiro.
- Respeitar a tabela **"Donos"** do Tasks: pergunte ao usuário qual pessoa ele é (A ou B) se não estiver claro, e **não edite cenas, prefabs ou pastas do outro**.
- **Não editar** `ProjectSettings/` nem `Packages/` sem pedido explícito.

## Cenas, prefabs e Editor
- **Não escrever/editar à mão** arquivos `.unity`, `.prefab`, `.asset` ou `.meta` (YAML da Unity). Gere os scripts C# e entregue **instruções passo a passo para o Editor** (criar objeto, adicionar componente, configurar campos, layer).
- Quando útil, crie scripts de Editor (`Scripts/Editor/`, dentro de `#if UNITY_EDITOR` ou pasta `Editor`) para automatizar a criação de assets.

## Fluxo por tarefa
1. Identificar o ID da tarefa (ex.: `B05`), ler as seções do GDD citadas e as dependências.
2. Implementar só o escopo da tarefa.
3. Explicar como testar, conforme o "Pronto quando".
4. Marcar a tarefa como `[x]` no Tasks quando o usuário confirmar que funcionou.
5. Sugerir a branch `feature/<ID>-<descricao>` e uma mensagem de commit.
