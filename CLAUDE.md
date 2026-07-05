# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A C#/.NET digital port (with the author's permission) of the solo board game **Utopia Engine** by Nick Hayes. The goal is a cross-platform game built on a shared, UI-agnostic engine. Rules reference lives in `Documentation/` (PDF rulebook + adventure sheets, and a markdown rulebook under `Documentation/rulebook/`).

## Toolchain & build

Single modern stack, all on **`net10.0`** (SDK-style projects).

- **Engine + tests → `UtopiaEngine.Net.slnx`** (`UE.Core`, `UE.Nunit`).
  - Build: `dotnet build UtopiaEngine.Net.slnx`
  - Test: `dotnet test UtopiaEngine.Net.slnx` (uses **NUnit 4** + `NUnit3TestAdapter` + `Microsoft.NET.Test.Sdk`).
  - Single test: `dotnet test UtopiaEngine.Net.slnx --filter "FullyQualifiedName~ScoreTest"` (or `Name~WinningAddsBonus`).
- **UI → `UI/UE.UI.slnx`** (Avalonia, CommunityToolkit.Mvvm), heads for Desktop/Android/Browser.
  - Run desktop: `dotnet run --project UI/UE.UI.Desktop` (or `just run`)
  - Run browser (WASM): `dotnet run --project UI/UE.UI.Browser` (or `just browser`)
  - Build both: `just build`
  - See `justfile` for the full command list.

There is no legacy stack anymore: the original MvvmCross/.NET Framework heads (`UE.Console`, `UE.WPF`, `UE.Droid`, `ConsoleGame`, `UtopiaEngine.sln`) were removed once the Avalonia UI reached feature parity — the Avalonia port was written from scratch against `IGameEngine`, not migrated from them.

## Projects

- **UE.Core** — the engine: game logic, entities, repositories. UI-agnostic, no UI framework dependency.
- **UE.Nunit** — engine unit tests (the most complete part of the codebase; treat as the behavior spec). On NUnit 4 — classic asserts use `ClassicAssert.*` (from `NUnit.Framework.Legacy`, surfaced via `GlobalUsings.cs`).
- **UI/UE.UI** — shared Avalonia UI (views, view models, localization) driving the engine through `IGameEngine`.
- **UI/UE.UI.Desktop**, **UI/UE.UI.Android**, **UI/UE.UI.Browser** — platform heads (Android/Browser untested on device/WASM, compile only).

## Architecture

The central design split is **GameDefinition (immutable rules) vs GameState (mutable per-playthrough), orchestrated by GameEngine**.

- `GameEngine` (`UE.Core/GameEngine.cs`) is the single entry point for all game logic — searching regions, combat, activating Constructs, linking, resting, scoring, win/lose conditions. UIs and tests should go through `IGameEngine`, never mutate `GameState` directly. It is constructed with an `IRepository` and an `IDiceRoller`.
- `GameDefinition` is deserialized from an **embedded** XML resource `Data\DefinitionStandard.xml` (the static rules: regions, constructs, components, links, treasures, events). `Quotes.xml` and `UIText.xml` are also embedded resources. `BaseRepository.LoadDefinition` maps a file-style path to a manifest resource name (`UE.Core.` + dotted path) — keep that in mind when referencing data files.
- `GameState` holds everything that changes during play and is what gets serialized for save/load. After deserialization it must be **re-hydrated**: `GameState.Hydrate(GameDefinition)` rewires object references (RegionState→Region, ConstructState→Construct, LinkState→Constructs, etc.) that aren't serialized. `GameEngine.Init(...)` then `LoadGameState(...)` is the load path.
- **Dice are injected** via `IDiceRoller` (`RandomDice`, `FixedDice`/`TwoDice`). Tests subclass `BaseEngineTest` and override `GetDiceRoller()` to make rolls deterministic — do the same when writing engine tests.
- **Repository pattern** for persistence: `BaseRepository` (abstract, owns definition/quote loading) → `XmlRepository` (plain file XML, used by tests and the UI). Autosave lives at `%AppData%/UtopiaEngine/autosave.xml` on desktop (see `UI/UE.UI/AppData.cs`).
- **MVVM via CommunityToolkit.Mvvm** in `UI/UE.UI`: `MainViewModel` is a shell driving page navigation (`CurrentPage` + status bar); each screen (Regions, Search, Combat, Constructs, Activation, Links, Camp, Help) has its own view model. The engine's `Architecture/Messages/*` types (`SearchResult`, `CombatResult`, `ActivationResult`, `LinkResult`, `TimePassed`, etc.) are the result/DTO objects the engine returns to the UI layer.
- `Architecture/Table` + `Column` model the game's number-placement grids (search boxes, construct activation tables, link connections). Engine methods like `WorkToActivate`, `WorkToLink`, `ApplySearch` operate on these and interpret results (`SearchResult`, `ActivationResult`, `LinkResult`). The UI factors dice placement into shared `DiceCellViewModel`/`DicePairViewModel` + `DicePairView`.
- Localization: two independent layers. Engine strings use `LocalizedText(s)` + `enumLanguage` (`Values-fr`, `UEResources/French` in the embedded data). UI strings are externalized to `UI/UE.UI/Localization/Strings.resx` (EN neutral) + `Strings.fr.resx`, both **generated** from `UI/UE.UI/Localization/gen_strings.py` — edit the script and rerun it, never the `.resx` files directly. Both layers follow `CurrentUICulture`; the in-app language selector persists the choice to `%AppData%/UtopiaEngine/language.txt`.

## Conventions specific to this repo

- When adding engine behavior, add/extend an NUnit test in `UE.Nunit` driven through `IGameEngine` with a deterministic dice roller — that suite is the de-facto specification.
- New game data goes in the embedded XML under `UE.Core/Data` (and must stay an `EmbeddedResource` in `UE.Core.csproj`); don't read it from the filesystem.
- New persistent `GameState` fields must round-trip through XML serialization **and** be wired up in `Hydrate` if they hold references into `GameDefinition`.
