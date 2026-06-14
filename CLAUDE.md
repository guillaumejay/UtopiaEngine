# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A C#/.NET digital port (with the author's permission) of the solo board game **Utopia Engine** by Nick Hayes. The goal is a cross-platform game built on a shared, UI-agnostic engine. Rules reference lives in `Documentation/` (PDF rulebook + adventure sheets, and a markdown rulebook under `Documentation/rulebook/`).

## Toolchain & build

The repo is **mid-migration** and has two stacks. Pick the right solution for what you touch:

- **Engine + tests → modern (`net10.0`, SDK-style).** `UE.Core` (engine only) and `UE.Nunit` target `net10.0`, grouped in **`UtopiaEngine.Net.slnx`**.
  - Build: `dotnet build UtopiaEngine.Net.slnx`
  - Test: `dotnet test UtopiaEngine.Net.slnx` (uses **NUnit 4** + `NUnit3TestAdapter` + `Microsoft.NET.Test.Sdk`).
  - Single test: `dotnet test UtopiaEngine.Net.slnx --filter "FullyQualifiedName~ScoreTest"` (or `Name~WinningAddsBonus`).
  - The net10 `UE.Core` is **MvvmCross-free**: `App.cs`, `ViewModels/*`, `Repository/MVVMRepository.cs`, `Converters/TableConverter.cs` are still on disk but **excluded** from compilation via `<Compile Remove>` (kept for the future UI layer).
- **UI heads → legacy (.NET 4.x, MvvmCross 3.1.1).** `UE.Console`, `UE.WPF`, `UE.Droid`, `ConsoleGame` remain in **`UtopiaEngine.sln`**, built with **Visual Studio / MSBuild** (NuGet `packages.config`, restore via `.nuget/NuGet.targets`). These are currently **orphaned** from the net10 `UE.Core` and will be re-wired to a modern MvvmCross later. `UE.Droid` needs the Xamarin toolchain; `UE.WPF` needs Windows.

## Projects

- **UE.Core** — the heart: game engine, entities, repositories (and, excluded-from-build, the MvvmCross ViewModels/converter/app). The net10 build is the pure, UI-agnostic engine.
- **UE.Nunit** — engine unit tests (the most complete part of the codebase; treat as the behavior spec). On NUnit 4 — classic asserts use `ClassicAssert.*` (from `NUnit.Framework.Legacy`, surfaced via `GlobalUsings.cs`).
- **UE.Console** — fully playable text UI (MvvmCross console host). Best reference for how to drive the engine end-to-end (legacy).
- **ConsoleGame** — older/scratch console experiment, not the MvvmCross app (legacy).
- **UE.Droid** / **UE.WPF** — incomplete UI front-ends (the README notes the author is slow on UI) (legacy).

## Architecture

The central design split is **GameDefinition (immutable rules) vs GameState (mutable per-playthrough), orchestrated by GameEngine**.

- `GameEngine` (`UE.Core/GameEngine.cs`) is the single entry point for all game logic — searching regions, combat, activating Constructs, linking, resting, scoring, win/lose conditions. UIs and tests should go through `IGameEngine`, never mutate `GameState` directly. It is constructed with an `IRepository` and an `IDiceRoller`.
- `GameDefinition` is deserialized from an **embedded** XML resource `Data\DefinitionStandard.xml` (the static rules: regions, constructs, components, links, treasures, events). `Quotes.xml` and `UIText.xml` are also embedded resources. `BaseRepository.LoadDefinition` maps a file-style path to a manifest resource name (`UE.Core.` + dotted path) — keep that in mind when referencing data files.
- `GameState` holds everything that changes during play and is what gets serialized for save/load. After deserialization it must be **re-hydrated**: `GameState.Hydrate(GameDefinition)` rewires object references (RegionState→Region, ConstructState→Construct, LinkState→Constructs, etc.) that aren't serialized. `GameEngine.Init(...)` then `LoadGameState(...)` is the load path.
- **Dice are injected** via `IDiceRoller` (`RandomDice`, `FixedDice`/`TwoDice`). Tests subclass `BaseEngineTest` and override `GetDiceRoller()` to make rolls deterministic — do the same when writing engine tests.
- **Repository pattern** for persistence: `BaseRepository` (abstract, owns definition/quote loading) → `XmlRepository` (used by tests, plain file XML) and `MVVMRepository` (platform file store via MvvmCross `IMvxFileStore`; save/load currently `NotImplementedException`). Autosave lives at `autosave.xml`.
- **MVVM via MvvmCross**: `UE.Core/ViewModels` (`BaseViewModel : MvxViewModel`, plus Title/Main/StartSearch/SearchRegion VMs) are shared; each UI project provides `Setup`/views and platform bootstrap plugins. The `Architecture/Messages/*` types (`SearchResult`, `CombatResult`, `ActivationResult`, `LinkResult`, `TimePassed`, etc.) are the result/DTO objects the engine returns to the UI layer.
- `Architecture/Table` + `Column` model the game's number-placement grids (search boxes, construct activation tables, link connections). Engine methods like `WorkToActivate`, `WorkToLink`, `ApplySearch` operate on these and interpret results (`SearchResult`, `ActivationResult`, `LinkResult`).
- Localization: `LocalizedText(s)` + `enumLanguage`; French resources exist (`Values-fr`, `UEResources/French`).

## Conventions specific to this repo

- When adding engine behavior, add/extend an NUnit test in `UE.Nunit` driven through `IGameEngine` with a deterministic dice roller — that suite is the de-facto specification.
- New game data goes in the embedded XML under `UE.Core/Data` (and must stay an `EmbeddedResource` in `UE.Core.csproj`); don't read it from the filesystem.
- New persistent `GameState` fields must round-trip through XML serialization **and** be wired up in `Hydrate` if they hold references into `GameDefinition`.
