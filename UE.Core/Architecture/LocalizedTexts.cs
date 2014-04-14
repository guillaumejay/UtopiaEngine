using System.Collections.Generic;
using System.Linq;

namespace UE.Core.Architecture
{
    public class LocalizedTexts : List<LocalizedText>
    {
        public string TextFor(string culture)
        {
            return this.Single(x => x.Culture == culture).Text;
        }

        public string TextFor(EnumLanguage enumLanguage)
        {
            string culture = enumLanguage.ToString();
            return  TextFor( culture);
        }

        public string Text { get { return TextFor(Tools.Language); } }
    }
}
