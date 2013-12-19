namespace Droog.Calculon {

    public interface IActor {
        IScene Scene { get; set; }
    }

    public abstract class AActor: IActor {
        public IScene Scene { get; set; }
    }
}