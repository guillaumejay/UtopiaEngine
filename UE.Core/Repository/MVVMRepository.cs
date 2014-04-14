using System;
using System.IO;
using System.Xml.Serialization;
using Cirrious.MvvmCross.Plugins.File;
using System.Reflection;
using UE.Core.Entities;
using UE.Core.Interfaces;

namespace UE.Core.Repository
{
    public class MVVMRepository:BaseRepository
    {

        
        private IMvxFileStore _fileStore;

        public MVVMRepository(IMvxFileStore fileStore)
        {
            _fileStore = fileStore;
        }

    

        public override void SaveGameState(string save, GameState gameState)
        {
            throw new NotImplementedException();
        }

        public override GameState LoadGameState(string file)
        {
            throw new NotImplementedException();
        }


        public  override bool IsAutoSaveAvailable
        {
            get { return _fileStore.Exists(_autosaveFileName); }
        }



        public override bool DeleteAutoSave()
        {
            if (!_fileStore.Exists(_autosaveFileName))
                return false;
            _fileStore.DeleteFile(_autosaveFileName);
            return true;
        }
    }
}
