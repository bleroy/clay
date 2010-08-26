using ClaySharp.Behaviors;

namespace ClaySharp {
    public class ClayFactory : Clay {
        public ClayFactory() : base(new ClayFactoryBehavior(), new ArrayFactoryBehavior()) {
        }
    }
}
