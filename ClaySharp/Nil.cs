using ClaySharp.Behaviors;

namespace ClaySharp {
    public static class Nil  {
        static readonly object Singleton = new Clay(new NilBehavior(), new InterfaceProxyBehavior());
        public static object Instance { get { return Singleton; } }
    }
}
