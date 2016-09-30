using WellFormedNames;

namespace PlayGen.RAGE.SportsTeamManager.Simulation
{
    public static class NameExtensions
    {
        public static Name ToName(this string value)
        {
            return Name.BuildName(value);
        }
    }
}
