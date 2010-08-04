using DynShape.Behaviors;

namespace DynShape {
    public static class Nil  {
        static readonly object _singleton = new Thing(new NilBehavior());
        public static object Instance { get { return _singleton; } }
    }
}
