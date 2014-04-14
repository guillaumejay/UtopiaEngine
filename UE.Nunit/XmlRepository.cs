using System.IO;
using System.Xml.Serialization;
using UE.Core.Entities;
using UE.Core.Repository;

namespace UE.NUnit
{
    public class XmlRepository:BaseRepository
    {
       

        public override void SaveGameState(string save, GameState gameState)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GameState));
            using (Stream writer = new FileStream(save, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(writer, gameState);
            }
        }


        public override GameState LoadGameState(string file)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GameState));
            GameState def;

            using (Stream reader = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                def = (GameState)serializer.Deserialize(reader);
            }
            return def;
        }


        public override bool IsAutoSaveAvailable
        {
            get { throw new System.NotImplementedException(); }
        }


        public override  bool DeleteAutoSave()
        {
            throw new System.NotImplementedException();
        }
    }
}
