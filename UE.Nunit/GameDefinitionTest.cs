using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using UE.Core;
using UE.Core.Architecture;
using UE.Core.Entities;

namespace UE.NUnit
{
  [TestFixture]
    public class GameDefinitionTest
    {

        private GameEngine gameEngine;
        public GameDefinitionTest()
        {
            gameEngine = new GameEngine(new XmlRepository(), new RandomDice());
        }

        [Test]
        public void DefinitionsAreLoadedCorrectly()
        {
            gameEngine.Init("Data\\DefinitionStandard.xml");
            ClassicAssert.AreEqual(1, gameEngine.GameDefinition.ID);
            ClassicAssert.AreEqual(6, gameEngine.GameDefinition.MaxHitPoint);
            ClassicAssert.AreEqual(22, gameEngine.GameDefinition.MaximumNumberOfDays);
            ClassicAssert.AreEqual(8, gameEngine.GameDefinition.NumberOfSkulls);
            ClassicAssert.IsFalse(gameEngine.IsGameLost);
            int nbRegions = 6;
            ClassicAssert.AreEqual(nbRegions, gameEngine.GameDefinition.Regions.Count);
            ClassicAssert.IsTrue(gameEngine.GameDefinition.Regions.All(x => x.Index != 0));
            ClassicAssert.AreEqual(nbRegions, gameEngine.GameDefinition.Regions.Count(x => Enumerable.Any(x.Name, y => y.Culture == "FR")));
            ClassicAssert.AreEqual(nbRegions, gameEngine.GameDefinition.Regions.Count(x => Enumerable.Any<LocalizedText>(x.Name, y => y.Culture == "US")));
            ClassicAssert.AreEqual(5 * nbRegions, gameEngine.GameDefinition.Regions.SelectMany(x => x.Encounters).Count());
            ClassicAssert.AreEqual(7, gameEngine.GameDefinition.Regions.SelectMany(x => x.Encounters.Where(y => y.IsSpirit)).Count());
            ClassicAssert.AreEqual(4,gameEngine.GameDefinition.Events.Count);
            ClassicAssert.IsTrue(gameEngine.GameDefinition.Regions.All(x => x.Construct != null));
            ClassicAssert.IsTrue(gameEngine.GameDefinition.Regions.All(x => x.Construct.Name.Any(y => y.Culture == "FR")));
            ClassicAssert.IsTrue(gameEngine.GameDefinition.Regions.All(x => x.Construct.Name.Any(y => y.Culture == "US")));
            ClassicAssert.IsTrue(gameEngine.GameDefinition.Quotes.Count >= 1);
        }

        [Test]
        public void LinksAreCorrect()
        {
            gameEngine.Init("Data\\DefinitionStandard.xml");
            ClassicAssert.AreEqual(6, gameEngine.GameDefinition.Links.Count);
            Link l = gameEngine.GameDefinition.Links.Single(x => x.ID == 1);
            ClassicAssert.AreEqual(1, l.ConstructID1);
            ClassicAssert.AreEqual(2, l.ConstructID2);
        }

        [Test]
        public void DayTrackerIsCorrect()
        {
            gameEngine.Init("Data\\DefinitionStandard.xml");
            Region reg = gameEngine.GameDefinition.Regions.Single(x => x.Index == 2);
            ClassicAssert.AreEqual(1, reg.DayPenaltyFor(0));
            ClassicAssert.AreEqual(0, reg.DayPenaltyFor(1));
        }

        [Test]
        public void DayIsEventTest()
        {
            gameEngine.Init("Data\\DefinitionStandard.xml");
            ClassicAssert.IsFalse(gameEngine.GameDefinition.IsEventDay(0));
            ClassicAssert.IsTrue(gameEngine.GameDefinition.IsEventDay(1));
        }

        [Test]
        public void ConstructsAreCorrect()
        {
            gameEngine.Init("Data\\DefinitionStandard.xml");
            ClassicAssert.IsTrue(gameEngine.GameDefinition.Regions.All(x => x.Construct != null));
            IEnumerable<Construct> constructs = gameEngine.GameDefinition.Regions.Select(x => x.Construct);
            ClassicAssert.IsTrue(gameEngine.GameDefinition.Components.All(x => x.Name.Any(y => y.Culture == "FR")));
            ClassicAssert.IsTrue(gameEngine.GameDefinition.Components.All(x => x.Name.Any(y => y.Culture == "US")));
            ClassicAssert.AreEqual(Ability.RecoverFaster, constructs.Single(x => x.ID == 3).Ability);
        }

        [Test]
        public void ComponentsAreCorrect()
        {
            gameEngine.Init("Data\\DefinitionStandard.xml");
            ClassicAssert.AreEqual(6, gameEngine.GameDefinition.Components.Count);
            ClassicAssert.IsTrue(gameEngine.GameDefinition.Components.All(x => x.Name.Any(y => y.Culture == "FR")));
            ClassicAssert.IsTrue(gameEngine.GameDefinition.Components.All(x => x.Name.Any(y => y.Culture == "US")));
            foreach (Region r in gameEngine.GameDefinition.Regions)
            {
                ClassicAssert.IsNotNull(r.Component);
                ClassicAssert.AreEqual(r.Index, r.Component.IndexRegion);
            }
        }

        [Test]
        public void LegendaryTreasuresAreCorrect()
        {
            gameEngine.Init("Data\\DefinitionStandard.xml");
            ClassicAssert.AreEqual(gameEngine.GameDefinition.NumberOfRegions, gameEngine.GameDefinition.LegendaryTreasures.Count);
            ClassicAssert.IsTrue(gameEngine.GameDefinition.LegendaryTreasures.All(x => x.Name.Any(y => y.Culture == "FR")));
            ClassicAssert.IsTrue(gameEngine.GameDefinition.LegendaryTreasures.All(x => x.Name.Any(y => y.Culture == "US")));
            foreach (Region r in gameEngine.GameDefinition.Regions)
            {
                ClassicAssert.IsNotNull(r.LegendaryTreasure);
                ClassicAssert.AreEqual(r.Index, r.LegendaryTreasure.IndexRegion);
            }
            ClassicAssert.AreEqual(Ability.BetterDefense, gameEngine.GameDefinition.LegendaryTreasures.Single(x => x.ID == 1).Ability);
            ClassicAssert.AreEqual(gameEngine.GameDefinition.NumberOfRegions, gameEngine.GameDefinition.LegendaryTreasures.Select(x => x.Ability).Distinct().Count());
        }

        [Test]
        public void EncounterDefinitionsAreCorrect()
        {
            gameEngine.Init("Data\\DefinitionStandard.xml");
            Region r = gameEngine.GameDefinition.Regions.Single(x => x.Index == 1);
            Encounter e = r.GetEncounter(5);
            ClassicAssert.AreEqual("1-4", e.AttackText);
            ClassicAssert.IsTrue(e.ToString().Contains("Pics"));
            r = gameEngine.GameDefinition.Regions.Single(x => x.Index == 4);
            e = r.GetEncounter(3);
            ClassicAssert.AreEqual("1-2", e.AttackText);
            ClassicAssert.IsTrue(e.ToString().Contains("(S)"));
            r = gameEngine.GameDefinition.Regions.Single(x => x.Index == 5);
            e = r.GetEncounter(1);
            ClassicAssert.AreEqual("1", e.AttackText);
            ClassicAssert.AreEqual("5-6", e.HitText);
        }
    }
}
