namespace etf.santorini.km150096d.model.interfaces
{
    public enum Height { H0 = 1, H1, H2, H3, ROOF };
    public interface ITile
    {
        Height Height { get; set; }
        IPlayer Player { get; set; }
        bool HasPlayer();
    }
}
