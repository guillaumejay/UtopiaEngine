using System.Threading;
using UE.Core.Architecture;

namespace UE.Core
{
    static class Tools
    {
        public static EnumLanguage Language
        {
            get
            {
                string tmp = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpper();
                EnumLanguage ret;
                if (tmp != "FR")
                    ret = EnumLanguage.US;
                else
                {
                    ret = EnumLanguage.FR;
                }
                return ret;
            }

        }
    }
}
