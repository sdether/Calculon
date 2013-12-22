using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Droog.Calculon.Backstage {
    public class MessageResponse {

        private static readonly Dictionary<Type, ReflectedTaskCompletionSource> _typeCache = new Dictionary<Type, ReflectedTaskCompletionSource>();

        public readonly Guid Id;
        private readonly object _completion;
        private readonly Type _type;
        private readonly ReflectedTaskCompletionSource _tcs;

        public MessageResponse(Type responseType) {
            lock(_typeCache) {
                if(!_typeCache.TryGetValue(responseType, out _tcs)) {
                    _typeCache[responseType] = _tcs = new ReflectedTaskCompletionSource(responseType);
                }
            }
            Id = Guid.NewGuid();
            _completion = _tcs.CreateInstance();
        }

        public Task Task { get { return _tcs.GetTask(_completion); } }

        public void Fault(Exception exception) {
            _tcs.Fault(_completion, exception);
        }

        public void Complete(object value) {
            _tcs.Complete(_completion, value);
        }
    }

    public class ReflectedTaskCompletionSource {
        private readonly Type _type;
        private readonly MethodInfo _taskGetter;
        private readonly MethodInfo _faultSetter;
        private readonly MethodInfo _resultSetter;
        public ReflectedTaskCompletionSource(Type responseType) {
            var generic = typeof(TaskCompletionSource<>);
            _type = generic.MakeGenericType(responseType ?? typeof(object));
            _taskGetter = _type.GetProperty("Task").GetGetMethod();
            _resultSetter = _type.GetMethod("SetResult");
            _faultSetter = _type.GetMethods().First(x => x.Name == "SetException" && x.GetParameters().First().ParameterType == typeof(Exception));
        }

        public object CreateInstance() {
            return Activator.CreateInstance(_type);
        }

        public Task GetTask(object instance) {
            return _taskGetter.Invoke(instance, null) as Task;
        }

        public void Fault(object instance, Exception e) {
            _faultSetter.Invoke(instance, new object[] { e });
        }

        public void Complete(object instance, object value) {
            _resultSetter.Invoke(instance, new[] { value });
        }
    }
}