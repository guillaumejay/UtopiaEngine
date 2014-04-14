using UE.Core.Entities;

namespace UE.Core.Interfaces
{
    public interface IRepository
    {
        GameDefinition LoadDefinition(string gameDef,string quoteFile);

        void SaveGameState(string save,GameState gameState);

        GameState LoadGameState(string file);

        bool IsAutoSaveAvailable { get; }

        void SaveAutoSave(GameState gameState);

        GameState LoadAutoSave();

        /// <summary>
        /// Delete the autosave file
        /// </summary>
        /// <returns>true if the file has really been deleted</returns>
        bool DeleteAutoSave();
    }
}
