using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UE.Core.Entities;
using UE.Core.Interfaces;

namespace UE.Core.Repository
{
    public abstract class BaseRepository:IRepository
    {
        protected virtual string _autosaveFileName { get { return "autosave.xml"; } }

        public virtual void SaveAutoSave(GameState gameState)
        {
            SaveGameState(_autosaveFileName, gameState);
        }

        public virtual GameState LoadAutoSave()
        {
            return LoadGameState(_autosaveFileName);
        }

        public GameDefinition LoadDefinition(string gameDef,string quoteFile="Data\\Quotes.xml")
        {
            Assembly assembly;
            assembly = Assembly.GetExecutingAssembly(); ;
            XmlSerializer serializer = new XmlSerializer(typeof(GameDefinition));
            string resourceName = GetResourceName(gameDef);
            GameDefinition def;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    def = (GameDefinition)serializer.Deserialize(reader);


                }
            }
            def.Quotes = LoadQuotes(assembly,quoteFile);
            return def;
        }

        private static string GetResourceName(string filePath)
        {
            return "UE.Core." + filePath.Replace('/', '.').Replace('\\', '.');
        }

        private List<Quote> LoadQuotes(Assembly assembly, string quoteFile)
        {
            List<Quote> quotes;
            XmlSerializer serializer = new XmlSerializer(typeof(List<Quote>));
            string resourceName = GetResourceName(quoteFile);
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    quotes = (List<Quote>)serializer.Deserialize(reader);


                }
            }
            return quotes;
        }


        public abstract void SaveGameState(string save, GameState gameState);

        public abstract GameState LoadGameState(string file);

        public abstract bool IsAutoSaveAvailable { get; }

        public abstract bool DeleteAutoSave();
    
    }
}
