using System.Xml.Serialization;

namespace UE.Core.Entities
{
    public enum Ability
    {
        [XmlEnum("BetterDefense")]
        BetterDefense,
        [XmlEnum("BetterAttack")]
        BetterAttack,
        [XmlEnum("ChargeGodsHandOnActivation")]
        ChargeGodsHandOnActivation,
        [XmlEnum("IgnoreEncounter")]
        IgnoreEncounter,
        [XmlEnum("RecoverHP")]
        RecoverHP,
        [XmlEnum("AutomaticallyConnect")]
        AutomaticallyConnect,
        [XmlEnum("RecoverFaster")]
        RecoverFaster,
        [XmlEnum("ActiveMonsters")]
        ActiveMonsters,
        [XmlEnum("FleetingVisions")]
        FleetingVisions,
        [XmlEnum("GoodFortune")]
        GoodFortune,
        [XmlEnum("FoulWeather")]
        FoulWeather,
        [XmlEnum("HelpAgainstSpirit")]
        HelpAgainstSpirit,
        [XmlEnum("HelpSearch34")]
       HelpSearch34,
        [XmlEnum("HelpSearch16")]
        HelpSearch16
    }
}
