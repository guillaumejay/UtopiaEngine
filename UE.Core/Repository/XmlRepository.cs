using System.IO;
using System.Xml.Serialization;
using UE.Core.Entities;

namespace UE.Core.Repository
{
    /// <summary>
    /// File-based XML persistence for game states. UI-agnostic; usable by any front-end.
    /// </summary>
    public class XmlRepository : BaseRepository
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
            using (Stream reader = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                return (GameState)serializer.Deserialize(reader);
            }
        }

        public override bool IsAutoSaveAvailable
        {
            get { return File.Exists(_autosaveFileName); }
        }

        public override bool DeleteAutoSave()
        {
            if (!File.Exists(_autosaveFileName))
                return false;
            File.Delete(_autosaveFileName);
            return true;
        }
    }
}
