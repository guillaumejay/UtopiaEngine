namespace UE.Core.Interfaces
{
    public interface IDiceRoller
    {
        int Get1d6();
        TwoDice Get2d6();

        int  GetVariableDice(int nbSide);
    }
}
