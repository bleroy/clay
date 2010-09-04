using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ClaySharp.Behaviors {
    public class ArrayBehavior : ClayBehavior {
        readonly List<object> _data = new List<object>();

        public override object GetIndex(Func<object> proceed, IEnumerable<object> keys) {
            return IfSingleInteger(keys, key => _data[key], proceed);
        }

        public override object SetIndex(Func<object> proceed, IEnumerable<object> keys, object value) {
            return IfSingleInteger(keys, key => _data[key] = value, proceed);
        }

        public override object GetMember(Func<object> proceed, object self, string name) {
            switch (name) {
                case "Length":
                case "Count":
                    return _data.Count;
                case "GetEnumerator":
                    return new Clay(new InterfaceProxyBehavior(), new EnumeratorBehavior(_data.GetEnumerator()));
            }
            return proceed();
        }


        public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
            switch (name) {
                case "AddRange":
                    _data.AddRange(((IEnumerable)args.Single()).OfType<object>());
                    return self;
                case "Add":
                    _data.AddRange(args);
                    return self;
                case "Insert":
                    return IfInitialInteger(args, (index, arr) => { _data.InsertRange(index, arr); return self; }, proceed);
                case "RemoveAt":
                    return IfSingleInteger(args, index => { _data.RemoveAt(index); return self; }, proceed);
                case "Contains":
                    return IfSingleArgument(args, arg => _data.Contains(arg), proceed);
                case "IndexOf":
                    return IfSingleArgument(args, arg => _data.IndexOf(arg), proceed);
                case "Remove":
                    return IfSingleArgument(args, arg => _data.Remove(arg), proceed);
                case "CopyTo":
                    return IfArguments<object[], int>(args, (array, arrayIndex) => {
                        _data.CopyTo(array, arrayIndex);
                        return self;
                    }, proceed);

            }

            if (!args.Any()) {
                return GetMember(proceed, self, name);
            }

            return proceed();
        }



        private static object IfArguments<T1, T2>(IEnumerable<object> args, Func<T1, T2, object> func, Func<object> proceed) {
            if (args.Count() != 2)
                return proceed();
            return func((T1)args.First(), (T2)args.Last());
        }


        private static object IfSingleArgument(IEnumerable<object> args, Func<object, object> func, Func<object> proceed) {
            return args.Count() == 1 ? func(args.Single()) : proceed();
        }

        private static object IfSingleInteger(IEnumerable<object> args, Func<int, object> func, Func<object> proceed) {
            if (args.Count() != 1)
                return proceed();

            return IfInitialInteger(args, (index, ignored) => func(index), proceed);
        }

        private static object IfInitialInteger(IEnumerable<object> args, Func<int, IEnumerable<object>, object> func, Func<object> proceed) {
            if (!args.Any())
                return proceed();

            var key = args.First();

            if (key.GetType() != typeof(int))
                return proceed();

            return func((int)key, args.Skip(1));
        }

    
    
        // small, dynamic wrapper around underlying IEnumerator. use of
        // dlr and dynamic proxy enables this enumerator to be used in places
        // where the array is being cast into a strong collection interface
        class EnumeratorBehavior : ClayBehavior {
            private readonly IEnumerator _enumerator;

            public EnumeratorBehavior(IEnumerator enumerator) {
                _enumerator = enumerator;
            }

            public override object InvokeMember(Func<object> proceed, object self, string name, INamedEnumerable<object> args) {
                switch (name) {
                    case "MoveNext":
                        return _enumerator.MoveNext();
                    case "Reset":
                        _enumerator.Reset();
                        return null;
                    case "Dispose":
                        if (_enumerator is IDisposable)
                            ((IDisposable)_enumerator).Dispose();
                        return null;
                }
                return proceed();
            }

            public override object GetMember(Func<object> proceed, object self, string name) {
                switch (name) {
                    case "Current":
                        return _enumerator.Current;
                }
                return proceed();
            }
        }}

}
