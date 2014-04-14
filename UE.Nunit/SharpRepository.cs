using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Entities;
using Engine.Interfaces;
using Polenter.Serialization;

namespace UE.NUnit
{
    class SharpRepository : IRepository
    {
        public GameDefinition LoadDefinition(string gameDef)
        {
            var serializer = new SharpSerializer();

            GameDefinition def;

            def = (GameDefinition)serializer.Deserialize(gameDef);

            return def;
        }

        public void SaveGameState(string save, Engine.Entities.GameState gameState)
        {
            throw new NotImplementedException();
        }

        public Engine.Entities.GameState LoadGameState(string file)
        {
            throw new NotImplementedException();
        }
    }
}
