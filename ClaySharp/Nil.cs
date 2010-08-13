using ClaySharp.Behaviors;

namespace ClaySharp {
    public static class Nil  {
        static readonly object Singleton = new Thing(new NilBehavior(), new InterfaceProxyBehavior());
        public static object Instance { get { return Singleton; } }
    }
}
